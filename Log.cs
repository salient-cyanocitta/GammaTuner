using Microsoft.Extensions.Logging;
using System.Diagnostics;

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
            private static ILogger logger
            {
                get
                {
                    if (_logger == null)
                    {
                        using ILoggerFactory factory = LoggerFactory.Create(builder => builder.AddConsole());
                        _logger = factory.CreateLogger("GammaTuner");
                    }
                    return _logger;
                }
            }
            private static ILogger? _logger;

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
    }
}
