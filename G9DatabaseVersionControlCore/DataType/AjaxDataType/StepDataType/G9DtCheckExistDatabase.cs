namespace G9DatabaseVersionControlCore.DataType.AjaxDataType.StepDataType
{
    /// <summary>
    ///     Data type for check exist database
    /// </summary>
    public struct G9DtCheckExistDatabase
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
    }
}