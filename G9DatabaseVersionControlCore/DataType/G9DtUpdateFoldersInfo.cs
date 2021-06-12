using System;
using System.Collections.Generic;

namespace G9DatabaseVersionControlCore.DataType
{
    /// <summary>
    ///     Data type for update folders info
    /// </summary>
    public readonly struct G9DtUpdateFoldersInfo
    {
        #region ### Fields And Properties ###

        /// <summary>
        ///     Specifies the folder full path
        /// </summary>
        public readonly string FolderFullPath;

        /// <summary>
        ///     Specifies the folder name
        /// </summary>
        public readonly string FolderName;

        /// <summary>
        ///     Specifies version for this folder
        /// </summary>
        public readonly string FolderVersion;

        /// <summary>
        ///     Specifies date time for this folder
        /// </summary>
        public readonly DateTime FolderDateTime;

        /// <summary>
        ///     Specifies update file info in this folder
        /// </summary>
        public readonly IList<G9DtUpdateFilesInfo> UpdateFilesInfos;

        #endregion

        #region ### Methods ###

        /// <summary>
        ///     Constructor - Initialize requirement
        /// </summary>
        /// <param name="folderFullPath">Specifies the folder full path</param>
        /// <param name="folderName">Specifies the folder name</param>
        /// <param name="folderVersion">Specifies version for this folder</param>
        /// <param name="folderDateTime">Specifies update file info in this folder</param>
        /// <param name="updateFilesInfos">Specifies update file info in this folder</param>
        public G9DtUpdateFoldersInfo(string folderFullPath, string folderName,
            string folderVersion, DateTime folderDateTime,
            IList<G9DtUpdateFilesInfo> updateFilesInfos)
        {
            FolderFullPath = folderFullPath;
            FolderName = folderName;
            UpdateFilesInfos = updateFilesInfos;
            FolderVersion = folderVersion;
            FolderDateTime = folderDateTime;
        }

        #endregion
    }
}