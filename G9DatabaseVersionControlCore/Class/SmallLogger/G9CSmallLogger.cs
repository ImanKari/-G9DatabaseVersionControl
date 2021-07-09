using System;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using G9DatabaseVersionControlCore.Class.SmallLogger.Enums;
using G9DatabaseVersionControlCore.Class.SmallLogger.Interface;

namespace G9DatabaseVersionControlCore.Class.SmallLogger
{
    /// <summary>
    ///     A small script for logging
    /// </summary>
    public class G9CSmallLogger : G9ISmallLogger
    {
        #region ### Fields And Properties ###

        /// <summary>
        ///     Specifies the logger is initialized or no
        /// </summary>
        public bool IsInitialize { private set; get; }

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
        public Encoding Encoding { private set; get; }

        /// <summary>
        ///     Specified log path
        /// </summary>
        public string LogPath { private set; get; }

        /// <summary>
        ///     Specified log directory name
        /// </summary>
        public string LogDirectoryName { private set; get; }

        /// <summary>
        ///     Specified log file name
        /// </summary>
        public string LogFileName { private set; get; }

        /// <summary>
        ///     Specified log full path
        /// </summary>
        public string LogFullPath { private set; get; }

        /// <summary>
        ///     Use lock
        /// </summary>
        private readonly object _writeFileLock = new object();

        #endregion

        #region ### Methods ###

        /// <summary>
        ///     Constructor - Initialize requirement
        /// </summary>
        /// <param name="logPath">Log path</param>
        /// <param name="logDirectoryName">Log directory name</param>
        /// <param name="logFileName">Log file name</param>
        /// <param name="encoding">Specify the encoding of log</param>
        public G9CSmallLogger(string logPath = null, string logDirectoryName = null, string logFileName = null,
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
        private void InitializeRequirement(string logPath = null, string logDirectoryName = null,
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
        private void CheckRequirement()
        {
            if (!IsInitialize)
                throw new TypeInitializationException(nameof(G9CSmallLogger),
                    new Exception(
                        $"Please initialize '{nameof(G9CSmallLogger)}'.\nUse 'Constructor' method for initialized this type."));
        }

        /// <inheritdoc />
        public void G9SmallLogInformation(string message,
            [CallerFilePath] string customCallerPath = null,
            [CallerMemberName] string customCallerName = null,
            [CallerLineNumber] int customLineNumber = 0)
        {
            CheckRequirement();
            WriteLogsToStream(
                $"{message}\nFile Name: {customCallerPath}\tMethod Base: {customCallerName}\tLine Number: {customLineNumber}",
                G9ESmallLogTypes.Information);
        }

        /// <inheritdoc />
        public void G9SmallLogWarning(string message,
            [CallerFilePath] string customCallerPath = null,
            [CallerMemberName] string customCallerName = null,
            [CallerLineNumber] int customLineNumber = 0)
        {
            CheckRequirement();
            WriteLogsToStream(
                $"{message}\nFile Name: {customCallerPath}\tMethod Base: {customCallerName}\tLine Number: {customLineNumber}",
                G9ESmallLogTypes.Warning);
        }

        /// <inheritdoc />
        public void G9SmallLogError(string errorMessage,
            [CallerFilePath] string customCallerPath = null,
            [CallerMemberName] string customCallerName = null,
            [CallerLineNumber] int customLineNumber = 0)
        {
            CheckRequirement();
            WriteLogsToStream(
                $"{errorMessage}\nFile Name: {customCallerPath}\tMethod Base: {customCallerName}\tLine Number: {customLineNumber}",
                G9ESmallLogTypes.Error);
        }

        /// <inheritdoc />
        public void G9SmallLogException(Exception ex, string additionalMessage = null,
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
        private void WriteLogsToStream(string logItemData, G9ESmallLogTypes logType, bool innerLog = false)
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

                lock (_writeFileLock)
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