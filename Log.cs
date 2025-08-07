using Serilog;
using Serilog.Extensions.Logging;
using System.Diagnostics;
using System.IO;

namespace GammaTuner
{
    public class Log
    {

        public static void Info(string message)
        {
            LogInternal.Log(message, LogLevel.Info);
        }
        public static void Warn(string message)
        {
            LogInternal.Log(message, LogLevel.Warning);
        }   
        public static void Error(string message)
        {
            LogInternal.Log(message, LogLevel.Error);
        }

        private enum LogLevel
        {
            Info,
            Warning,
            Error
        }

        class LogInternal
        {
            private static Logger logger
            {
                get
                {
                    if (_logger == null)
                    {
                        _logger = new Logger();
                    }
                    return _logger;
                }
            }
            private static Logger? _logger;

            public static void Log(string message, LogLevel logLevel, bool debugWriteLine = true)
            {
                if (debugWriteLine)
                {
                    Debug.WriteLine(message);
                }
                switch (logLevel)
                {
                    case LogLevel.Info:
                        logger.LogInformation(message);
                        break;
                    case LogLevel.Warning:
                        logger.LogWarning(message);
                        break;
                    case LogLevel.Error:
                        logger.LogError(message);
                        break;
                }
            }
        }

        class Logger
        {
            StreamWriter logWriter;
            public Logger()
            {
                logWriter = new StreamWriter("GammaTuner.log", true)
                {
                    AutoFlush = true
                };
                logWriter.WriteLine($"[{DateTime.Now}] GammaTuner Log Started");
            }

            public void LogInformation(string message)
            {
                logWriter.WriteLine($"[{DateTime.Now}] INFO: {message}");
            }
            public void LogWarning(string message)
            {
                logWriter.WriteLine($"[{DateTime.Now}] WARNING: {message}");
            }
            public void LogError(string message)
            {
                logWriter.WriteLine($"[{DateTime.Now}] ERROR: {message}");
            }
        }
    }
}
