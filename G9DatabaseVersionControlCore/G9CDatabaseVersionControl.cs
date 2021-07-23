using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using G9DatabaseVersionControlCore.Class.SmallLogger;
using G9DatabaseVersionControlCore.Class.SmallLogger.Interface;
using G9DatabaseVersionControlCore.DataType;
using G9DatabaseVersionControlCore.DataType.AjaxDataType;
using G9DatabaseVersionControlCore.DataType.AjaxDataType.StepDataType;
using G9DatabaseVersionControlCore.Enums;

// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedMemberInSuper.Global

namespace G9DatabaseVersionControlCore
{
    /// <summary>
    ///     A core class for database version control
    /// </summary>
    public abstract partial class G9CDatabaseVersionControl
    {
        #region ### Fields And Properties ###

        /// <summary>
        ///     Specifies default product version (Assembly version)
        /// </summary>
        public static readonly string DefaultProductVersion =
#if (NETSTANDARD2_1 || NETSTANDARD2_0 || NETCOREAPP)
            string.IsNullOrEmpty(Assembly.GetExecutingAssembly().GetName().Version.ToString())
                ? Assembly.GetEntryAssembly()?.GetName().Version?.ToString() ?? "0.0.0.0"
                : Assembly.GetExecutingAssembly().GetName().Version.ToString();
#elif (NETSTANDARD1_6 || NETSTANDARD1_5)
        string.IsNullOrEmpty(Assembly.GetEntryAssembly().GetName().Version.ToString())
                ? Assembly.GetEntryAssembly()?.GetName().Version?.ToString() ?? "0.0.0.0"
                : Assembly.GetEntryAssembly().GetName().Version.ToString();
#else
            string.IsNullOrEmpty(Assembly.Load(new AssemblyName(nameof(G9CDatabaseVersionControl))).GetName().Version
                .ToString())
                ? Assembly.Load(new AssemblyName(nameof(G9CDatabaseVersionControl)))?.GetName().Version?.ToString() ??
                  "0.0.0.0"
                : Assembly.Load(new AssemblyName(nameof(G9CDatabaseVersionControl))).GetName().Version.ToString();
#endif

        /// <summary>
        ///     Specifies product version
        /// </summary>
        public string ProductVersion { protected set; get; }

        /// <summary>
        ///     Specifies default update files full path
        /// </summary>
        public const string DefaultDatabaseUpdateFilesFullPath = @"DatabaseUpdateFiles\";

        /// <summary>
        ///     Specifies default schema for create tables
        /// </summary>
        public const string DefaultSchemaForTables = "dbo";

        /// <summary>
        ///     Access to all projects and infos
        /// </summary>
        private Dictionary<string, G9DtProjectInfo> _totalProjects;

        /// <summary>
        ///     Specifies last status of task
        /// </summary>
        private G9DtTaskAnswer _lastTaskStatus;

        /// <summary>
        ///     Specifies current database version
        /// </summary>
        public string CurrentDatabaseVersion { protected set; get; }

        /// <summary>
        ///     Variable for store map items
        /// </summary>
        public G9DtMap ProjectMapData { protected set; get; }

        /// <summary>
        ///     Category for store total map data
        /// </summary>
        private static readonly Dictionary<string, G9DtMap> TotalMapData = new Dictionary<string, G9DtMap>();

        /// <summary>
        /// Save logger object
        /// </summary>
        private static G9ISmallLogger _logger;

        /// <summary>
        ///     Access to logger
        /// </summary>
        protected static G9ISmallLogger Logger
        {
            private set => _logger = value;
            get
            {
                if (_logger == null)
                    _logger = new G9CSmallLogger();
                return _logger;
            }
        }

        #endregion

        #region ### Methods ###

        /// <summary>
        ///     Constructor - Initialize Requirement
        /// </summary>
        /// <param name="projectName">Specifies project ns assigned map.</param>
        /// <param name="logger">Specifies custom logger (if null use default logger)</param>
        /// <exception cref="ArgumentException">
        ///     If not exist a map for this project name. The method throw exception about the map
        ///     not found.
        /// </exception>
        protected G9CDatabaseVersionControl(string projectName, G9ISmallLogger logger = null)
        {
            // Initialize log
            Logger = logger ?? new G9CSmallLogger();

            if (!TotalMapData.ContainsKey(projectName))
                throw new ArgumentException(
                    $"Map with this project name not found. please use static method '{nameof(MapProjects)}' or use the constructor with map param.");
            ProjectMapData = TotalMapData[projectName];
            ProductVersion = ProjectMapData.ProductVersionFunc();
            CacheTotalData();
        }

        /// <summary>
        ///     Constructor - Initialize Requirement
        /// </summary>
        /// <param name="map">Specifies a map for assign to project.</param>
        /// <param name="logger">Specifies custom logger (if null use default logger)</param>
        protected G9CDatabaseVersionControl(G9DtMap map, G9ISmallLogger logger = null)
        {
            // Initialize log
            Logger = logger ?? new G9CSmallLogger();
            ProjectMapData = map;
            ProductVersion = ProjectMapData.ProductVersionFunc();
            MapProjects(ProjectMapData);
            CacheTotalData();
        }

        /// <summary>
        ///     Method to cache total data
        /// </summary>
        private void CacheTotalData()
        {
            try
            {
                _totalProjects = new Dictionary<string, G9DtProjectInfo>();

                // Get total project update files full path
                var totalProjectPathData =
                    TotalMapData.Select(s => s.Value.DatabaseUpdateFilesFullPath).Distinct().ToArray();

                foreach (var projectFullPath in totalProjectPathData)
                {
                    var projectsInPath = GetFolderNamesFromPath(projectFullPath).Select(GetFullPathFromPath).ToArray();
                    foreach (var projectPath in projectsInPath)
                    {
                        var projectName = RemovePreFixPath(projectPath);
                        // Get project map
                        var map = TotalMapData.Where(s => s.Key == projectName).Select(s => s.Value)
                            .FirstOrDefault();

                        var projectUpdateFolders = GetFolderNamesFromPath(projectPath);
                        var foldersInfo = new List<G9DtUpdateFoldersInfo>();
                        foreach (var projectUpdateFolder in projectUpdateFolders)
                        {
                            var updateFilesInfo = new List<G9DtUpdateFilesInfo>();
                            var projectUpdateFileNames =
                                GetFilesFromPath(Path.Combine(projectPath, projectUpdateFolder));

                            foreach (var projectUpdateFileName in projectUpdateFileNames)
                            {
                                var fileData = File.ReadAllText(projectUpdateFileName,
                                    ProjectMapData.DatabaseUpdateScriptFileEncoding);
                                var author = GetDataBetweenTwoXmlTag(fileData, "Author");
                                var description = GetDataBetweenTwoXmlTag(fileData, "Description");
                                var version = GetDataBetweenTwoXmlTag(fileData, "Version");
                                DateTime updateDateTime;
                                try
                                {
                                    var datetime = GetDataBetweenTwoXmlTag(fileData, "UpdateDateTime");
                                    updateDateTime = string.IsNullOrEmpty(datetime)
                                        ? DateTime.MinValue
                                        : DateTime.Parse(datetime);
                                }
                                catch (Exception e)
                                {
                                    Logger.G9SmallLogException(e);
                                    // Ignore
                                    updateDateTime = DateTime.MinValue;
                                }

                                // Check validation
                                if (!map.Equals(default(G9DtMap)))
                                {
                                    if (map.DatabaseScriptRequirements.IsRequiredToSetAuthor &&
                                        string.IsNullOrEmpty(author))
                                        throw new Exception(
                                            $"Requirement Error! The 'Author' is not set in the update script file! Script path: {projectUpdateFileName}");

                                    if (map.DatabaseScriptRequirements.IsRequiredToSetDescription &&
                                        string.IsNullOrEmpty(description))
                                        throw new Exception(
                                            $"Requirement Error! The 'Description' is not set in the update script file! Script path: {projectUpdateFileName}");

                                    if (map.DatabaseScriptRequirements.IsRequiredToSetVersion &&
                                        (string.IsNullOrEmpty(version) || !int.TryParse(version.Replace(".", string.Empty), out _)))
                                        throw new Exception(
                                            $"Requirement Error! The 'Version' is not set or incorrect in the update script file! Script path: {projectUpdateFileName}");

                                    if (map.DatabaseScriptRequirements.IsRequiredToSetUpdateDateTime &&
                                        updateDateTime == DateTime.MinValue)
                                        throw new Exception(
                                            $"Requirement Error! The 'UpdateDateTime' is not set or incorrect in the update script file! Script path: {projectUpdateFileName}");
                                }

                                updateFilesInfo.Add(new G9DtUpdateFilesInfo(GetFullPathFromPath(projectUpdateFileName),
                                    RemovePreFixPath(projectUpdateFileName), author, description, updateDateTime,
                                    version,
                                    fileData));
                            }

                            var folderName = RemovePreFixPath(projectUpdateFolder);
                            string folderVersion;
                            DateTime folderDateTime;
                            try
                            {
                                folderVersion = folderName.Substring(0, 7);
                                if (!int.TryParse(folderVersion.Replace(".", string.Empty), out _))
                                    throw new Exception("Incorrect folder name!");
                                folderDateTime =
                                    DateTime.Parse(
                                        $"{folderName.Substring(8, 4)}/{folderName.Substring(12, 2)}/{folderName.Substring(14, 2)}",
                                        CultureInfo.InvariantCulture);
                            }
                            catch (Exception e)
                            {
                                var exception = new Exception(
                                    $"There is a problem naming the update folders.\nIncorrect name: '{folderName}'\nStandard name: '1.0.0.0-20210601'",
                                    e);
                                Logger.G9SmallLogException(exception);
                                throw exception;
                            }

                            foldersInfo.Add(new G9DtUpdateFoldersInfo(GetFullPathFromPath(projectUpdateFolder),
                                folderName,
                                folderVersion
                                , folderDateTime, updateFilesInfo));
                        }

                        _totalProjects.Add(projectName,
                            new G9DtProjectInfo(GetFullPathFromPath(projectPath), RemovePreFixPath(projectPath), null,
                                DateTime.MinValue, foldersInfo));
                    }
                }
            }
            catch (Exception e)
            {
                Logger.G9SmallLogException(e, "Error On Cache Data");
                throw;
            }
        }

        /// <summary>
        ///     Method to reset cache data
        /// </summary>
        public void ResetCacheData()
        {
            CacheTotalData();
        }

        /// <summary>
        ///     Method to get update files info
        /// </summary>
        /// <returns>Return update files info</returns>
        public IList<G9DtUpdateFilesInfo> GetUpdateFiles()
        {
            try
            {
                // Check exist project name
                if (!_totalProjects.ContainsKey(ProjectMapData.ProjectName))
                    throw new Exception(
                        $"Project with this name is not exist!\nProject name is '{ProjectMapData.ProjectName}'.");
                int versionInt;
                // convert version name
                try
                {
                    versionInt = int.Parse(CurrentDatabaseVersion.Replace(".", string.Empty));
                }
                catch (Exception e)
                {
                    throw new ArgumentException(
                        $"The parameter value is incorrect!\nIncorrect value: '{CurrentDatabaseVersion}'\nStandard value: '1.0.0.0'",
                        nameof(CurrentDatabaseVersion), e);
                }

                return _totalProjects[ProjectMapData.ProjectName].UpdateFoldersInfo
                    .Where(s => int.Parse(s.FolderVersion.Replace(".", string.Empty)) > versionInt)
                    .SelectMany(s => s.UpdateFilesInfos).ToArray();
            }
            catch (Exception e)
            {
                Logger.G9SmallLogException(e);
                throw;
            }
        }

        /// <summary>
        ///     Method to get update folders info
        /// </summary>
        /// <returns>Return update folders info</returns>
        public IList<G9DtUpdateFoldersInfo> GetUpdateFolders()
        {
            try
            {
                // Check exist project name
                if (!_totalProjects.ContainsKey(ProjectMapData.ProjectName))
                    throw new Exception(
                        $"Project whit this name not exist!\nProject name is '{ProjectMapData.ProjectName}'.");
                int versionInt;
                // convert version name
                try
                {
                    versionInt = int.Parse(CurrentDatabaseVersion.Replace(".", string.Empty));
                }
                catch (Exception e)
                {
                    throw new ArgumentException(
                        $"The parameter value is incorrect!\nIncorrect value: '{CurrentDatabaseVersion}'\nStandard value: '1.0.0.0'",
                        nameof(CurrentDatabaseVersion), e);
                }

                return _totalProjects[ProjectMapData.ProjectName].UpdateFoldersInfo
                    .Where(s => int.Parse(s.FolderVersion.Replace(".", string.Empty)) > versionInt).ToArray();
            }
            catch (Exception e)
            {
                Logger.G9SmallLogException(e);
                throw;
            }
        }

        /// <summary>
        ///     Method to get count of update files info
        /// </summary>
        /// <returns>Return count of update files</returns>
        public int GetCountOfUpdateFiles()
        {
            try
            {
                // Check exist project name
                if (!_totalProjects.ContainsKey(ProjectMapData.ProjectName))
                    throw new Exception(
                        $"Project whit this name not exist!\nProject name is '{ProjectMapData.ProjectName}'.");
                int versionInt;
                // convert version name
                try
                {
                    versionInt = int.Parse(CurrentDatabaseVersion.Replace(".", string.Empty));
                }
                catch (Exception e)
                {
                    throw new ArgumentException(
                        $"The parameter value is incorrect!\nIncorrect value: '{CurrentDatabaseVersion}'\nStandard value: '1.0.0.0'",
                        nameof(CurrentDatabaseVersion), e);
                }

                return _totalProjects[ProjectMapData.ProjectName].UpdateFoldersInfo
                    .Where(s => int.Parse(s.FolderVersion.Replace(".", string.Empty)) > versionInt)
                    .SelectMany(s => s.UpdateFilesInfos).Count();
            }
            catch (Exception e)
            {
                Logger.G9SmallLogException(e);
                throw;
            }
        }

        /// <summary>
        ///     Method to check update exist
        /// </summary>
        /// <returns></returns>
        public bool CheckUpdateExist()
        {
            return GetCountOfUpdateFiles() > 0;
        }

        /// <summary>
        ///     Method to get project names
        /// </summary>
        /// <returns>Array of project names</returns>
        public IList<string> GetProjectNames()
        {
            return GetTotalProjectNames();
        }

        /// <summary>
        ///     Method to get project names
        /// </summary>
        /// <returns>Array of project names</returns>
        public static IList<string> GetTotalProjectNames(string databaseUpdateFilesFullPath = null)
        {
            try
            {
                return RemovePreFixPath(
                    GetFolderNamesFromPath(databaseUpdateFilesFullPath ?? DefaultDatabaseUpdateFilesFullPath));
            }
            catch (Exception e)
            {
                Logger.G9SmallLogException(e);
                throw;
            }
        }

        /// <summary>
        ///     Method to get folders name from a path
        /// </summary>
        /// <param name="path">Specify a path to get folder names</param>
        /// <returns>Array of folder names</returns>
        private static IList<string> GetFolderNamesFromPath(string path)
        {
            try
            {
                // Check Path
                if (!Directory.Exists(path))
                    throw new Exception($"Path '{path}' not exist!");
                // Get folders name
                var directories = Directory.GetDirectories(path);
                Array.Sort((Array) directories);
                return directories;
            }
            catch (Exception e)
            {
                Logger.G9SmallLogException(e);
                throw;
            }
        }

        /// <summary>
        ///     Method to get file names from a path
        /// </summary>
        /// <param name="path">Specify a path to get file names</param>
        /// <returns>Array of file names</returns>
        private IList<string> GetFilesFromPath(string path)
        {
            try
            {
                // Check Path
                if (!Directory.Exists(path))
                    throw new Exception($"Path '{path}' not exist!");
                // Get folders name
                var files = Directory.GetFiles(path);
                Array.Sort((Array) files);
                return files;
            }
            catch (Exception e)
            {
                Logger.G9SmallLogException(e);
                throw;
            }
        }

        /// <summary>
        ///     Method to remove pre fix from path
        /// </summary>
        /// <param name="pathArray">Specifies path array</param>
        /// <returns>Get last name from path</returns>
        private static IList<string> RemovePreFixPath(IEnumerable<string> pathArray)
        {
            return pathArray.Select(s =>
                s.Remove(0, s.LastIndexOf("\\", StringComparison.Ordinal) + 1)
                    .Remove(0, s.LastIndexOf("/", StringComparison.Ordinal) + 1)).ToArray();
        }

        /// <summary>
        ///     Method to remove pre fix from path
        /// </summary>
        /// <param name="path">Specifies path</param>
        /// <returns>Get last name from path</returns>
        private string RemovePreFixPath(string path)
        {
            return path.Remove(0, path.LastIndexOf("\\", StringComparison.Ordinal) + 1).Remove(0, path.LastIndexOf("/", StringComparison.Ordinal) + 1);
        }

        /// <summary>
        ///     Method to get data between two xml tag
        /// </summary>
        /// <param name="data">Specifies data for check</param>
        /// <param name="xmlTagName">Specifies tag name</param>
        /// <returns>If found return data else return null</returns>
        private string GetDataBetweenTwoXmlTag(string data, string xmlTagName)
        {
            var starterTagName = $"<{xmlTagName}>".ToUpper();
            var endTagName = $"</{xmlTagName}>".ToUpper();
            var firstIndex = data.ToUpper().IndexOf(starterTagName, StringComparison.Ordinal);
            var lastIndex = data.ToUpper().LastIndexOf(endTagName, StringComparison.Ordinal);
            if (firstIndex != -1 && lastIndex != -1 && firstIndex < lastIndex)
                return data.Substring(firstIndex + starterTagName.Length,
                    lastIndex - (firstIndex + starterTagName.Length));
            return null;
        }

        /// <summary>
        ///     Return full path from a path
        /// </summary>
        /// <param name="path">Specifies path</param>
        /// <returns>Return full path</returns>
        private string GetFullPathFromPath(string path)
        {
            try
            {
                return new DirectoryInfo(path).FullName;
            }
            catch (Exception e)
            {
                Logger.G9SmallLogException(e);
                throw;
            }
        }

        /// <summary>
        ///     Method to set last task status
        /// </summary>
        /// <param name="status">status</param>
        /// <param name="percentCurrentStep">Percent Current Step</param>
        protected void SetLastTaskStatus(G9ETaskStatus status, double percentCurrentStep)
        {
            try
            {
                _lastTaskStatus = new G9DtTaskAnswer
                {
                    Success = true,
                    StepOfInstall = status,
                    PercentCurrectStep = percentCurrentStep,
                    RowReceiveNumberCount = _lastTaskStatus.RowReceiveNumberCount,
                    NumberOfErrorScript = _lastTaskStatus.NumberOfErrorScript
                };
            }
            catch (Exception e)
            {
                Logger.G9SmallLogException(e);
                throw;
            }
        }

        /// <summary>
        ///     Method for add to count of error script
        /// </summary>
        protected void PlusCountOfTaskError()
        {
            try
            {
                _lastTaskStatus = new G9DtTaskAnswer
                {
                    Success = true,
                    StepOfInstall = _lastTaskStatus.StepOfInstall,
                    PercentCurrectStep = _lastTaskStatus.PercentCurrectStep,
                    RowReceiveNumberCount = _lastTaskStatus.RowReceiveNumberCount,
                    NumberOfErrorScript = _lastTaskStatus.NumberOfErrorScript + 1
                };
            }
            catch (Exception e)
            {
                Logger.G9SmallLogException(e);
                throw;
            }
        }

        /// <summary>
        ///     Method to get last task status
        /// </summary>
        /// <returns></returns>
        public G9DtTaskAnswer GetLastTaskStatus()
        {
            try
            {
                return _lastTaskStatus;
            }
            catch (Exception e)
            {
                return new G9DtTaskAnswer
                {
                    Message = e.Message,
                    FatalErrorStopInstall = true,
                    NeedShowMessage = true,
                    Success = false
                };
            }
        }

        /// <inherit />
        public virtual void Dispose()
        {
            _totalProjects?.Clear();
            _totalProjects = null;
        }

        /// <summary>
        ///     Method for check exist a database
        /// </summary>
        /// <returns>If found database return true</returns>
        public abstract bool CheckDatabaseExist();

        /// <summary>
        ///     Method for get database version
        /// </summary>
        public abstract string GetDatabaseVersion();

        /// <summary>
        ///     Method to initialize requirement and tables for database
        /// </summary>
        protected abstract void CreateRequirementTables();

        /// <summary>
        ///     Method to remove tables of database version control system from database
        /// </summary>
        /// <returns>If successful return true</returns>
        public abstract bool RemoveTablesOfDatabaseVersionControlFromDatabase();

        /// <summary>
        ///     Method for handle and execute new update script on a database
        /// </summary>
        /// <param name="customDatabaseName">Specifies custom database name for restore</param>
        /// <returns>If successful the method will return 'true' </returns>
        public abstract Task<bool> StartUpdate(string customDatabaseName = null);

        /// <summary>
        ///     Method for restore a empty database and execute all update script on a database
        /// </summary>
        /// <param name="customDatabaseName">Specifies custom database name for restore</param>
        /// <param name="databaseRestorePath">Specifies custom restore database file path</param>
        /// <returns>If successful the method will return 'true' </returns>
        public abstract Task<bool> StartInstall(string customDatabaseName = null, string databaseRestorePath = null);

        /// <summary>
        ///     Method for execute custom task
        /// </summary>
        /// <param name="nicknameOfCustomTask">Specifies nickname of custom task for execute custom task</param>
        /// <param name="customDatabaseName">Specifies custom database name for restore</param>
        /// <returns>If successful the method will return 'true' </returns>
        public abstract Task<bool> StartCustomTask(string nicknameOfCustomTask, string customDatabaseName = null);

        /// <summary>
        ///     Method for get backup from a database
        /// </summary>
        /// <param name="backupPath">Specifies a path for backup</param>
        /// <returns>If success to get backup return true</returns>
        public abstract bool BackupDatabase(string backupPath);

        /// <summary>
        ///     Method to execute query on database without result
        /// </summary>
        /// <param name="query">Specifies query</param>
        public abstract void ExecuteQueryWithoutResult(string query);

        /// <summary>
        ///     Method to execute query on database with result
        /// </summary>
        /// <param name="query">Specifies query</param>
        /// <returns>Received data from query - List specifies rows and dictionary specifies column and value</returns>
        public abstract List<Dictionary<string, object>> ExecuteQueryWithResult(string query);

        /// <summary>
        ///     Method to add or update map items in category
        ///     <para />
        ///     If was exists a map with same project name, the map will be updated. otherwise, will be adding a map like a new map
        ///     item.
        /// </summary>
        /// <exception cref="ArgumentNullException">The method throwing an exception if the param is null or empty.</exception>
        /// <exception cref="ArgumentNullException">The method throwing an exception if param items are set to the default value.</exception>
        /// <param name="mapData">Specifies map item</param>
        public static void MapProjects(params G9DtMap[] mapData)
        {
            if ((mapData?.Length ?? 0) == 0)
                throw new ArgumentNullException(nameof(mapData), $"Param '{nameof(mapData)}' can't be null or empty.");

            if (mapData.Any(s => s.Equals(default(G9DtMap))))
                throw new ArgumentNullException(nameof(mapData), $"Param '{nameof(mapData)}' can't be set default.");

            foreach (var map in mapData)
                if (TotalMapData.ContainsKey(map.ProjectName))
                    TotalMapData[map.ProjectName] = map;
                else
                    TotalMapData.Add(map.ProjectName, map);
        }

        /// <summary>
        ///     The method will be removing map items by project name from the map category.
        /// </summary>
        /// <exception cref="ArgumentNullException">The method throwing exception if project name not exist in map category</exception>
        public static void UnmapProjects(params string[] projectNames)
        {
            var checkExistsValidationProjectName =
                projectNames.FirstOrDefault(s => TotalMapData.All(x => x.Key != s));
            if (!Equals(checkExistsValidationProjectName, null))
                throw new ArgumentException(
                    $"The map was not found with this project name. Project name: '{checkExistsValidationProjectName}'",
                    nameof(projectNames));

            foreach (var projectName in projectNames)
                if (TotalMapData.ContainsKey(projectName))
                    TotalMapData.Remove(projectName);
        }

        /// <summary>
        ///     The method to get total assigned maps
        /// </summary>
        public static IList<G9DtMap> GetAssignedMaps()
        {
            return TotalMapData.Select(s => s.Value).ToArray();
        }

        #endregion
    }
}