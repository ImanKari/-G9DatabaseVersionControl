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
        ///     Request for install and update software
        /// </summary>
        InstallSoftwareAndUpdate,

        /// <summary>
        ///     Request for update
        /// </summary>
        UpdateSoftware,

        /// <summary>
        ///     Request for custom task
        /// </summary>
        CustomTask,

        /// <summary>
        /// Request for get last status
        /// </summary>
        CheckLastStatus
    }
}