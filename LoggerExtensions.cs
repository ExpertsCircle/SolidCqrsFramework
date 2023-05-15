using System;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace SolidCqrsFramework
{
    public static class LoggerExtensions
    {
        public static void LogInformationWithObject(this ILogger logger, string message, object data)
        {
            LogWithObject(logger, LogLevel.Information, message, data);
        }

        public static void LogErrorWithObject(this ILogger logger, string message, object data)
        {
            LogWithObject(logger, LogLevel.Error, message, data);
        }

        public static void LogErrorWithObject(this ILogger logger, Exception e, string message, object data)
        {
            LogWithObject(logger, LogLevel.Error, message, data, e);
        }

        public static void LogTraceWithObject(this ILogger logger, string message, object data)
        {
            LogWithObject(logger, LogLevel.Trace, message, data);
        }

        public static void LogWarningWithObject(this ILogger logger, string message, object data)
        {
            LogWithObject(logger, LogLevel.Warning, message, data);
        }

        private static void LogWithObject(ILogger logger, LogLevel logLevel, string message, object data, Exception exception = null)
        {
            var logEntry = new
            {
                message,
                data
            };

            var jsonLogEntry = JsonConvert.SerializeObject(logEntry);

            if (exception == null)
            {
                logger.Log(logLevel, jsonLogEntry);
            }
            else
            {
                logger.Log(logLevel, exception, jsonLogEntry);
            }
        }
    }
}
