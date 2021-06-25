namespace G9DatabaseVersionControlCore.Enums
{
    /// <summary>
    ///     Enum specifies base database type for initialize and create
    /// </summary>
    public enum G9EBaseDatabaseType : byte
    {
        /// <summary>
        ///     Not set
        /// </summary>
        NotSet,

        /// <summary>
        ///     Create base database by backup database path
        /// </summary>
        CreateBaseDatabaseByBackupDatabasePath,

        /// <summary>
        ///     Create base database by create script data
        /// </summary>
        CreateBaseDatabaseByScriptData,

        /// <summary>
        ///     Create base database by received func
        /// </summary>
        CreateBaseDatabaseByFunc
    }
}