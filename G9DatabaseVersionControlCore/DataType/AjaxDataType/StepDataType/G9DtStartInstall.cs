namespace G9DatabaseVersionControlCore.DataType.AjaxDataType.StepDataType
{
    public struct G9DtStartInstallOrUpdate
    {
        /// <summary>
        ///     Specifies data source for connection string
        /// </summary>
        public string DataSource;

        /// <summary>
        ///     Specifies user id for connection string
        /// </summary>
        public string UserId;

        /// <summary>
        ///     Specifies password for connection string
        /// </summary>
        public string Password;

        /// <summary>
        ///     Specifies database name
        /// </summary>
        public string DatabaseName;

        /// <summary>
        ///     Specifies project name
        /// </summary>
        public string ProjectName;

        /// <summary>
        ///     Specifies Custom Database Restore Path
        /// </summary>
        public string CustomDatabaseRestorePath;

        /// <summary>
        ///     Specifies nick name of custom task
        /// </summary>
        public string CustomTaskNickname;
    }
}