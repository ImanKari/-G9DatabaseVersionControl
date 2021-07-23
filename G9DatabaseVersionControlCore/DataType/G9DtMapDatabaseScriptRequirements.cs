namespace G9DatabaseVersionControlCore.DataType
{
    /// <summary>
    ///     Data Type for specifies database script Requirements
    /// </summary>
    public readonly struct G9DtMapDatabaseScriptRequirements
    {
        #region ### Fields And Properties ###

        /// <summary>
        ///     Specifies 'Is Required To Set Author?'
        /// </summary>
        public readonly bool IsRequiredToSetAuthor;

        /// <summary>
        ///     Specifies 'Is Required To Set Description?'
        /// </summary>
        public readonly bool IsRequiredToSetDescription;

        /// <summary>
        ///     Specifies 'Is Required To Set UpdateDateTime?'
        /// </summary>
        public readonly bool IsRequiredToSetUpdateDateTime;

        /// <summary>
        ///     Specifies 'Is Required To Set Version?'
        /// </summary>
        public readonly bool IsRequiredToSetVersion;

        /// <summary>
        ///     Specifies need to save the updated script data in log table 'G9DatabaseVersionUpdateHistory'
        /// </summary>
        public readonly bool NeedToSaveUpdatedScriptData;

        /// <summary>
        ///     It specifies whether to check and remove the phrase "Go" in update scripts or not (Just in run time, The original
        ///     file will not be edited)
        ///     <para />
        ///     Notice: This setting is for SQL Server only!
        ///     <para />
        ///     <see href="https://stackoverflow.com/questions/25680812/incorrect-syntax-near-go">Incorrect syntax near 'GO'</see>
        /// </summary>
        public readonly bool IsRequiredToRemoveGoPhrase;

        #endregion

        #region ### Methods ###

        /// <summary>
        ///     Constructor - Set requirements
        /// </summary>
        /// <param name="isRequiredToSetAuthor">Specifies 'Is Required To Set Author?'</param>
        /// <param name="isRequiredToSetDescription">Specifies 'Is Required To Set Description?'</param>
        /// <param name="isRequiredToSetUpdateDateTime">Specifies 'Is Required To Set UpdateDateTime?'</param>
        /// <param name="isRequiredToSetVersion">Specifies 'Is Required To Set Version?'</param>
        /// <param name="needToSaveUpdatedScriptData">
        ///     Specifies need to save the updated script data in log table
        ///     'G9DatabaseVersionUpdateHistory'
        /// </param>
        /// <param name="isRequiredToRemoveGoPhrase">
        ///     It specifies whether to check and remove the phrase "Go" in update scripts or not (Just in run time, The original
        ///     file will not be edited)
        ///     <para />
        ///     Notice: This setting is for SQL Server only!
        ///     <para />
        ///     <see href="https://stackoverflow.com/questions/25680812/incorrect-syntax-near-go">Incorrect syntax near 'GO'</see>
        /// </param>
        public G9DtMapDatabaseScriptRequirements(bool isRequiredToSetAuthor = false,
            bool isRequiredToSetDescription = false, bool isRequiredToSetUpdateDateTime = false,
            bool isRequiredToSetVersion = false, bool needToSaveUpdatedScriptData = false,
            bool isRequiredToRemoveGoPhrase = false)
        {
            IsRequiredToSetAuthor = isRequiredToSetAuthor;
            IsRequiredToSetDescription = isRequiredToSetDescription;
            IsRequiredToSetUpdateDateTime = isRequiredToSetUpdateDateTime;
            IsRequiredToSetVersion = isRequiredToSetVersion;
            NeedToSaveUpdatedScriptData = needToSaveUpdatedScriptData;
            IsRequiredToRemoveGoPhrase = isRequiredToRemoveGoPhrase;
        }

        #endregion
    }
}