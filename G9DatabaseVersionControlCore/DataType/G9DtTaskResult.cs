namespace G9DatabaseVersionControlCore.DataType
{
    /// <summary>
    ///     Data type for task result
    /// </summary>
    public class G9DtTaskResult
    {
        #region ### Fields And Properties ###

        /// <summary>
        ///     Specifies Task is Successful
        /// </summary>
        public readonly bool IsSuccessful;

        /// <summary>
        ///     Specifies message on the unsuccessful task
        /// </summary>
        public readonly string OnUnsuccessfulMessage;

        #endregion

        #region ### Methods ###

        /// <summary>
        ///     Constructor - Use for unsuccessful task result
        /// </summary>
        /// <param name="onUnsuccessfulMessage">Specifies message on the unsuccessful task.</param>
        public G9DtTaskResult(string onUnsuccessfulMessage)
        {
            IsSuccessful = false;
            OnUnsuccessfulMessage = onUnsuccessfulMessage;
        }

        /// <summary>
        ///     Constructor - Use for successful task result
        /// </summary>
        public G9DtTaskResult()
        {
            IsSuccessful = true;
            OnUnsuccessfulMessage = string.Empty;
        }

        #endregion
    }
}