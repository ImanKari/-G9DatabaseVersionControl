using System;
using System.Runtime.CompilerServices;

namespace G9DatabaseVersionControlCore.Class.SmallLogger.Interface
{
    // ReSharper disable once InconsistentNaming
    public interface G9ISmallLogger
    {
        /// <summary>
        ///     Handle information log
        ///     Used to default instance
        /// </summary>
        /// <param name="message">Information message</param>
        /// <param name="customCallerPath">Custom caller path</param>
        /// <param name="customCallerName">Custom caller name</param>
        /// <param name="customLineNumber">Custom line number</param>
        void G9SmallLogInformation(string message,
            [CallerFilePath] string customCallerPath = null,
            [CallerMemberName] string customCallerName = null,
            [CallerLineNumber] int customLineNumber = 0);

        /// <summary>
        ///     Handle information log
        ///     Used to default instance
        /// </summary>
        /// <param name="message">Information message</param>
        /// <param name="customCallerPath">Custom caller path</param>
        /// <param name="customCallerName">Custom caller name</param>
        /// <param name="customLineNumber">Custom line number</param>
        void G9SmallLogWarning(string message,
            [CallerFilePath] string customCallerPath = null,
            [CallerMemberName] string customCallerName = null,
            [CallerLineNumber] int customLineNumber = 0);

        /// <summary>
        ///     Handle exception log
        ///     Used to default instance
        /// </summary>
        /// <param name="errorMessage">Error message</param>
        /// <param name="customCallerPath">Custom caller path</param>
        /// <param name="customCallerName">Custom caller name</param>
        /// <param name="customLineNumber">Custom line number</param>
        void G9SmallLogError(string errorMessage,
            [CallerFilePath] string customCallerPath = null,
            [CallerMemberName] string customCallerName = null,
            [CallerLineNumber] int customLineNumber = 0);

        /// <summary>
        ///     Handle exception log
        ///     Used to default instance
        /// </summary>
        /// <param name="ex">Exception</param>
        /// <param name="additionalMessage">Additional message</param>
        /// <param name="customCallerPath">Custom caller path</param>
        /// <param name="customCallerName">Custom caller name</param>
        /// <param name="customLineNumber">Custom line number</param>
        void G9SmallLogException(Exception ex, string additionalMessage = null,
            [CallerFilePath] string customCallerPath = null,
            [CallerMemberName] string customCallerName = null,
            [CallerLineNumber] int customLineNumber = 0);
    }
}