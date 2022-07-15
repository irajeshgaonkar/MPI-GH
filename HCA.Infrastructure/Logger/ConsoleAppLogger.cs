using Amazon.Lambda.Core;

namespace HCA.Infrastructure.Logger;

public class AppLogger : ILogger
{
    private readonly ILambdaLogger _logger;

    public AppLogger(ILambdaLogger logger)
    {
        _logger = logger;
    }

    public void LogInformation(string message) =>
        _logger.LogInformation(message);

    public void LogError(Exception ex) =>
        _logger.LogError(ex.StackTrace);

    public void LogCritical(string message) =>
        _logger.LogCritical(message);

    public void LogDebug(string message) =>
        _logger.LogDebug(message);

    public void LogTrace(string message) =>
        _logger.LogTrace(message);

    public void LogWarning(string message) =>
        _logger.LogWarning(message);
}

