using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using G9DatabaseVersionControlCore.Class.SmallLogger.Interface;
using G9DatabaseVersionControlCore.DataType;
using G9DatabaseVersionControlCore.DataType.AjaxDataType;
using G9DatabaseVersionControlCore.DataType.AjaxDataType.StepDataType;
using G9DatabaseVersionControlCore.Enums;
using Newtonsoft.Json;

// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedMemberInSuper.Global

namespace G9DatabaseVersionControlCore.SqlServer
{
    /// <summary>
    ///     A class for database version control for SQLServer
    /// </summary>
    // ReSharper disable once InconsistentNaming
    public class G9CDatabaseVersionControlCoreSQLServer : G9CDatabaseVersionControl
    {
        #region ### Fields And Properties ###

        /// <summary>
        ///     Specifies full connection string
        /// </summary>
        public readonly string ConnectionString;

        #endregion

        #region ### Methods ###

        #region ### Constructor ###

        /// <summary>
        ///     Constructor - Initialize requirement
        /// </summary>
        /// <param name="connectionString">Specifies connection string</param>
        /// <param name="projectName">Specifies project name to access assigned map.</param>
        /// <param name="logger">Specifies custom logger (if null use default logger)</param>
        /// <exception cref="ArgumentException">
        ///     If not exist a map for this project name. The method throw exception about the map
        ///     not found.
        /// </exception>
        public G9CDatabaseVersionControlCoreSQLServer(string connectionString, string projectName, G9ISmallLogger logger = null)
            : base(projectName, logger)
        {
            ConnectionString = connectionString ??
                               throw new ArgumentNullException(nameof(connectionString));
            if (!CheckConnectionString(ConnectionString))
                throw new Exception(
                    $"The connection string ({ConnectionString}) is incorrect!");
            CurrentDatabaseVersion = GetDatabaseVersion();
        }

        /// <summary>
        ///     Constructor - Initialize requirement
        /// </summary>
        /// <param name="connectionString">Specifies connection string</param>
        /// <param name="map">Specifies a map for assign to project.</param>
        /// <param name="logger">Specifies custom logger (if null use default logger)</param>
        public G9CDatabaseVersionControlCoreSQLServer(string connectionString, G9DtMap map, G9ISmallLogger logger = null)
            : base(map, logger)
        {
            ConnectionString = connectionString ??
                               throw new ArgumentNullException(nameof(connectionString));
            if (!CheckConnectionString(ConnectionString))
                throw new Exception(
                    $"The connection string ({ConnectionString}) is incorrect!");
            CurrentDatabaseVersion = GetDatabaseVersion();
        }

        /// <summary>
        ///     Constructor - Initialize requirement
        /// </summary>
        /// <param name="connectionStringDataSource">Specifies connection string data source</param>
        /// <param name="connectionStringUserId">Specifies connection string user id</param>
        /// <param name="connectionStringPassword">Specifies connection string password</param>
        /// <param name="projectName">Specifies project name to access assigned map.</param>
        /// <param name="logger">Specifies custom logger (if null use default logger)</param>
        /// <exception cref="ArgumentException">
        ///     If not exist a map for this project name. The method throw exception about the map
        ///     not found.
        /// </exception>
        public G9CDatabaseVersionControlCoreSQLServer(string connectionStringDataSource, string connectionStringUserId,
            string connectionStringPassword, string projectName, G9ISmallLogger logger = null)
            : base(projectName, logger)
        {
            var connectionStringDataSource1 = connectionStringDataSource ??
                                              throw new ArgumentNullException(nameof(connectionStringDataSource));
            var connectionStringUserId1 =
                connectionStringUserId ?? throw new ArgumentNullException(nameof(connectionStringUserId));
            var connectionStringPassword1 = connectionStringPassword ??
                                            throw new ArgumentNullException(nameof(connectionStringPassword));
            if (!CheckConnectionString(connectionStringDataSource1, connectionStringUserId1, connectionStringPassword1))
                throw new Exception(
                    $"The entered connection string parameters ({nameof(connectionStringDataSource)}, {nameof(connectionStringUserId)}, {nameof(connectionStringPassword)}) are incorrect!\nConnection string: '{ConvertFieldToConnectionString(connectionStringDataSource, connectionStringUserId, connectionStringPassword)}'");

            ConnectionString = ConvertFieldToConnectionString(connectionStringDataSource, connectionStringUserId,
                connectionStringPassword);
            CurrentDatabaseVersion = GetDatabaseVersion();
        }

        /// <summary>
        ///     Constructor - Initialize requirement
        /// </summary>
        /// <param name="connectionStringDataSource">Specifies connection string data source</param>
        /// <param name="connectionStringUserId">Specifies connection string user id</param>
        /// <param name="connectionStringPassword">Specifies connection string password</param>
        /// <param name="map">Specifies a map for assign to project.</param>
        /// <param name="logger">Specifies custom logger (if null use default logger)</param>
        public G9CDatabaseVersionControlCoreSQLServer(string connectionStringDataSource, string connectionStringUserId,
            string connectionStringPassword, G9DtMap map, G9ISmallLogger logger = null)
            : base(map, logger)
        {
            var connectionStringDataSource1 = connectionStringDataSource ??
                                              throw new ArgumentNullException(nameof(connectionStringDataSource));
            var connectionStringUserId1 =
                connectionStringUserId ?? throw new ArgumentNullException(nameof(connectionStringUserId));
            var connectionStringPassword1 = connectionStringPassword ??
                                            throw new ArgumentNullException(nameof(connectionStringPassword));

            if (!CheckConnectionString(connectionStringDataSource1, connectionStringUserId1, connectionStringPassword1))
                throw new Exception(
                    $"The entered connection string parameters ({nameof(connectionStringDataSource)}, {nameof(connectionStringUserId)}, {nameof(connectionStringPassword)}) are incorrect!\nConnection string: '{ConvertFieldToConnectionString(connectionStringDataSource, connectionStringUserId, connectionStringPassword)}'");
            ConnectionString = ConvertFieldToConnectionString(connectionStringDataSource, connectionStringUserId,
                connectionStringPassword);
            CurrentDatabaseVersion = GetDatabaseVersion();
        }

        #endregion

        /// <summary>
        ///     Method to check connection string
        /// </summary>
        /// <param name="connectionDataSource">Specifies connection string data source</param>
        /// <param name="connectionUserId">Specifies connection string user id</param>
        /// <param name="connectionPassword">Specifies connection string password</param>
        /// <param name="databaseName">Specifies database name (Optional)</param>
        /// <returns>if the connection string is the correct return true</returns>
        public static bool CheckConnectionString(string connectionDataSource, string connectionUserId,
            string connectionPassword, string databaseName = null)
        {
            try
            {
                using (var conn =
                    string.IsNullOrEmpty(databaseName)
                        ? new SqlConnection(ConvertFieldToConnectionString(connectionDataSource, connectionUserId,
                            connectionPassword))
                        : new SqlConnection(ConvertFieldToConnectionString(connectionDataSource, connectionUserId,
                            connectionPassword, databaseName))
                )
                {
                    conn.Open();
                    return conn.State == ConnectionState.Open;
                }
            }
            catch (Exception ex)
            {
                // Ignore
                Logger.G9SmallLogException(ex);
                return false;
            }
        }

        /// <summary>
        ///     Method to check connection string
        /// </summary>
        /// <param name="connectionString">Specifies connection string</param>
        /// <returns>if the connection string is the correct return true</returns>
        public static bool CheckConnectionString(string connectionString)
        {
            try
            {
                using (var conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    return conn.State == ConnectionState.Open;
                }
            }
            catch (Exception ex)
            {
                // Ignore
                Logger.G9SmallLogException(ex);
                return false;
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="connectionDataSource">Specifies connection string data source</param>
        /// <param name="connectionUserId">Specifies connection string user id</param>
        /// <param name="connectionPassword">Specifies connection string password</param>
        /// <param name="databaseName">Specifies database name (Optional)</param>
        /// <returns>Return full connection string</returns>
        public static string ConvertFieldToConnectionString(string connectionDataSource,
            string connectionUserId, string connectionPassword, string databaseName = null)
        {
            try
            {
                return string.IsNullOrEmpty(databaseName)
                    ? $"data source='{connectionDataSource}'; user id='{connectionUserId}'; password='{connectionPassword}'; MultipleActiveResultSets=True; App=EntityFramework"
                    : $"data source='{connectionDataSource}'; initial catalog='{databaseName}'; user id='{connectionUserId}'; password='{connectionPassword}'; MultipleActiveResultSets=True; App=EntityFramework";
            }
            catch (Exception ex)
            {
                Logger.G9SmallLogException(ex);
                throw;
            }
        }

        /// <inheritdoc />
        public override void Dispose()
        {
            Console.WriteLine("Dispose!");
            base.Dispose();
        }

        /// <summary>
        ///     Method for check exist a database
        /// </summary>
        /// <param name="databaseName">Specifies database to check</param>
        /// <param name="dataSource">Specifies data source for connection string</param>
        /// <param name="userId">Specifies userId for connection string</param>
        /// <param name="password">Specifies password for connection string</param>
        /// <returns>If found database return true</returns>
        public static bool CheckDatabaseExist(string databaseName, string dataSource, string userId, string password)
        {
            try
            {
                using (var connection =
                    new SqlConnection(ConvertFieldToConnectionString(dataSource, userId, password)))
                {
                    using (var command = new SqlCommand("SELECT db_id('" + databaseName + "')", connection))
                    {
                        connection.Open();
                        return command.ExecuteScalar() != DBNull.Value;
                    }
                }
            }
            catch (Exception e)
            {
                Logger.G9SmallLogException(e);
                return false;
            }
        }

        /// <summary>
        ///     Method for check exist a database
        /// </summary>
        /// <param name="databaseName">Specifies database to check</param>
        /// <param name="connectionString">Specifies connection string</param>
        /// <returns>If found database return true</returns>
        public static bool CheckDatabaseExist(string databaseName, string connectionString)
        {
            try
            {
                using (var connection =
                    new SqlConnection(connectionString))
                {
                    using (var command = new SqlCommand("SELECT db_id('" + databaseName + "')", connection))
                    {
                        connection.Open();
                        return command.ExecuteScalar() != DBNull.Value;
                    }
                }
            }
            catch (Exception e)
            {
                Logger.G9SmallLogException(e);
                return false;
            }
        }

        /// <inheritdoc />
        public override bool CheckDatabaseExist()
        {
            try
            {
                using (var connection =
                    new SqlConnection(ConnectionString))
                {
                    using (var command =
                        new SqlCommand("SELECT db_id('" + ProjectMapData.DatabaseName + "')", connection))
                    {
                        connection.Open();
                        return command.ExecuteScalar() != DBNull.Value;
                    }
                }
            }
            catch (Exception e)
            {
                Logger.G9SmallLogException(e);
                return false;
            }
        }

        /// <inheritdoc />
        public sealed override string GetDatabaseVersion()
        {
            try
            {
                var result =
                    ExecuteQueryWithResult(
                        $"SELECT DatabaseVersion FROM [{ProjectMapData.DefaultSchemaForTables}].[G9DatabaseVersion]");
                return result.Any() && result[0].Any() ? result[0]["DatabaseVersion"].ToString() : "0.0.0.0";
            }
            catch
            {
                // Ignore
                return "0.0.0.0";
            }
        }

        /// <inheritdoc />
        protected sealed override void CreateRequirementTables()
        {
            ExecuteQueryWithoutResult($@"IF (NOT EXISTS
(
    SELECT *
    FROM INFORMATION_SCHEMA.TABLES
    WHERE TABLE_SCHEMA = '{ProjectMapData.DefaultSchemaForTables}'
          AND TABLE_NAME = 'G9DatabaseVersion'
)
   )
BEGIN
    CREATE TABLE [{ProjectMapData.DefaultSchemaForTables}].[G9DatabaseVersion]
    (
        [DatabaseVersion] [NVARCHAR](50) NOT NULL,
        [ProductVersion] [NVARCHAR](50) NOT NULL,
        [DatabaseVersionDateTime] [DATETIME2](7) NOT NULL,
        [LastUpdateDateTime] [DATETIME2](7) NOT NULL
    ) ON [PRIMARY];
END;

IF (NOT EXISTS
(
    SELECT *
    FROM INFORMATION_SCHEMA.TABLES
    WHERE TABLE_SCHEMA = '{ProjectMapData.DefaultSchemaForTables}'
          AND TABLE_NAME = 'G9DatabaseUpdateHistory'
)
   )
BEGIN
    CREATE TABLE [{ProjectMapData.DefaultSchemaForTables}].[G9DatabaseUpdateHistory]
    (
        [UpdateFileFullPath] [NVARCHAR](300) NOT NULL,
        [ExecuteDateTime] [DATETIME2](7) NOT NULL,
        [Author] [NVARCHAR](30) NULL,
        [Description] [NVARCHAR](300) NULL,
        [UpdateDateTime] [NVARCHAR](30) NULL,
        [Version] [NVARCHAR](30) NULL,
        [IsSuccess] [BIT] NOT NULL
    ) ON [PRIMARY];
END;

IF ((SELECT COUNT(*) FROM [{ProjectMapData.DefaultSchemaForTables}].[G9DatabaseVersion]) = 0)
BEGIN
    INSERT INTO [{ProjectMapData.DefaultSchemaForTables}].[G9DatabaseVersion]
    (
        [DatabaseVersion],
        [ProductVersion],
        [DatabaseVersionDateTime],
        [LastUpdateDateTime]
    )
    VALUES
    (N'0.0.0.0', N'{ProjectMapData.DefaultSchemaForTables}', '1990-09-01 00:00:00', '1990-09-01 00:00:00');
END;");
        }

        /// <inheritdoc />
        public override bool RemoveTablesOfDatabaseVersionControlFromDatabase()
        {
            try
            {
                ExecuteQueryWithoutResult($@"IF EXISTS
(
    SELECT 1
    FROM INFORMATION_SCHEMA.TABLES
    WHERE TABLE_SCHEMA = '{ProjectMapData.DefaultSchemaForTables}'
          AND TABLE_NAME = 'G9DatabaseVersion'
)
    DROP TABLE [{ProjectMapData.DefaultSchemaForTables}].[G9DatabaseVersion];
IF EXISTS
(
    SELECT 1
    FROM INFORMATION_SCHEMA.TABLES
    WHERE TABLE_SCHEMA = '{ProjectMapData.DefaultSchemaForTables}'
          AND TABLE_NAME = 'G9DatabaseUpdateHistory'
)
    DROP TABLE [{ProjectMapData.DefaultSchemaForTables}].[G9DatabaseUpdateHistory];");
                return true;
            }
            catch (Exception ex)
            {
                Logger.G9SmallLogException(ex);
                return false;
            }
        }

        /// <inheritdoc />
        public override async Task<bool> StartUpdate(string customDatabaseName = null)
        {
            return await Task.Run(() =>
            {
                SetLastTaskStatus(G9ETaskStatus.UpdateDataBase, 0);

                // Step 1: Rename the database as needed
                if (ProjectMapData.EnableSetCustomDatabaseName && !string.IsNullOrEmpty(customDatabaseName) &&
                    customDatabaseName != ProjectMapData.DatabaseName)
                    ProjectMapData.ChangeDatabaseName(customDatabaseName);

                // Step 2: Create Requirements tables if not exists
                CreateRequirementTables();

                // Step 3: If there was an update for the current version  => Execute update scripts on the database
                if (CheckUpdateExist())
                    ExecutesScriptsOnDatabase(GetUpdateFolders(), GetCountOfUpdateFiles());

                SetLastTaskStatus(G9ETaskStatus.UpdateDataBase, 100);

                SetLastTaskStatus(G9ETaskStatus.InstallFinished, 100);

                // Last step: return true
                return true;
            });
        }

        /// <inheritdoc />
        public override async Task<bool> StartInstall(string customDatabaseName = null,
            string databaseRestorePath = null)
        {
            SetLastTaskStatus(G9ETaskStatus.CheckInstallData, 100);
            if (!ProjectMapData.EnableSetCustomDatabaseName)
                customDatabaseName = ProjectMapData.DatabaseName;
            if (!ProjectMapData.EnableSetCustomDatabaseRestoreFilePath)
                databaseRestorePath = null;

            return await Task.Run(async () =>
            {
                try
                {
                    SetLastTaskStatus(G9ETaskStatus.CheckInstallData, 100);

                    // Step 1: Restore 
                    switch (ProjectMapData.BaseDatabaseType)
                    {
                        case G9EBaseDatabaseType.NotSet:
                            throw new Exception(
                                "Can't find the base database setting for this project name. please check the assigned map.");
                        case G9EBaseDatabaseType.CreateBaseDatabaseByBackupDatabasePath:
                            if (!RestoreDatabase(ProjectMapData.BaseDatabaseBackupPath,
                                ProjectMapData.EnableSetCustomDatabaseName && !string.IsNullOrEmpty(customDatabaseName)
                                    ? customDatabaseName
                                    : ProjectMapData.DatabaseName))
                                throw new Exception("Error on database restore, Please check log data!");
                            break;
                        case G9EBaseDatabaseType.CreateBaseDatabaseByScriptData:
                            ExecuteQueryWithoutResult(ProjectMapData.GenerateBaseDatabaseScriptFunc(
                                ProjectMapData.EnableSetCustomDatabaseName && !string.IsNullOrEmpty(customDatabaseName)
                                    ? customDatabaseName
                                    : ProjectMapData.DatabaseName
                                , ProjectMapData.EnableSetCustomDatabaseRestoreFilePath &&
                                  !string.IsNullOrEmpty(databaseRestorePath)
                                    ? databaseRestorePath
                                    : GetDefaultDatabasePath()));
                            break;
                        case G9EBaseDatabaseType.CreateBaseDatabaseByFunc:
                            ProjectMapData.CreateDatabaseFunc(
                                ProjectMapData.EnableSetCustomDatabaseName && !string.IsNullOrEmpty(customDatabaseName)
                                    ? customDatabaseName
                                    : null
                                , ProjectMapData.EnableSetCustomDatabaseRestoreFilePath &&
                                  !string.IsNullOrEmpty(databaseRestorePath)
                                    ? databaseRestorePath
                                    : GetDefaultDatabasePath()
                                , ConnectionString);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException(nameof(G9EBaseDatabaseType),
                                $"There is no implementation for type '{ProjectMapData.BaseDatabaseType}'!");
                    }

                    SetLastTaskStatus(G9ETaskStatus.RestoreEmptyDataBase, 100);

                    // Step 2: Rename the database as needed
                    if (ProjectMapData.EnableSetCustomDatabaseName && !string.IsNullOrEmpty(customDatabaseName) &&
                        ProjectMapData.DatabaseName != customDatabaseName)
                        ProjectMapData.ChangeDatabaseName(customDatabaseName);

                    // Step 3: Create Requirements tables
                    CreateRequirementTables();

                    // Step 4: Execute update scripts on base database
                    var result = await StartUpdate();

                    SetLastTaskStatus(G9ETaskStatus.InstallFinished, 100);

                    return result;
                }
                catch (Exception ex)
                {
                    Logger.G9SmallLogException(ex);
                    throw;
                }
            });
        }

        /// <inheritdoc />
        public override async Task<bool> StartCustomTask(string customDatabaseName = null)
        {
            return await Task.Run(() =>
            {
                if (ProjectMapData.CustomTaskFunc == null)
                    throw new Exception("Custom func is not available! please check assigned map.");

                SetLastTaskStatus(G9ETaskStatus.UpdateDataBase, 0);

                // Step 1: Rename the database as needed
                if (ProjectMapData.EnableSetCustomDatabaseName && !string.IsNullOrEmpty(customDatabaseName) &&
                    customDatabaseName != ProjectMapData.DatabaseName)
                    ProjectMapData.ChangeDatabaseName(customDatabaseName);

                // Step 2: Create Requirements tables if not exists
                CreateRequirementTables();

                // Step 3: If there was an update for the current version  => Execute update scripts on the database
                ProjectMapData.CustomTaskFunc(customDatabaseName, ExecuteQueryWithoutResult, ExecuteQueryWithResult);

                SetLastTaskStatus(G9ETaskStatus.UpdateDataBase, 100);

                SetLastTaskStatus(G9ETaskStatus.InstallFinished, 100);

                // Last step: return true
                return true;
            });
        }

        /// <inheritdoc />
        public override bool BackupDatabase(string backupPath)
        {
            try
            {
                if (CheckDatabaseExist())
                    using (var sqlCon = new SqlConnection(ConnectionString))
                    {
                        var backupUrl = Path.Combine(backupPath,
                            $"{ProjectMapData.DatabaseName}-{DateTime.Now:yyyy-MM-dd-HH-mm}.bak");
                        if (File.Exists(backupUrl))
                            File.Delete(Path.Combine(backupPath,
                                $"{ProjectMapData.DatabaseName}-{DateTime.Now:yyyy-MM-dd-HH-mm}.bak"));

                        sqlCon.Open();


                        using (var sqlCmd = new SqlCommand(
                            "backup database " + ProjectMapData.DatabaseName + " to disk='" + backupUrl + "'", sqlCon))
                        {
                            return sqlCmd.ExecuteNonQuery() != 0;
                        }
                    }

                Logger.G9SmallLogError("Database not found! backup fail!");
                return false;
            }
            catch (Exception e)
            {
                Logger.G9SmallLogException(e);
                return false;
            }
        }

        /// <summary>
        ///     Method to handle and execute list of folders and files, then update database version by folder information
        /// </summary>
        /// <param name="updateFolders">Specifies list of data type update folders info</param>
        /// <param name="totalScriptsCount">Specifies total scripts count</param>
        protected void ExecutesScriptsOnDatabase(IList<G9DtUpdateFoldersInfo> updateFolders, int totalScriptsCount)
        {
            try
            {
                var counter = 0;

                if (updateFolders == null || !updateFolders.Any())
                    throw new ArgumentException("The Parameter can't be empty!", nameof(updateFolders));
                foreach (var folder in updateFolders)
                {
                    var queryDatabaseVersionUpdate =
                        $"UPDATE [{ProjectMapData.DefaultSchemaForTables}].[G9DatabaseVersion] SET DatabaseVersion = '{folder.FolderVersion}', ProductVersion = '{ProductVersion}', DatabaseVersionDateTime = '{folder.FolderDateTime.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture)}', LastUpdateDateTime = GETDATE()";
                    try
                    {
                        ExecuteQueryWithoutResult(queryDatabaseVersionUpdate);
                    }
                    catch (Exception ex)
                    {
                        Logger.G9SmallLogError(
                            $"Error on update the database version.\nQuery: {queryDatabaseVersionUpdate}\nException Message: {ex.Message}");
                        PlusCountOfTaskError();
                        // Ignore
                    }

                    foreach (var file in folder.UpdateFilesInfos)
                        try
                        {
                            ExecuteQueryWithoutResult(file.UpdateFileData);

                            var queryUpdateHistory =
                                $@"INSERT INTO [{ProjectMapData.DefaultSchemaForTables}].[G9DatabaseUpdateHistory]
([UpdateFileFullPath], [ExecuteDateTime], [Author], [Description], [UpdateDateTime], [Version], [IsSuccess])
VALUES
('{FixedLengthFromEndOfString(file.UpdateFileFullPath, 300)}', GETDATE(), '{FixedLengthFromEndOfString(file.Author, 30)}',
'{FixedLengthFromEndOfString(file.Description, 300)}',  '{file.UpdateDateTime.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture)}',
'{FixedLengthFromEndOfString(file.Version, 30)}', 1);";
                            try
                            {
                                ExecuteQueryWithoutResult(queryUpdateHistory);
                            }
                            catch (Exception ex)
                            {
                                Logger.G9SmallLogError(
                                    $"Error on update the database update history.\nQuery: {queryUpdateHistory}\nException Message: {ex.Message}");
                                PlusCountOfTaskError();
                                // Ignore
                            }
                        }
                        catch (Exception ex)
                        {
                            Logger.G9SmallLogError(
                                $"Error on execute update script.\nScript path: {file.UpdateFileFullPath}\nException Message: {ex.Message}");
                            PlusCountOfTaskError();
                            // Ignore

                            var queryUpdateHistory =
                                $@"INSERT INTO [{ProjectMapData.DefaultSchemaForTables}].[G9DatabaseUpdateHistory]
([UpdateFileFullPath], [ExecuteDateTime], [Author], [Description], [UpdateDateTime], [Version], [IsSuccess])
VALUES
('{FixedLengthFromEndOfString(file.UpdateFileFullPath, 300)}', GETDATE(), '{FixedLengthFromEndOfString(file.Author, 30)}',
'{FixedLengthFromEndOfString(file.Description, 30)}',  '{file.UpdateDateTime.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture)}',
'{FixedLengthFromEndOfString(file.Version, 30)}', 0);";
                            try
                            {
                                ExecuteQueryWithoutResult(queryUpdateHistory);
                            }
                            catch (Exception ex2)
                            {
                                Logger.G9SmallLogError(
                                    $"Error on update the database update history.\nQuery: {queryUpdateHistory}\nException Message: {ex2.Message}");
                                PlusCountOfTaskError();
                                // Ignore
                            }
                        }
                        finally
                        {
                            counter++;
                            SetLastTaskStatus(G9ETaskStatus.UpdateDataBase, counter * 100d / totalScriptsCount);
                        }
                }
            }
            catch (Exception ex)
            {
                Logger.G9SmallLogException(ex);
                throw;
            }
        }

        /// <inheritdoc />
        public override void ExecuteQueryWithoutResult(string query)
        {
            var databaseExist = CheckDatabaseExist(ProjectMapData.DatabaseName, ConnectionString);
            using (var connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                if (databaseExist)
                    connection.ChangeDatabase(ProjectMapData.DatabaseName);
                using (var command =
                    new SqlCommand(
                        query,
                        connection))
                {
                    command.CommandTimeout = 0;
                    command.ExecuteNonQuery();
                }
            }
        }

        /// <inheritdoc />
        public override List<Dictionary<string, object>> ExecuteQueryWithResult(string query)
        {
            var data = new List<Dictionary<string, object>>();
            var databaseExist = CheckDatabaseExist(ProjectMapData.DatabaseName, ConnectionString);
            using (var connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                if (databaseExist)
                    connection.ChangeDatabase(ProjectMapData.DatabaseName);
                using (var command =
                    new SqlCommand(
                        query,
                        connection))
                {
                    command.CommandTimeout = 0;
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            data.Add(new Dictionary<string, object>());
                            for (var i = 0; i < reader.FieldCount; i++)
                                data[data.Count - 1].Add(reader.GetName(i), reader[i]);
                        }
                    }
                }
            }

            return data;
        }

        /// <summary>
        ///     Method for restore a database
        /// </summary>
        /// <param name="databaseFullPath">Specifies full path of database(backup)</param>
        /// <param name="databaseName">Specifies database name for restore</param>
        /// <returns>If success return true</returns>
        private bool RestoreDatabase(string databaseFullPath, string databaseName)
        {
            try
            {
                if (!File.Exists(databaseFullPath))
                    throw new Exception($"Basic database not found! path: {databaseFullPath}");

                if (CheckDatabaseExist())
                    throw new Exception(
                        $"There is a database with this name and it cannot be restored before deleting it. Database name: {databaseName}");

                databaseFullPath = Path.GetFullPath(databaseFullPath);

                using (var sqlCon = new SqlConnection(ConnectionString))
                {
                    var script =
                        $@"RESTORE DATABASE [{databaseName}] FROM DISK = N'{databaseFullPath}'";
                    sqlCon.Open();

                    using (var sqlCmd = new SqlCommand(script, sqlCon))
                    {
                        sqlCmd.CommandTimeout = 0;
                        return sqlCmd.ExecuteNonQuery() != 0;
                    }
                }
            }
            catch (Exception e)
            {
                Logger.G9SmallLogException(e);
                return false;
            }
        }

        /// <summary>
        ///     Func for get default database path
        /// </summary>
        /// <returns>Default database path</returns>
        public string GetDefaultDatabasePath()
        {
            using (var connection = new SqlConnection(ConnectionString))
            {
                const string script =
                    "SELECT LEFT(physical_name,LEN(physical_name) - charindex('\\',reverse(physical_name),1) + 1) [path] FROM SYS.database_files WHERE type=0";

                connection.Open();

                using (var command = new SqlCommand(script, connection))
                {
                    command.CommandTimeout = 0;
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            var pathForDatabase = reader["path"].ToString();
                            if (string.IsNullOrEmpty(pathForDatabase))
                                throw new Exception(
                                    $"The specified path is not available for storing database files. Path: '{pathForDatabase}'");
                            return pathForDatabase;
                        }

                        throw new Exception(
                            "Can't get path for storing database files.");
                    }
                }
            }
        }

        /// <summary>
        ///     Remove first and save end of string if data length > maxLength
        /// </summary>
        /// <param name="data">Specifies data for fixed</param>
        /// <param name="maxLength">Specifies maximum length</param>
        /// <returns>Return Fixed Length From End Of String</returns>
        private static string FixedLengthFromEndOfString(string data, int maxLength)
        {
            if (string.IsNullOrEmpty(data)) return data;
            return data.Length > maxLength ? data.Substring(data.Length - maxLength, maxLength) : data;
        }


        private static G9CDatabaseVersionControlCoreSQLServer _installOrUpdateObject;

        public static string HandleTaskRequest(G9DtTaskRequest requestPacket)
        {
            try
            {
                switch (requestPacket.TaskRequest)
                {
                    case G9ETaskRequest.EnterConnectionString:
                        var checkConnectionString =
                            JsonConvert.DeserializeObject<G9DtConnectionString>(requestPacket.JsonData);

                        var connectionString = ConvertFieldToConnectionString(
                            checkConnectionString.DataSource,
                            checkConnectionString.UserId,
                            checkConnectionString.Password);
                        if (!CheckConnectionString(connectionString))
                            return JsonConvert.SerializeObject(new G9DtTaskAnswer
                            {
                                Success = false,
                                NeedShowMessage = true,
                                Message = "The fields entered for connecting to the database are incorrect."
                            });
                        var projectData = JsonConvert.SerializeObject(GetAssignedMaps()
                            .Select(s =>
                                new
                                {
                                    s.ProjectName,
                                    ProjectVersion = s.ProductVersionFunc(),
                                    s.DatabaseName,
                                    DatabaseVersion =
                                        CheckDatabaseExist(s.DatabaseName,
                                            connectionString)
                                            ? new G9CDatabaseVersionControlCoreSQLServer(connectionString, s)
                                                .GetDatabaseVersion()
                                            : "0.0.0.0",
                                    ExistBaseDatabase = s.BaseDatabaseType != G9EBaseDatabaseType.NotSet,
                                    ExistConvert = s.CustomTaskFunc != null,
                                    s.EnableSetCustomDatabaseName,
                                    s.EnableSetCustomDatabaseRestoreFilePath
                                }).ToArray());

                        return JsonConvert.SerializeObject(new G9DtTaskAnswer
                        {
                            Success = true,
                            Data = projectData
                        });

                    case G9ETaskRequest.CheckExistDatabase:
                        var dbExist = JsonConvert.DeserializeObject<G9DtCheckExistDatabase>(requestPacket.JsonData);
                        if (CheckDatabaseExist(dbExist.DatabaseName,
                            dbExist.DataSource, dbExist.UserId, dbExist.Password))
                            return JsonConvert.SerializeObject(new G9DtTaskAnswer
                            {
                                Success = true
                            });

                        return JsonConvert.SerializeObject(new G9DtTaskAnswer
                        {
                            Success = false
                        });
                    case G9ETaskRequest.InstallSoftwareAndUpdate:
                        var startInstall =
                            JsonConvert.DeserializeObject<G9DtStartInstallOrUpdate>(requestPacket.JsonData);
                        _installOrUpdateObject = new G9CDatabaseVersionControlCoreSQLServer(startInstall.DataSource,
                            startInstall.UserId, startInstall.Password,
                            GetAssignedMaps()
                                .First(s => s.ProjectName == startInstall.ProjectName));
                        Task.Run(async () =>
                            await _installOrUpdateObject.StartInstall(startInstall.DatabaseName,
                                startInstall.CustomDatabaseRestorePath));
                        return JsonConvert.SerializeObject(new G9DtTaskAnswer
                        {
                            Success = true
                        });
                    case G9ETaskRequest.UpdateSoftware:
                        var startUpdate =
                            JsonConvert.DeserializeObject<G9DtStartInstallOrUpdate>(requestPacket.JsonData);
                        _installOrUpdateObject = new G9CDatabaseVersionControlCoreSQLServer(startUpdate.DataSource,
                            startUpdate.UserId, startUpdate.Password,
                            GetAssignedMaps()
                                .First(s => s.ProjectName == startUpdate.ProjectName));
                        Task.Run(async () =>
                            await _installOrUpdateObject.StartUpdate(startUpdate.DatabaseName));
                        return JsonConvert.SerializeObject(new G9DtTaskAnswer
                        {
                            Success = true
                        });
                    case G9ETaskRequest.CustomTask:
                        var startCustomTask =
                            JsonConvert.DeserializeObject<G9DtStartInstallOrUpdate>(requestPacket.JsonData);
                        _installOrUpdateObject = new G9CDatabaseVersionControlCoreSQLServer(startCustomTask.DataSource,
                            startCustomTask.UserId, startCustomTask.Password,
                            GetAssignedMaps()
                                .First(s => s.ProjectName == startCustomTask.ProjectName));
                        Task.Run(async () =>
                            await _installOrUpdateObject.StartCustomTask(startCustomTask.DatabaseName));
                        return JsonConvert.SerializeObject(new G9DtTaskAnswer
                        {
                            Success = true
                        });
                    case G9ETaskRequest.CheckLastStatus:
                        return JsonConvert.SerializeObject(_installOrUpdateObject.GetLastTaskStatus());
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            catch (Exception ex)
            {
                return JsonConvert.SerializeObject(new G9DtTaskAnswer
                {
                    Success = false,
                    NeedShowMessage = true,
                    Message = ex.Message,
                    FatalErrorStopInstall = true
                });
            }
        }

        #endregion
    }
}