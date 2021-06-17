using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using G9DatabaseVersionControlCore.Class.SmallLogger;
using G9DatabaseVersionControlCore.DataType;
using G9DatabaseVersionControlCore.Enums;

// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedMemberInSuper.Global

namespace G9DatabaseVersionControlCore
{
    /// <summary>
    ///     A core class for database version control
    /// </summary>
    public abstract class G9CDatabaseVersionControl
    {
        #region ### Fields And Properties ###

        /// <summary>
        ///     Specifies default product version (Assembly version)
        /// </summary>
        public static readonly string DefaultProductVersion =
#if (NETSTANDARD2_1 || NETSTANDARD2_0)
            string.IsNullOrEmpty(Assembly.GetExecutingAssembly().GetName().Version.ToString())
                ? Assembly.GetEntryAssembly()?.GetName().Version.ToString() ?? "0.0.0.0"
                : Assembly.GetExecutingAssembly().GetName().Version.ToString();
#elif (NETSTANDARD1_6 || NETSTANDARD1_5)
        string.IsNullOrEmpty(Assembly.GetEntryAssembly().GetName().Version.ToString())
                ? Assembly.GetEntryAssembly()?.GetName().Version.ToString() ?? "0.0.0.0"
                : Assembly.GetEntryAssembly().GetName().Version.ToString();
#else
            string.IsNullOrEmpty(Assembly.Load(new AssemblyName(nameof(G9CDatabaseVersionControl))).GetName().Version
                .ToString())
                ? Assembly.Load(new AssemblyName(nameof(G9CDatabaseVersionControl)))?.GetName().Version.ToString() ??
                  "0.0.0.0"
                : Assembly.Load(new AssemblyName(nameof(G9CDatabaseVersionControl))).GetName().Version.ToString();
#endif

        /// <summary>
        ///     Specifies product version (Assembly version)
        /// </summary>
        public readonly string ProductVersion;

        /// <summary>
        ///     Specifies default update files full path
        /// </summary>
        public const string DefaultDatabaseUpdateFilesFullPath = @"DatabaseUpdateFiles\";

        /// <summary>
        ///     Specifies default schema for create tables
        /// </summary>
        public const string DefaultSchemaForTables = "dbo";

        /// <summary>
        ///     Specifies update files full path
        /// </summary>
        public static string DatabaseUpdateFilesFullPath { private set; get; }

        /// <summary>
        ///     Specifies schema for create tables
        /// </summary>
        public readonly string SchemaForTables;

        /// <summary>
        ///     Access to all projects and infos
        /// </summary>
        private Dictionary<string, G9DtProjectInfo> _totalProjects;

        /// <summary>
        ///     Specifies encoding of update file
        /// </summary>
        public readonly Encoding DatabaseUpdateFileEncoding;

        /// <summary>
        ///     Specifies last status of task
        /// </summary>
        private G9DtLastTaskStatus _lastTaskStatus;

        /// <summary>
        ///     Specifies project name
        /// </summary>
        public readonly string ProjectName;

        /// <summary>
        ///     Specifies focus database
        /// </summary>
        public readonly string DatabaseName;

        /// <summary>
        ///     Specifies company
        /// </summary>
        public readonly string CompanyName;

        /// <summary>
        ///     Specifies current database version
        /// </summary>
        public string CurrentDatabaseVersion { protected set; get; }

        #endregion

        #region ### Methods ###

        /// <summary>
        ///     Constructor - Initialize Requirement
        /// </summary>
        /// <param name="projectName">
        ///     Specifies project name (You can use static method GetTotalProjectNames() to get list of
        ///     projects)
        /// </param>
        /// <param name="databaseName">Specifies focus database</param>
        /// <param name="companyName">Specifies company name (customer)</param>
        /// <param name="databaseUpdateFilesPath">Specify database update file path</param>
        /// <param name="defaultSchemaForTables">Specify default schema for create required table</param>
        /// <param name="productVersion">Specifies product version (Assembly version)</param>
        /// <param name="databaseUpdateFileEncoding">Specifies encoding of update file - default is UTF8</param>
        protected G9CDatabaseVersionControl(string projectName, string databaseName, string companyName,
            string databaseUpdateFilesPath = null, string defaultSchemaForTables = null,
            string productVersion = null, Encoding databaseUpdateFileEncoding = null)
        {
            DatabaseUpdateFilesFullPath = databaseUpdateFilesPath ?? DefaultDatabaseUpdateFilesFullPath;
            SchemaForTables = defaultSchemaForTables ?? DefaultSchemaForTables;
            DatabaseUpdateFileEncoding = databaseUpdateFileEncoding ?? Encoding.UTF8;
            ProductVersion = productVersion ?? DefaultProductVersion;
            ProjectName = projectName ?? throw new ArgumentNullException(nameof(projectName));
            DatabaseName = databaseName ?? throw new ArgumentNullException(nameof(databaseName));
            CompanyName = companyName ?? "G9TM";
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
                var projectsPath = GetFolderNamesFromPath(DatabaseUpdateFilesFullPath)
                    .Select(GetFullPathFromPath).ToArray();
                foreach (var projectPath in projectsPath)
                {
                    var projectUpdateFolders = GetFolderNamesFromPath(projectPath);
                    var foldersInfo = new List<G9DtUpdateFoldersInfo>();
                    foreach (var projectUpdateFolder in projectUpdateFolders)
                    {
                        var updateFilesInfo = new List<G9DtUpdateFilesInfo>();
                        var projectUpdateFileNames =
                            GetFilesFromPath(Path.Combine(projectPath, projectUpdateFolder));

                        foreach (var projectUpdateFileName in projectUpdateFileNames)
                        {
                            var fileData = File.ReadAllText(projectUpdateFileName, DatabaseUpdateFileEncoding);
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
                                e.G9SmallLogException();
                                // Ignore
                                updateDateTime = DateTime.MinValue;
                            }

                            updateFilesInfo.Add(new G9DtUpdateFilesInfo(GetFullPathFromPath(projectUpdateFileName),
                                RemovePreFixPath(projectUpdateFileName), author, description, updateDateTime, version,
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
                            exception.G9SmallLogException();
                            throw exception;
                        }

                        foldersInfo.Add(new G9DtUpdateFoldersInfo(GetFullPathFromPath(projectUpdateFolder), folderName,
                            folderVersion
                            , folderDateTime, updateFilesInfo));
                    }

                    _totalProjects.Add(RemovePreFixPath(projectPath),
                        new G9DtProjectInfo(GetFullPathFromPath(projectPath), RemovePreFixPath(projectPath), null,
                            DateTime.MinValue, foldersInfo));
                }
            }
            catch (Exception e)
            {
                e.G9SmallLogException("Error On Cache Data");
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
                if (!_totalProjects.ContainsKey(ProjectName))
                    throw new Exception($"Project with this name is not exist!\nProject name is '{ProjectName}'.");
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

                return _totalProjects[ProjectName].UpdateFoldersInfo
                    .Where(s => int.Parse(s.FolderVersion.Replace(".", string.Empty)) > versionInt)
                    .SelectMany(s => s.UpdateFilesInfos).ToArray();
            }
            catch (Exception e)
            {
                e.G9SmallLogException();
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
                if (!_totalProjects.ContainsKey(ProjectName))
                    throw new Exception($"Project whit this name not exist!\nProject name is '{ProjectName}'.");
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

                return _totalProjects[ProjectName].UpdateFoldersInfo
                    .Where(s => int.Parse(s.FolderVersion.Replace(".", string.Empty)) > versionInt).ToArray();
            }
            catch (Exception e)
            {
                e.G9SmallLogException();
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
                if (!_totalProjects.ContainsKey(ProjectName))
                    throw new Exception($"Project whit this name not exist!\nProject name is '{ProjectName}'.");
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

                return _totalProjects[ProjectName].UpdateFoldersInfo
                    .Where(s => int.Parse(s.FolderVersion.Replace(".", string.Empty)) > versionInt)
                    .SelectMany(s => s.UpdateFilesInfos).Count();
            }
            catch (Exception e)
            {
                e.G9SmallLogException();
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
                e.G9SmallLogException();
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
                e.G9SmallLogException();
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
                e.G9SmallLogException();
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
            return pathArray.Select(s => s.Remove(0, s.LastIndexOf(Path.DirectorySeparatorChar) + 1)).ToArray();
        }

        /// <summary>
        ///     Method to remove pre fix from path
        /// </summary>
        /// <param name="path">Specifies path</param>
        /// <returns>Get last name from path</returns>
        private string RemovePreFixPath(string path)
        {
            return path.Remove(0, path.LastIndexOf(Path.DirectorySeparatorChar) + 1);
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
                e.G9SmallLogException();
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
                _lastTaskStatus = new G9DtLastTaskStatus
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
                e.G9SmallLogException();
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
                _lastTaskStatus = new G9DtLastTaskStatus
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
                e.G9SmallLogException();
                throw;
            }
        }

        /// <summary>
        ///     Method to get last task status
        /// </summary>
        /// <returns></returns>
        public G9DtLastTaskStatus GetLastTaskStatus()
        {
            try
            {
                return _lastTaskStatus;
            }
            catch (Exception e)
            {
                return new G9DtLastTaskStatus
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
        /// Method to remove tables of database version control system from database
        /// </summary>
        /// <returns>If successful return true</returns>
        public abstract bool RemoveTablesOfDatabaseVersionControlFromDatabase();

        /// <summary>
        ///     Method for handle and execute new update script on a database
        /// </summary>
        public abstract void StartUpdate();

        /// <summary>
        ///     Method for restore a empty database and execute all update script on a database
        /// </summary>
        public abstract void StartInstall();

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

        #endregion
    }
}