using System;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace SolidCqrsFramework
{
    public static class LoggerExtensions
    {
        private static readonly JsonSerializerOptions SerializerOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };

        public static void LogInformationWithObject(this ILogger logger, string message, object data)
        {
            LogWithObject(logger, LogLevel.Information, message, data);
        }

        public static void LogErrorWithObject(this ILogger logger, string message, object data)
        {
            LogWithObject(logger, LogLevel.Error, message, data);
        }

        public static void LogErrorWithObject(this ILogger logger, Exception exception, string message, object data)
        {
            LogWithObject(logger, LogLevel.Error, message, data, exception);
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
                data = GetSafeSerializableData(data)
            };

            string jsonLogEntry = JsonSerializer.Serialize(logEntry, SerializerOptions);

            if (exception == null)
            {
                logger.Log(logLevel, jsonLogEntry);
            }
            else
            {
                logger.Log(logLevel, exception, jsonLogEntry);
            }
        }

        private static object GetSafeSerializableData(object data)
        {
            try
            {
                // Try serializing the original data
                JsonSerializer.Serialize(data, SerializerOptions);
                return data; // If successful, return the original object
            }
            catch (Exception)
            {
                // If serialization fails, return a safe string representation
                return data?.ToString() ?? "Unserializable data";
            }
        }

    }
}
