using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using G9DatabaseVersionControlCore.Class.SmallLogger;
using G9DatabaseVersionControlCore.DataType;
using G9DatabaseVersionControlCore.Enums;

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
        /// <exception cref="ArgumentException">
        ///     If not exist a map for this project name. The method throw exception about the map
        ///     not found.
        /// </exception>
        public G9CDatabaseVersionControlCoreSQLServer(string connectionString, string projectName)
            : base(projectName)
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
        public G9CDatabaseVersionControlCoreSQLServer(string connectionString, G9DtMap map)
            : base(map)
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
        /// <exception cref="ArgumentException">
        ///     If not exist a map for this project name. The method throw exception about the map
        ///     not found.
        /// </exception>
        public G9CDatabaseVersionControlCoreSQLServer(string connectionStringDataSource, string connectionStringUserId,
            string connectionStringPassword, string projectName)
            : base(projectName)
        {
            var connectionStringDataSource1 = connectionStringDataSource ??
                                              throw new ArgumentNullException(nameof(connectionStringDataSource));
            var connectionStringUserId1 =
                connectionStringUserId ?? throw new ArgumentNullException(nameof(connectionStringUserId));
            var connectionStringPassword1 = connectionStringPassword ??
                                            throw new ArgumentNullException(nameof(connectionStringPassword));
            if (!CheckConnectionString(connectionStringDataSource1, connectionStringUserId1, connectionStringPassword1,
                ProjectMapData.DatabaseName))
                throw new Exception(
                    $"The entered connection string parameters ({nameof(connectionStringDataSource)}, {nameof(connectionStringUserId)}, {nameof(connectionStringPassword)}) are incorrect!\nConnection string: '{ConvertFieldToConnectionString(connectionStringDataSource, ProjectMapData.DatabaseName, connectionStringUserId, connectionStringPassword)}'");

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
        public G9CDatabaseVersionControlCoreSQLServer(string connectionStringDataSource, string connectionStringUserId,
            string connectionStringPassword, G9DtMap map)
            : base(map)
        {
            var connectionStringDataSource1 = connectionStringDataSource ??
                                              throw new ArgumentNullException(nameof(connectionStringDataSource));
            var connectionStringUserId1 =
                connectionStringUserId ?? throw new ArgumentNullException(nameof(connectionStringUserId));
            var connectionStringPassword1 = connectionStringPassword ??
                                            throw new ArgumentNullException(nameof(connectionStringPassword));

            if (!CheckConnectionString(connectionStringDataSource1, connectionStringUserId1, connectionStringPassword1,
                ProjectMapData.DatabaseName))
                throw new Exception(
                    $"The entered connection string parameters ({nameof(connectionStringDataSource)}, {nameof(connectionStringUserId)}, {nameof(connectionStringPassword)}) are incorrect!\nConnection string: '{ConvertFieldToConnectionString(connectionStringDataSource, ProjectMapData.DatabaseName, connectionStringUserId, connectionStringPassword)}'");
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
        /// <returns></returns>
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
                    if (conn.State == ConnectionState.Open)
                        return true;
                    return false;
                }
            }
            catch (Exception ex)
            {
                // Ignore
                ex.G9SmallLogException();
                return false;
            }
        }

        /// <summary>
        ///     Method to check connection string
        /// </summary>
        /// <param name="connectionString">Specifies connection string</param>
        /// <param name="databaseName">Specifies database name (Optional)</param>
        /// <returns></returns>
        public static bool CheckConnectionString(string connectionString, string databaseName = null)
        {
            try
            {
                using (var conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    if (conn.State == ConnectionState.Open)
                        return true;
                    return false;
                }
            }
            catch (Exception ex)
            {
                // Ignore
                ex.G9SmallLogException();
                return false;
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="connectionDataSource">Specifies connection string data source</param>
        /// <param name="connectionUserId">Specifies connection string user id</param>
        /// <param name="connectionPassword">Specifies connection string password</param>
        /// <param name="databaseName">Specifies database name (Optional)</param>
        /// <returns></returns>
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
                ex.G9SmallLogException();
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
                e.G9SmallLogException();
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
                        if (command.ExecuteScalar() == DBNull.Value)
                            return false;
                        return true;
                    }
                }
            }
            catch (Exception e)
            {
                e.G9SmallLogException();
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
            catch (Exception ex)
            {
                ex.G9SmallLogException();
                throw;
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
                ex.G9SmallLogException();
                return false;
            }
        }

        /// <inheritdoc />
        public override async Task<bool> StartUpdate(string customDatabaseName = null)
        {
            return await Task.Run(() =>
            {
                // Step 1: Rename the database as needed
                if (ProjectMapData.EnableSetCustomDatabaseName && !string.IsNullOrEmpty(customDatabaseName))
                    ProjectMapData.ChangeDatabaseName(customDatabaseName);

                // Step 2: Create Requirements tables if not exists
                CreateRequirementTables();

                // Step 3: If there was an update for the current version  => Execute update scripts on the database
                if (CheckUpdateExist())
                    ExecutesScriptsOnDatabase(GetUpdateFolders(), GetCountOfUpdateFiles());

                // Last step: return true
                return true;
            });
        }

        /// <inheritdoc />
        public override async Task<bool> StartInstall(string customDatabaseName = null,
            string databaseRestorePath = null)
        {
            return await Task.Run(async () =>
            {
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
                                : ProjectMapData.DatabaseName
                            , ProjectMapData.EnableSetCustomDatabaseRestoreFilePath &&
                              !string.IsNullOrEmpty(databaseRestorePath)
                                ? databaseRestorePath
                                : null))
                            throw new Exception("Error on database restore, Please check log data!");
                        break;
                    case G9EBaseDatabaseType.CreateBaseDatabaseByScriptData:
                        ExecuteQueryWithResult(ProjectMapData.GenerateBaseDatabaseScriptFunc(
                            ProjectMapData.EnableSetCustomDatabaseName && !string.IsNullOrEmpty(customDatabaseName)
                                ? customDatabaseName
                                : null
                            , ProjectMapData.EnableSetCustomDatabaseRestoreFilePath &&
                              !string.IsNullOrEmpty(databaseRestorePath)
                                ? databaseRestorePath
                                : null));
                        break;
                    case G9EBaseDatabaseType.CreateBaseDatabaseByFunc:
                        ProjectMapData.CreateDatabaseFunc(
                            ProjectMapData.EnableSetCustomDatabaseName && !string.IsNullOrEmpty(customDatabaseName)
                                ? customDatabaseName
                                : null
                            , ProjectMapData.EnableSetCustomDatabaseRestoreFilePath &&
                              !string.IsNullOrEmpty(databaseRestorePath)
                                ? databaseRestorePath
                                : null
                            , ConnectionString);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(G9EBaseDatabaseType),
                            $"There is no implementation for type '{ProjectMapData.BaseDatabaseType}'!");
                }

                // Step 2: Rename the database as needed
                if (ProjectMapData.EnableSetCustomDatabaseName && !string.IsNullOrEmpty(customDatabaseName))
                    ProjectMapData.ChangeDatabaseName(customDatabaseName);

                // Step 3: Create Requirements tables
                CreateRequirementTables();

                // Step 4: Execute update scripts on base database
                return await StartUpdate();
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

                "Database not found! backup fail!".G9SmallLogError();
                return false;
            }
            catch (Exception e)
            {
                e.G9SmallLogException();
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
                        $"Error on update the database version.\nQuery: {queryDatabaseVersionUpdate}\nException Message: {ex.Message}"
                            .G9SmallLogError();
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
                                $"Error on update the database update history.\nQuery: {queryUpdateHistory}\nException Message: {ex.Message}"
                                    .G9SmallLogError();
                                PlusCountOfTaskError();
                                // Ignore
                            }
                        }
                        catch (Exception ex)
                        {
                            $"Error on execute update script.\nScript path: {file.UpdateFileFullPath}\nException Message: {ex.Message}"
                                .G9SmallLogError();
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
                                $"Error on update the database update history.\nQuery: {queryUpdateHistory}\nException Message: {ex2.Message}"
                                    .G9SmallLogError();
                                PlusCountOfTaskError();
                                // Ignore
                            }
                        }
                }
            }
            catch (Exception ex)
            {
                ex.G9SmallLogException();
                throw;
            }
        }

        /// <inheritdoc />
        public override void ExecuteQueryWithoutResult(string query)
        {
            using (var connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
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
            using (var connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
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
        /// <param name="databaseRestorePath">Specifies a path for restore file.</param>
        /// <returns>If success return true</returns>
        private bool RestoreDatabase(string databaseFullPath, string databaseName, string databaseRestorePath = null)
        {
            try
            {
                if (!File.Exists(databaseFullPath))
                    throw new Exception($"Basic database not found! path: {databaseFullPath}");

                if (CheckDatabaseExist())
                    throw new Exception(
                        $"There is a database with this name and it cannot be restored before deleting it. Database name: {databaseName}");

                string pathForDatabase;
                if (databaseRestorePath == null)
                {
                    using (var connection = new SqlConnection(ConnectionString))
                    {
                        var Script =
                            "SELECT LEFT(physical_name,LEN(physical_name) - charindex('\\',reverse(physical_name),1) + 1) [path] FROM SYS.database_files WHERE type=0";

                        connection.Open();

                        using (var command = new SqlCommand(Script, connection))
                        {
                            command.CommandTimeout = 0;
                            using (var reader = command.ExecuteReader())
                            {
                                if (reader.Read())
                                {
                                    pathForDatabase = reader["path"].ToString();
                                    if (string.IsNullOrEmpty(pathForDatabase))
                                        throw new Exception(
                                            $"The specified path is not available for storing database files. Path: '{pathForDatabase}'");
                                }
                            }
                        }
                    }

                    return true;
                }

                pathForDatabase = databaseRestorePath;

                using (var sqlCon = new SqlConnection(ConnectionString))
                {
                    var script = string.Format(
                        @"RESTORE DATABASE [{0}] FROM  DISK = N'{1}' WITH FILE = 1,  MOVE N'PdlWeb_Data' TO N'{2}\\{3}.mdf',  MOVE N'PdlWeb_Log' TO N'{2}\\{3}.ldf',  NOUNLOAD,  STATS = 10",
                        databaseName,
                        databaseFullPath,
                        pathForDatabase,
                        ProjectMapData.DatabaseName
                    );
                    sqlCon.Open();

                    using (var sqlCmd = new SqlCommand(script, sqlCon))
                    {
                        sqlCmd.CommandTimeout = 0;
                        if (sqlCmd.ExecuteNonQuery() != 0)
                            return true;
                        return false;
                    }
                }
            }
            catch (Exception e)
            {
                e.G9SmallLogException();
                return false;
            }
        }

        /// <summary>
        ///     Remove first and save end of string if data length > maxLength
        /// </summary>
        /// <param name="data">Specifies data for fixed</param>
        /// <param name="maxLength">Specifies maximum length</param>
        /// <returns></returns>
        private string FixedLengthFromEndOfString(string data, int maxLength)
        {
            if (string.IsNullOrEmpty(data)) return data;
            return data.Length > maxLength ? data.Substring(data.Length - maxLength, maxLength) : data;
        }

        #endregion
    }
}