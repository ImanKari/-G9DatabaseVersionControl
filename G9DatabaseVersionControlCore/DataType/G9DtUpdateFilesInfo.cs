using System;

namespace G9DatabaseVersionControlCore.DataType
{
    /// <summary>
    ///     Data type for update files info
    /// </summary>
    public readonly struct G9DtUpdateFilesInfo
    {
        #region ### Fields And Properties ###

        /// <summary>
        ///     Specifies the update file full path
        /// </summary>
        public readonly string UpdateFileFullPath;

        /// <summary>
        ///     Specifies the update file name
        /// </summary>
        public readonly string UpdateFileName;

        /// <summary>
        ///     Specifies author of file
        /// </summary>
        public readonly string Author;

        /// <summary>
        ///     Specifies description of file
        /// </summary>
        public readonly string Description;

        /// <summary>
        ///     Specifies update date time for the update file
        /// </summary>
        public readonly DateTime UpdateDateTime;

        /// <summary>
        ///     Specifies version for the update file
        /// </summary>
        public readonly string Version;

        /// <summary>
        ///     Specifies file data
        /// </summary>
        public readonly string UpdateFileData;

        #endregion

        #region ### Methods ###

        /// <summary>
        ///     Constructor - Initialize requirement
        /// </summary>
        /// <param name="updateFileFullPath">Specifies the update file full path</param>
        /// <param name="updateFileName">Specifies the update file name</param>
        /// <param name="author">Specifies author of file</param>
        /// <param name="description">Specifies description of file</param>
        /// <param name="updateDateTime">Specifies update date time for the update file</param>
        /// <param name="version">Specifies version for the update file</param>
        /// <param name="updateFileData">Specifies file data</param>
        public G9DtUpdateFilesInfo(string updateFileFullPath, string updateFileName, string author, string description,
            DateTime updateDateTime, string version, string updateFileData)
        {
            UpdateFileFullPath = updateFileFullPath;
            UpdateFileName = updateFileName;
            Author = author;
            Description = description;
            UpdateDateTime = updateDateTime;
            Version = version;
            UpdateFileData = updateFileData;
        }

        #endregion
    }
}