﻿using System;
using System.IO;
using System.Linq;
using System.Text;
using G9DatabaseVersionControlCore.Enums;

// ReSharper disable UnusedMember.Global

namespace G9DatabaseVersionControlCore.DataType
{
    /// <summary>
    ///     Data type for map custom data to database control project
    /// </summary>
    public struct G9DtMap
    {
        #region ### Fields And Properties ###

        /// <summary>
        ///     Specifies project name
        /// </summary>
        public readonly string ProjectName;

        /// <summary>
        ///     Specifies database name
        /// </summary>
        public string DatabaseName { private set; get; }

        /// <summary>
        ///     Specifies base database backup path
        /// </summary>
        public readonly string BaseDatabaseBackupPath;

        /// <summary>
        ///     Specifies a func for generate base database script
        ///     <param />
        ///     <param />
        ///     First param: string => Specifies database name (pass automatically on execute func)
        ///     <para />
        ///     Second param: string => Specifies database custom path for restore files (if not set it's null).
        ///     <para />
        ///     Third param: G9DtTaskResult => Specifies generated base database script for return
        /// </summary>
        public readonly Func<string, string, string> GenerateBaseDatabaseScriptFunc;

        /// <summary>
        ///     Specifies a func for create new database
        ///     <param />
        ///     <param />
        ///     First param: string => Specifies database name (pass automatically on execute func)
        ///     <para />
        ///     Second param: string => Specifies database custom path for restore files (if not set it's null).
        ///     <para />
        ///     Third param: object => It can include string connection, connection object, and so on. (For example, for the SQL
        ///     server type, the string connection is passed.)
        ///     <para />
        ///     Fourth param: G9DtTaskResult => Specifies task (create new database) is successful or no (func answer)
        /// </summary>
        public readonly Func<string, string, object, G9DtTaskResult> CreateDatabaseFunc;

        /// <summary>
        ///     specifies base database type for initialize and create
        /// </summary>
        public readonly G9EBaseDatabaseType BaseDatabaseType;

        /// <summary>
        ///     Specifies ability to select the desired name for the database on database create or restore step (default is true)
        /// </summary>
        public readonly bool EnableSetCustomDatabaseName;

        /// <summary>
        ///     Specifies ability to set custom database restore file path (default is true)
        /// </summary>
        public readonly bool EnableSetCustomDatabaseRestoreFilePath;

        /// <summary>
        ///     Specifies database script Requirements
        /// </summary>
        public readonly G9DtMapDatabaseScriptRequirements DatabaseScriptRequirements;

        /// <summary>
        ///     Specifies database update file path (default is 'DatabaseUpdateFiles\')
        /// </summary>
        public readonly string DatabaseUpdateFilesFullPath;

        /// <summary>
        ///     Specifies default schema for create required table (default is 'dbo')
        /// </summary>
        public readonly string DefaultSchemaForTables;

        /// <summary>
        ///     Specifies encoding of update script file (default is 'UTF8')
        /// </summary>
        public readonly Encoding DatabaseUpdateScriptFileEncoding;

        /// <summary>
        ///     Func for specifies product version (default use Assembly version)
        /// </summary>
        public readonly Func<string> ProductVersionFunc;

        /// <summary>
        ///     Collection for custom task
        /// </summary>
        public readonly G9DtCustomTask[] CustomTasks;

        #endregion

        #region ### Methods ###

        /// <summary>
        ///     Constructor - Initialize Requirement
        /// </summary>
        /// <param name="projectName">Specifies project name</param>
        /// <param name="databaseName">Specifies database name</param>
        /// <param name="databaseScriptRequirements">Specifies database script Requirements</param>
        /// <param name="databaseUpdateFilesFullPath">Specifies database update file path (default is 'DatabaseUpdateFiles\')</param>
        /// <param name="defaultSchemaForTables">Specifies default schema for create required table (default is 'dbo')</param>
        /// <param name="databaseUpdateScriptFileEncoding">Specifies encoding of update script file (default is 'UTF8')</param>
        /// <param name="productVersionFunc">Func for specifies product version (default use Assembly version)</param>
        /// <param name="customTasks">Specifies custom tasks</param>
        public G9DtMap(string projectName, string databaseName,
            G9DtMapDatabaseScriptRequirements databaseScriptRequirements, string databaseUpdateFilesFullPath = null,
            string defaultSchemaForTables = null, Encoding databaseUpdateScriptFileEncoding = null,
            Func<string> productVersionFunc = null, params G9DtCustomTask[] customTasks)
        {
            DatabaseUpdateFilesFullPath = string.IsNullOrEmpty(databaseUpdateFilesFullPath)
                ? G9CDatabaseVersionControl.DefaultDatabaseUpdateFilesFullPath
                : databaseUpdateFilesFullPath;

            // Check Common Validation
            CheckCommonValidation(projectName, databaseName, DatabaseUpdateFilesFullPath);

            // Set Requirement
            BaseDatabaseType = G9EBaseDatabaseType.NotSet;
            ProjectName = projectName;
            DatabaseName = databaseName;
            DatabaseScriptRequirements = databaseScriptRequirements;
            CustomTasks = customTasks;
            ProductVersionFunc = productVersionFunc ?? (() => G9CDatabaseVersionControl.DefaultProductVersion);
            DatabaseUpdateScriptFileEncoding = databaseUpdateScriptFileEncoding ?? Encoding.UTF8;
            DefaultSchemaForTables = string.IsNullOrEmpty(defaultSchemaForTables)
                ? G9CDatabaseVersionControl.DefaultSchemaForTables
                : defaultSchemaForTables;
            BaseDatabaseBackupPath = null;
            GenerateBaseDatabaseScriptFunc = null;
            CreateDatabaseFunc = null;
            EnableSetCustomDatabaseName = false;
            EnableSetCustomDatabaseRestoreFilePath = false;
            DatabaseScriptRequirements = databaseScriptRequirements;
        }

        /// <summary>
        ///     Constructor - Initialize Requirement
        ///     <para />
        ///     Map base database backup path for create database
        /// </summary>
        /// <param name="projectName">Specifies project name</param>
        /// <param name="databaseName">Specifies database name</param>
        /// <param name="baseDatabasePath">Specifies base database backup path</param>
        /// <param name="databaseScriptRequirements">Specifies database script Requirements</param>
        /// <param name="databaseUpdateFilesFullPath">Specifies database update file path (default is 'DatabaseUpdateFiles\')</param>
        /// <param name="enableCustomDatabaseName">
        ///     Specifies ability to select the desired name for the database on database create
        ///     or restore step (default is true)
        /// </param>
        /// <param name="defaultSchemaForTables">Specifies default schema for create required table (default is 'dbo')</param>
        /// <param name="databaseUpdateScriptFileEncoding">Specifies encoding of update script file (default is 'UTF8')</param>
        /// <param name="productVersionFunc">Func for specifies product version (default use Assembly version)</param>
        /// <param name="customTasks">Specifies custom tasks</param>
        public G9DtMap(string projectName, string databaseName, string baseDatabasePath,
            G9DtMapDatabaseScriptRequirements databaseScriptRequirements, string databaseUpdateFilesFullPath = null,
            bool enableCustomDatabaseName = true, string defaultSchemaForTables = null,
            Encoding databaseUpdateScriptFileEncoding = null, Func<string> productVersionFunc = null,
            params G9DtCustomTask[] customTasks)
        {
            DatabaseUpdateFilesFullPath = string.IsNullOrEmpty(databaseUpdateFilesFullPath)
                ? G9CDatabaseVersionControl.DefaultDatabaseUpdateFilesFullPath
                : databaseUpdateFilesFullPath;

            // Check Common Validation
            CheckCommonValidation(projectName, databaseName, DatabaseUpdateFilesFullPath);

            // Check Special Validation
            if (string.IsNullOrEmpty(baseDatabasePath))
                throw new ArgumentNullException(nameof(baseDatabasePath),
                    $"Param '{nameof(baseDatabasePath)}' can't be null or empty");
            if (!File.Exists(baseDatabasePath))
                throw new ArgumentException(nameof(baseDatabasePath),
                    $"The base database backup path is incorrect! File not exist: '{baseDatabasePath}'");

            // Set Requirement
            BaseDatabaseType = G9EBaseDatabaseType.CreateBaseDatabaseByBackupDatabasePath;
            ProjectName = projectName;
            DatabaseName = databaseName;
            BaseDatabaseBackupPath = baseDatabasePath;
            DatabaseScriptRequirements = databaseScriptRequirements;
            CustomTasks = customTasks;
            EnableSetCustomDatabaseRestoreFilePath = false;
            ProductVersionFunc = productVersionFunc ?? (() => G9CDatabaseVersionControl.DefaultProductVersion);
            DatabaseUpdateScriptFileEncoding = databaseUpdateScriptFileEncoding ?? Encoding.UTF8;
            DefaultSchemaForTables = string.IsNullOrEmpty(defaultSchemaForTables)
                ? G9CDatabaseVersionControl.DefaultSchemaForTables
                : defaultSchemaForTables;
            GenerateBaseDatabaseScriptFunc = null;
            CreateDatabaseFunc = null;
            EnableSetCustomDatabaseName = enableCustomDatabaseName;
        }

        /// <summary>
        ///     Constructor - Initialize Requirement
        ///     <para />
        ///     Map base database backup path for create database
        /// </summary>
        /// <param name="projectName">Specifies project name</param>
        /// <param name="databaseName">Specifies database name</param>
        /// <param name="generateBaseDatabaseScriptFunc">
        ///     Specifies a func for generate base database script
        ///     <param />
        ///     <param />
        ///     First param: string => Specifies database name (pass automatically on execute func)
        ///     <para />
        ///     Second param: string => Specifies database custom path for restore files (if not set it's null).
        ///     <para />
        ///     Third param: G9DtTaskResult => Specifies generated base database script for return
        /// </param>
        /// <param name="databaseScriptRequirements">Specifies database script Requirements</param>
        /// <param name="databaseUpdateFilesFullPath">Specifies database update file path (default is 'DatabaseUpdateFiles\')</param>
        /// <param name="enableCustomDatabaseName">
        ///     Specifies ability to select the desired name for the database on database create
        ///     or restore step (default is true)
        /// </param>
        /// <param name="enableSetCustomDatabaseRestoreFilePath">
        ///     Specifies ability to set custom database restore file path
        ///     (default is true)
        /// </param>
        /// <param name="defaultSchemaForTables">Specifies default schema for create required table (default is 'dbo')</param>
        /// <param name="databaseUpdateScriptFileEncoding">Specifies encoding of update script file (default is 'UTF8')</param>
        /// <param name="productVersionFunc">Func for specifies product version (default use Assembly version)</param>
        /// <param name="customTasks">Specifies custom tasks</param>
        public G9DtMap(string projectName, string databaseName,
            Func<string, string, string> generateBaseDatabaseScriptFunc,
            G9DtMapDatabaseScriptRequirements databaseScriptRequirements, string databaseUpdateFilesFullPath = null,
            bool enableCustomDatabaseName = true, bool enableSetCustomDatabaseRestoreFilePath = true,
            string defaultSchemaForTables = null,
            Encoding databaseUpdateScriptFileEncoding = null, Func<string> productVersionFunc = null,
            params G9DtCustomTask[] customTasks)
        {
            DatabaseUpdateFilesFullPath = string.IsNullOrEmpty(databaseUpdateFilesFullPath)
                ? G9CDatabaseVersionControl.DefaultDatabaseUpdateFilesFullPath
                : databaseUpdateFilesFullPath;

            // Check Common Validation
            CheckCommonValidation(projectName, databaseName, DatabaseUpdateFilesFullPath);

            // Set Requirement
            BaseDatabaseType = G9EBaseDatabaseType.CreateBaseDatabaseByScriptData;
            ProjectName = projectName;
            DatabaseName = databaseName;
            BaseDatabaseBackupPath = null;
            GenerateBaseDatabaseScriptFunc = generateBaseDatabaseScriptFunc ?? throw new ArgumentNullException(
                nameof(generateBaseDatabaseScriptFunc),
                $"Param '{nameof(generateBaseDatabaseScriptFunc)}' can't be null!");
            DatabaseScriptRequirements = databaseScriptRequirements;
            CustomTasks = customTasks;
            EnableSetCustomDatabaseRestoreFilePath = enableSetCustomDatabaseRestoreFilePath;
            ProductVersionFunc = productVersionFunc ?? (() => G9CDatabaseVersionControl.DefaultProductVersion);
            DatabaseUpdateScriptFileEncoding = databaseUpdateScriptFileEncoding ?? Encoding.UTF8;
            DefaultSchemaForTables = string.IsNullOrEmpty(defaultSchemaForTables)
                ? G9CDatabaseVersionControl.DefaultSchemaForTables
                : defaultSchemaForTables;
            CreateDatabaseFunc = null;
            EnableSetCustomDatabaseName = enableCustomDatabaseName;
        }

        /// <summary>
        ///     Constructor - Map by base database path
        /// </summary>
        /// <param name="projectName">Specifies project name</param>
        /// <param name="databaseName">Specifies database name</param>
        /// <param name="createDatabaseFunc">
        ///     Specifies a func for create new database
        ///     <param />
        ///     <param />
        ///     First param: string => Specifies database name (pass automatically on execute func)
        ///     <para />
        ///     Second param: string => Specifies database custom path for restore files (if not set it's null).
        ///     <para />
        ///     Third param: object => It can include string connection, connection object, and so on. (For example, for the SQL
        ///     server type, the string connection is passed.)
        ///     <para />
        ///     Fourth param: G9DtTaskResult => Specifies task (create new database) is successful or no (func answer)
        /// </param>
        /// <param name="databaseScriptRequirements">Specifies database script Requirements</param>
        /// <param name="databaseUpdateFilesFullPath">
        ///     Specifies database update file path (default is 'DatabaseUpdateFiles\')
        ///     (default is 'DatabaseUpdateFiles\')
        /// </param>
        /// <param name="enableCustomDatabaseName">
        ///     Specifies ability to select the desired name for the database on database create
        ///     or restore step (default is true)
        /// </param>
        /// <param name="enableSetCustomDatabaseRestoreFilePath">
        ///     Specifies ability to set custom database restore file path
        ///     (default is true)
        /// </param>
        /// <param name="defaultSchemaForTables">Specifies default schema for create required table (default is 'dbo')</param>
        /// <param name="databaseUpdateScriptFileEncoding">Specifies encoding of update script file (default is 'UTF8')</param>
        /// <param name="productVersionFunc">Func for specifies product version (default use Assembly version)</param>
        /// <param name="customTasks">Specifies custom tasks</param>
        public G9DtMap(string projectName, string databaseName,
            Func<string, string, object, G9DtTaskResult> createDatabaseFunc,
            G9DtMapDatabaseScriptRequirements databaseScriptRequirements, string databaseUpdateFilesFullPath = null,
            bool enableCustomDatabaseName = true, bool enableSetCustomDatabaseRestoreFilePath = true,
            string defaultSchemaForTables = null,
            Encoding databaseUpdateScriptFileEncoding = null, Func<string> productVersionFunc = null,
            params G9DtCustomTask[] customTasks)
        {
            DatabaseUpdateFilesFullPath = string.IsNullOrEmpty(databaseUpdateFilesFullPath)
                ? G9CDatabaseVersionControl.DefaultDatabaseUpdateFilesFullPath
                : databaseUpdateFilesFullPath;

            // Check Common Validation
            CheckCommonValidation(projectName, databaseName, DatabaseUpdateFilesFullPath);

            // Set Requirement
            BaseDatabaseType = G9EBaseDatabaseType.CreateBaseDatabaseByFunc;
            ProjectName = projectName;
            DatabaseName = databaseName;
            BaseDatabaseBackupPath = null;
            GenerateBaseDatabaseScriptFunc = null;
            CreateDatabaseFunc = createDatabaseFunc ?? throw new ArgumentNullException(nameof(createDatabaseFunc),
                $"Param '{nameof(createDatabaseFunc)}' can't be null!");
            DatabaseScriptRequirements = databaseScriptRequirements;
            CustomTasks = customTasks;
            EnableSetCustomDatabaseRestoreFilePath = enableSetCustomDatabaseRestoreFilePath;
            ProductVersionFunc = productVersionFunc ?? (() => G9CDatabaseVersionControl.DefaultProductVersion);
            DatabaseUpdateScriptFileEncoding = databaseUpdateScriptFileEncoding ?? Encoding.UTF8;
            DefaultSchemaForTables = string.IsNullOrEmpty(defaultSchemaForTables)
                ? G9CDatabaseVersionControl.DefaultSchemaForTables
                : defaultSchemaForTables;
            EnableSetCustomDatabaseName = enableCustomDatabaseName;
        }

        /// <summary>
        ///     Method to check common validation
        /// </summary>
        /// <param name="projectName">Specifies project name</param>
        /// <param name="databaseName">Specifies database name</param>
        /// <param name="databaseUpdateFilesFullPath">Specifies database update file path</param>
        private static void CheckCommonValidation(string projectName, string databaseName,
            string databaseUpdateFilesFullPath)
        {
            if (string.IsNullOrEmpty(projectName))
                throw new ArgumentNullException(nameof(projectName),
                    $"Param '{nameof(projectName)}' can't be null or empty");
            if (G9CDatabaseVersionControl.GetTotalProjectNames(databaseUpdateFilesFullPath).All(s => s != projectName))
                throw new ArgumentException(nameof(G9DtMap),
                    $"The name specified for the project by parameter '{nameof(projectName)}' could not be found. Please use the '{nameof(G9CDatabaseVersionControl.GetTotalProjectNames)}' function to get the names of the projects or check update project path: '{databaseUpdateFilesFullPath}'.");
            if (string.IsNullOrEmpty(databaseName))
                throw new ArgumentNullException(nameof(databaseName),
                    $"Param '{nameof(databaseName)}' can't be null or empty");
        }

        /// <summary>
        ///     Method to change database name
        /// </summary>
        /// <param name="newDatabaseName">Specifies new database name</param>
        public void ChangeDatabaseName(string newDatabaseName)
        {
            DatabaseName = newDatabaseName;
        }

        #endregion
    }
}