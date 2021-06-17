using System;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using G9DatabaseVersionControlCore.Class.SmallLogger.Enums;

namespace G9DatabaseVersionControlCore.Class.SmallLogger
{
    /// <summary>
    ///     A small script for logging
    /// </summary>
    public static class G9CSmallLogger
    {
        #region ### Fields And Properties ###

        /// <summary>
        ///     Specifies the logger is initialized or no
        /// </summary>
        public static bool IsInitialize { private set; get; }

        /// <summary>
        ///     Specified log header for info
        /// </summary>
        public const string InformationHeader = "############################## INFO ##############################";

        /// <summary>
        ///     Specified warning header for info
        /// </summary>
        public const string WarningHeader = "############################## WARNING ##############################";

        /// <summary>
        ///     Specified log header for error
        /// </summary>
        public const string ErrorHeader = "############################## ERROR ##############################";

        /// <summary>
        ///     Specified log header for exception
        /// </summary>
        public const string ExceptionHeader = "############################## Exception ##############################";

        /// <summary>
        ///     Encoding for log write
        /// </summary>
        public static Encoding Encoding { private set; get; }

        /// <summary>
        ///     Specified log path
        /// </summary>
        public static string LogPath { private set; get; }

        /// <summary>
        ///     Specified log directory name
        /// </summary>
        public static string LogDirectoryName { private set; get; }

        /// <summary>
        ///     Specified log file name
        /// </summary>
        public static string LogFileName { private set; get; }

        /// <summary>
        ///     Specified log full path
        /// </summary>
        public static string LogFullPath { private set; get; }

        /// <summary>
        ///     Use lock
        /// </summary>
        private static readonly object WriteFileLock = new object();

        #endregion

        #region ### Methods ###

        /// <summary>
        ///     Constructor - Initialize requirement
        /// </summary>
        static G9CSmallLogger()
        {
            InitializeRequirement();
        }

        /// <summary>
        ///     Method for initialize logger
        /// </summary>
        /// <param name="logPath">Log path</param>
        /// <param name="logDirectoryName">Log directory name</param>
        /// <param name="logFileName">Log file name</param>
        /// <param name="encoding">Specify the encoding of log</param>
        public static void Initialize(string logPath = null, string logDirectoryName = null, string logFileName = null,
            Encoding encoding = null)
        {
            InitializeRequirement(logPath, logDirectoryName, logFileName, encoding);
        }

        /// <summary>
        ///     Method for initialize logger
        /// </summary>
        /// <param name="logPath">Log path</param>
        /// <param name="logDirectoryName">Log directory name</param>
        /// <param name="logFileName">Log file name</param>
        /// <param name="encoding">Specify the encoding of log</param>
        private static void InitializeRequirement(string logPath = null, string logDirectoryName = null,
            string logFileName = null, Encoding encoding = null)
        {
            LogPath = logPath ?? string.Empty;
            LogDirectoryName = logDirectoryName ?? "DatabaseUpdateLogs";
            LogFileName = string.IsNullOrEmpty(logFileName)
                ? $"{DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss-fff", CultureInfo.InvariantCulture)}.txt"
                : $"{logFileName}.txt";
            LogFullPath = Path.Combine(LogPath, LogDirectoryName);
            Encoding = encoding ?? new UTF8Encoding(true);
            if (!Directory.Exists(LogFullPath))
                Directory.CreateDirectory(LogFullPath);
            LogFullPath = Path.Combine(LogFullPath, LogFileName);
            IsInitialize = true;
        }

        /// <summary>
        ///     Method for check requirement
        /// </summary>
        private static void CheckRequirement()
        {
            if (!IsInitialize)
                throw new TypeInitializationException(nameof(G9CSmallLogger),
                    new Exception(
                        $"Please initialize '{nameof(G9CSmallLogger)}'.\nUse '{nameof(Initialize)}' method for initialized this type."));
        }

        /// <summary>
        ///     Handle information log
        ///     Used to default instance
        /// </summary>
        /// <param name="message">Information message</param>
        /// <param name="customCallerPath">Custom caller path</param>
        /// <param name="customCallerName">Custom caller name</param>
        /// <param name="customLineNumber">Custom line number</param>
        public static void G9SmallLogInformation(this string message,
            [CallerFilePath] string customCallerPath = null,
            [CallerMemberName] string customCallerName = null,
            [CallerLineNumber] int customLineNumber = 0)
        {
            CheckRequirement();
            WriteLogsToStream(
                $"{message}\nFile Name: {customCallerPath}\tMethod Base: {customCallerName}\tLine Number: {customLineNumber}",
                G9ESmallLogTypes.Information);
        }

        /// <summary>
        ///     Handle information log
        ///     Used to default instance
        /// </summary>
        /// <param name="message">Information message</param>
        /// <param name="customCallerPath">Custom caller path</param>
        /// <param name="customCallerName">Custom caller name</param>
        /// <param name="customLineNumber">Custom line number</param>
        public static void G9SmallLogWarning(this string message,
            [CallerFilePath] string customCallerPath = null,
            [CallerMemberName] string customCallerName = null,
            [CallerLineNumber] int customLineNumber = 0)
        {
            CheckRequirement();
            WriteLogsToStream(
                $"{message}\nFile Name: {customCallerPath}\tMethod Base: {customCallerName}\tLine Number: {customLineNumber}",
                G9ESmallLogTypes.Warning);
        }

        /// <summary>
        ///     Handle exception log
        ///     Used to default instance
        /// </summary>
        /// <param name="errorMessage">Error message</param>
        /// <param name="customCallerPath">Custom caller path</param>
        /// <param name="customCallerName">Custom caller name</param>
        /// <param name="customLineNumber">Custom line number</param>
        public static void G9SmallLogError(this string errorMessage,
            [CallerFilePath] string customCallerPath = null,
            [CallerMemberName] string customCallerName = null,
            [CallerLineNumber] int customLineNumber = 0)
        {
            CheckRequirement();
            WriteLogsToStream(
                $"{errorMessage}\nFile Name: {customCallerPath}\tMethod Base: {customCallerName}\tLine Number: {customLineNumber}",
                G9ESmallLogTypes.Error);
        }

        /// <summary>
        ///     Handle exception log
        ///     Used to default instance
        /// </summary>
        /// <param name="ex">Exception</param>
        /// <param name="additionalMessage">Additional message</param>
        /// <param name="customCallerPath">Custom caller path</param>
        /// <param name="customCallerName">Custom caller name</param>
        /// <param name="customLineNumber">Custom line number</param>
        public static void G9SmallLogException(this Exception ex, string additionalMessage = null,
            [CallerFilePath] string customCallerPath = null,
            [CallerMemberName] string customCallerName = null,
            [CallerLineNumber] int customLineNumber = 0)
        {
            CheckRequirement();
            var receiveException = ex.InnerException;
            var exceptionMessage = $"Exception Message: {ex.Message}";
            while (receiveException != null)
            {
                exceptionMessage += $"Inner Exception Message: {receiveException.Message}\n";
                receiveException = receiveException.InnerException;
            }

            if (!string.IsNullOrEmpty(additionalMessage))
                exceptionMessage += $"Additional Message: {additionalMessage}";

            WriteLogsToStream(
                $"{exceptionMessage}\nFile Name: {customCallerPath}\tMethod Base: {customCallerName}\tLine Number: {customLineNumber}",
                G9ESmallLogTypes.Exception);
        }

        /// <summary>
        ///     Write log item in file stream
        /// </summary>
        /// <param name="logItemData">Log item data</param>
        /// <param name="logType">Specify the log type</param>
        /// <param name="innerLog">Specify log is inner system log</param>
        private static void WriteLogsToStream(string logItemData, G9ESmallLogTypes logType, bool innerLog = false)
        {
            try
            {
                var sb = new StringBuilder();
                switch (logType)
                {
                    case G9ESmallLogTypes.Information:
                        sb.Append(InformationHeader);
                        break;
                    case G9ESmallLogTypes.Warning:
                        sb.Append(WarningHeader);
                        break;
                    case G9ESmallLogTypes.Error:
                        sb.Append(ErrorHeader);
                        break;
                    case G9ESmallLogTypes.Exception:
                        sb.Append(ExceptionHeader);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(logType), logType, null);
                }

                sb.Append($"\n[{DateTime.Now:yyyy-MM-dd HH:mm:ss}]: {logItemData}\n\n");

                lock (WriteFileLock)
                {
                    using (var os = new FileStream(LogFullPath, FileMode.OpenOrCreate))
                    {
                        os.Seek(0, SeekOrigin.End);
                        var dataByte = Encoding.GetBytes(sb.ToString());
                        os.Write(dataByte, 0, dataByte.Length);
                    }
                }
            }
            catch (Exception ex)
            {
                if (!innerLog)
                    WriteLogsToStream($"Raise Exception In {nameof(WriteLogsToStream)}\n{ex.Message}",
                        G9ESmallLogTypes.Exception, true);
                else
                    throw;
            }
        }

        #endregion
    }
}