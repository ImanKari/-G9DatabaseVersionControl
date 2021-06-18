using G9DatabaseVersionControlCore.Enums;

namespace G9DatabaseVersionControlCore.DataType.AjaxDataType
{
    public struct G9DtTaskAnswer
    {
        /// <summary>
        ///     Specifies task is success
        /// </summary>
        public bool Success { set; get; }

        /// <summary>
        ///     Specifies need to show a message
        /// </summary>
        public bool NeedShowMessage { set; get; }

        /// <summary>
        ///     Specifies message for show
        /// </summary>
        public string Message { set; get; }

        /// <summary>
        ///     Specifies fatal error
        /// </summary>
        public bool FatalErrorStopInstall { set; get; }

        /// <summary>
        ///     Specifies number of error script
        /// </summary>
        public int NumberOfErrorScript { set; get; }

        /// <summary>
        ///     Specifies need tp accept or denied
        /// </summary>
        public bool ShowAcceptAndDeniedBTN { set; get; }

        /// <summary>
        ///     Specifies task status
        /// </summary>
        public G9ETaskStatus StepOfInstall { set; get; }

        /// <summary>
        ///     Specifies percent for current step
        /// </summary>
        public double PercentCurrectStep { set; get; }

        /// <summary>
        ///     Specifies row numbers for update
        /// </summary>
        public long RowReceiveNumberCount { set; get; }
    }
}