using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using G9DatabaseVersionControlCore.Class.SmallLogger;
using G9DatabaseVersionControlCore.DataType;

namespace G9DatabaseVersionControlCore.SqlServer
{
    public class G9CDatabaseVersionControlCoreSQLServer : G9CDatabaseVersionControl
    {
        #region ### Fields And Properties ###

        /// <summary>
        ///     Specifies connection string data source
        /// </summary>
        public readonly string ConnectionStringDataSource;

        /// <summary>
        ///     Specifies connection string user id
        /// </summary>
        public readonly string ConnectionStringUserId;

        /// <summary>
        ///     Specifies connection string password
        /// </summary>
        public readonly string ConnectionStringPassword;


        /// <summary>
        ///     پروپرتی برای نگهداری کانکشن استرینگ دیتابیس
        /// </summary>
        public string ConnectionString
        {
            get
            {
                if (!string.IsNullOrEmpty(ConnectionStringDataSource) &&
                    !string.IsNullOrEmpty(ConnectionStringDataSource) &&
                    !string.IsNullOrEmpty(ConnectionStringDataSource))
                    return ConvertFieldToConnectionString(ConnectionStringDataSource, ConnectionStringUserId,
                        ConnectionStringPassword);
                throw new Exception("اطلاعات برای ساخت کانکشن استرینگ ناقص است.");
            }
        }

        #endregion

        #region ### Methods ###

        /// <summary>
        ///     Constructor - Initialize requirement
        /// </summary>
        /// <param name="connectionStringDataSource">Specifies connection string data source</param>
        /// <param name="connectionStringUserId">Specifies connection string user id</param>
        /// <param name="connectionStringPassword">Specifies connection string password</param>
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
        public G9CDatabaseVersionControlCoreSQLServer(string connectionStringDataSource, string connectionStringUserId,
            string connectionStringPassword, string projectName, string databaseName, string companyName,
            string databaseUpdateFilesPath = null, string defaultSchemaForTables = null, string productVersion = null,
            Encoding databaseUpdateFileEncoding = null)
            : base(projectName, databaseName, companyName, databaseUpdateFilesPath, defaultSchemaForTables,
                productVersion, databaseUpdateFileEncoding)
        {
            ConnectionStringDataSource = connectionStringDataSource ??
                                         throw new ArgumentNullException(nameof(connectionStringDataSource));
            ConnectionStringUserId =
                connectionStringUserId ?? throw new ArgumentNullException(nameof(connectionStringUserId));
            ConnectionStringPassword = connectionStringPassword ??
                                       throw new ArgumentNullException(nameof(connectionStringPassword));
            if (!CheckConnectionString(ConnectionStringDataSource, ConnectionStringUserId, ConnectionStringPassword,
                databaseName))
                throw new Exception(
                    $"The entered connection string parameters ({nameof(connectionStringDataSource)}, {nameof(connectionStringUserId)}, {nameof(connectionStringPassword)}) are incorrect!\nConnection string: '{ConvertFieldToConnectionString(connectionStringDataSource, databaseName, connectionStringUserId, connectionStringPassword)}'");
            CreateRequirementTables();
            CurrentDatabaseVersion = GetDatabaseVersion();
        }

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
        public override bool CheckDatabaseExist()
        {
            try
            {
                using (var connection =
                    new SqlConnection(ConvertFieldToConnectionString(ConnectionStringDataSource,
                        ConnectionStringUserId, ConnectionStringPassword)))
                {
                    using (var command = new SqlCommand("SELECT db_id('" + DatabaseName + "')", connection))
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
                    ExecuteQueryWithResult($"SELECT DatabaseVersion FROM [{SchemaForTables}].[G9DatabaseVersion]");
                if (((result as DataTable)?.Rows?.Count ?? 0) > 0)
                    return (result as DataTable).Rows[0][0].ToString();
                return "0.0.0.0";
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
    WHERE TABLE_SCHEMA = '{SchemaForTables}'
          AND TABLE_NAME = 'G9DatabaseVersion'
)
   )
BEGIN
    CREATE TABLE [{SchemaForTables}].[G9DatabaseVersion]
    (
        [DatabaseVersion] [NVARCHAR](50) NOT NULL,
        [ProductVersion] [NVARCHAR](50) NOT NULL,
        [DatabaseVersionDateTime] [DATETIME2](7) NOT NULL,
        [LastUpdateDateTime] [DATETIME2](7) NOT NULL,
        [CompanyName] [NVARCHAR](50) NOT NULL
    ) ON [PRIMARY];
END;

IF (NOT EXISTS
(
    SELECT *
    FROM INFORMATION_SCHEMA.TABLES
    WHERE TABLE_SCHEMA = '{SchemaForTables}'
          AND TABLE_NAME = 'G9DatabaseUpdateHistory'
)
   )
BEGIN
    CREATE TABLE [{SchemaForTables}].[G9DatabaseUpdateHistory]
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

IF ((SELECT COUNT(*) FROM [{SchemaForTables}].[G9DatabaseVersion]) = 0)
BEGIN
    INSERT INTO [{SchemaForTables}].[G9DatabaseVersion]
    (
        [DatabaseVersion],
        [ProductVersion],
        [DatabaseVersionDateTime],
        [LastUpdateDateTime],
        [CompanyName]
    )
    VALUES
    (N'0.0.0.0', N'{ProductVersion}', '1990-09-01 00:00:00', '1990-09-01 00:00:00', N'{CompanyName}');
END;");
        }

        /// <inheritdoc />
        public override void StartUpdate()
        {
            ExecutesScriptsOnDatabase(GetUpdateFolders(), GetCountOfUpdateFiles());
        }

        /// <inheritdoc />
        public override void StartInstall()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public override bool BackupDatabase(string backupPath)
        {
            try
            {
                if (CheckDatabaseExist())
                    using (var sqlcon = new SqlConnection(ConnectionString))
                    {
                        var backupUrl = Path.Combine(backupPath, $"{DatabaseName}-{DateTime.Now:yyyy-MM-dd-HH-mm}.bak");
                        if (File.Exists(backupUrl))
                            File.Delete(Path.Combine(backupPath,
                                $"{DatabaseName}-{DateTime.Now:yyyy-MM-dd-HH-mm}.bak"));

                        sqlcon.Open();


                        using (var sqlcmd = new SqlCommand(
                            "backup database " + DatabaseName + " to disk='" + backupUrl + "'", sqlcon))
                        {
                            if (sqlcmd.ExecuteNonQuery() != 0) return true;

                            return false;
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
                        $"UPDATE [{SchemaForTables}].[G9DatabaseVersion] SET DatabaseVersion = '{folder.FolderVersion}', ProductVersion = '{ProductVersion}', DatabaseVersionDateTime = '{folder.FolderDateTime.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture)}', LastUpdateDateTime = GETDATE()";
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

                            var queryUpdateHistory = $@"INSERT INTO [{SchemaForTables}].[G9DatabaseUpdateHistory]
([UpdateFileFullPath], [ExecuteDateTime], [Author], [Description], [UpdateDateTime], [Version], [IsSuccess])
VALUES
('{FixedLengthFromEndOfString(file.UpdateFileFullPath, 300)}', GETDATE(), '{FixedLengthFromEndOfString(file.Author, 30)}',
'{FixedLengthFromEndOfString(file.Description, 30)}',  '{file.UpdateDateTime.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture)}',
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

                            var queryUpdateHistory = $@"INSERT INTO [{SchemaForTables}].[G9DatabaseUpdateHistory]
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
        protected override void ExecuteQueryWithoutResult(string query)
        {
            using (var connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                connection.ChangeDatabase(DatabaseName);
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
        protected override object ExecuteQueryWithResult(string query)
        {
            var dt = new DataTable();
            using (var connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                connection.ChangeDatabase(DatabaseName);
                using (var command =
                    new SqlCommand(
                        query,
                        connection))
                {
                    command.CommandTimeout = 0;
                    using (var reader = command.ExecuteReader())
                    {
                        dt.Load(reader);
                        return dt;
                    }
                }
            }
        }

        /// <summary>
        ///     Method for restore a database
        /// </summary>
        /// <param name="databaseFullPath">Specifies full path of database(backup)</param>
        /// <param name="databaseName">Specifies database name for restore</param>
        /// <param name="databaseRestorePath">Specifies a path for restore file.</param>
        /// <returns>If success return true</returns>
        // ReSharper disable once UnusedMember.Local
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
                        DatabaseName
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