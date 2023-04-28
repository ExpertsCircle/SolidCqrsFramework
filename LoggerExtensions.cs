using System;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace SolidCqrsFramework;

public static class LoggerExtensions
{
    public static void LogInformationWithObject(this ILogger logger, string message, object data)
    {
        var jsonData = JsonConvert.SerializeObject(data);
        logger.LogInformation("{message} - Data: {jsonData}", message, jsonData);
    }

    public static void LogErrorWithObject(this ILogger logger, string message, object data)
    {
        var jsonData = JsonConvert.SerializeObject(data);
        logger.LogError("{message} - Data: {jsonData}", message, jsonData);
    }

    public static void LogErrorWithObject(this ILogger logger, Exception e, string message, object data)
    {
        var jsonData = JsonConvert.SerializeObject(data);
        logger.LogError(e, "{message} - Data: {jsonData}", message, jsonData);
    }

    public static void LogTraceWithObject(this ILogger logger, string message, object data)
    {
        var jsonData = JsonConvert.SerializeObject(data);
        logger.LogTrace("{message} - Data: {jsonData}", message, jsonData);
    }
}
