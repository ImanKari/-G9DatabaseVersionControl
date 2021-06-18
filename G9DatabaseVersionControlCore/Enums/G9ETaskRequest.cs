namespace G9DatabaseVersionControlCore.Enums
{
    /// <summary>
    ///     Enum to specifies task request
    /// </summary>
    public enum G9ETaskRequest : byte
    {
        /// <summary>
        ///     Request for enter connection string
        /// </summary>
        EnterConnectionString,

        /// <summary>
        ///     Request for check exist database
        /// </summary>
        CheckExistDatabase,

        /// <summary>
        ///     Request for install software
        /// </summary>
        InstallSoftware,

        /// <summary>
        ///     Request for update
        /// </summary>
        UpdateSoftware,

        /// <summary>
        ///     Request for convert old database data to new database data
        /// </summary>
        ConvertOldDatabaseToNewDatabase
    }
}