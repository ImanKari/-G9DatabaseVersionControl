using System;
using System.Collections.Generic;

namespace G9DatabaseVersionControlCore.DataType
{
    /// <summary>
    ///     Data type for update project info
    /// </summary>
    public readonly struct G9DtProjectInfo
    {
        #region ### Fields And Properties ###

        /// <summary>
        ///     Specifies full path of project
        /// </summary>
        public readonly string ProjectFullPath;

        /// <summary>
        ///     Specifies the project name
        /// </summary>
        public readonly string ProjectName;

        /// <summary>
        ///     Specified Update folders info for this project
        /// </summary>
        public readonly IList<G9DtUpdateFoldersInfo> UpdateFoldersInfo;

        /// <summary>
        ///     Specifies current version of database for this project
        /// </summary>
        public readonly string CurrentVersion;

        /// <summary>
        ///     Specifies last update date time of database for this project
        /// </summary>
        public readonly DateTime LastUpdateDateTime;

        #endregion

        #region ### Methods ###

        /// <summary>
        ///     Constructor - Initialize requirement
        /// </summary>
        /// <param name="projectFullPath">Specifies full path of project</param>
        /// <param name="projectName">Specifies the project name</param>
        /// <param name="currentVersion">Specifies current version of database for this project</param>
        /// <param name="lastUpdateDateTime">Specifies last update date time of database for this project</param>
        /// <param name="updateFoldersInfo">Specified Update folders info for this project</param>
        public G9DtProjectInfo(string projectFullPath, string projectName, string currentVersion,
            DateTime lastUpdateDateTime,
            IList<G9DtUpdateFoldersInfo> updateFoldersInfo)
        {
            ProjectName = projectName;
            UpdateFoldersInfo = updateFoldersInfo;
            ProjectFullPath = projectFullPath;
            CurrentVersion = currentVersion;
            LastUpdateDateTime = lastUpdateDateTime;
        }

        #endregion
    }
}