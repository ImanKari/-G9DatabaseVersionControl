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

        #endregion

        #region ### Methods ###

        /// <summary>
        ///     Constructor - Set requirements
        /// </summary>
        /// <param name="isRequiredToSetAuthor">Specifies 'Is Required To Set Author?'</param>
        /// <param name="isRequiredToSetDescription">Specifies 'Is Required To Set Description?'</param>
        /// <param name="isRequiredToSetUpdateDateTime">Specifies 'Is Required To Set UpdateDateTime?'</param>
        /// <param name="isRequiredToSetVersion">Specifies 'Is Required To Set Version?'</param>
        public G9DtMapDatabaseScriptRequirements(bool isRequiredToSetAuthor = false,
            bool isRequiredToSetDescription = false,
            bool isRequiredToSetUpdateDateTime = false, bool isRequiredToSetVersion = false)
        {
            IsRequiredToSetAuthor = isRequiredToSetAuthor;
            IsRequiredToSetDescription = isRequiredToSetDescription;
            IsRequiredToSetUpdateDateTime = isRequiredToSetUpdateDateTime;
            IsRequiredToSetVersion = isRequiredToSetVersion;
        }

        #endregion
    }
}