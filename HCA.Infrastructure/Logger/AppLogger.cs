using System.Runtime.CompilerServices;
using Amazon.Lambda.Core;

namespace HCA.Infrastructure.Logger;

public class AppLogger : IAppLogger
{
    private const string _logPrefix = "HCA - MPI => ";

    private readonly ILambdaLogger _logger;

    public AppLogger(ILambdaLogger logger)
    {
        _logger = logger;
    }

    public void LogInformation(string message, [CallerMemberName] string callerName = "") =>
        _logger.LogInformation($"{_logPrefix} {callerName}: {message}");

    public void LogError(Exception ex, [CallerMemberName] string callerName = "") =>
        _logger.LogError($"{_logPrefix} {callerName}: {ex.StackTrace}");

    public void LogCritical(string message, [CallerMemberName] string callerName = "") =>
        _logger.LogCritical($"{_logPrefix} {callerName}: {message}");

    public void LogDebug(string message, [CallerMemberName] string callerName = "") =>
        _logger.LogDebug($"{_logPrefix} {callerName}: {message}");

    public void LogTrace(string message, [CallerMemberName] string callerName = "") =>
        _logger.LogTrace($"{_logPrefix} {callerName}: {message}");

    public void LogWarning(string message, [CallerMemberName] string callerName = "") =>
        _logger.LogWarning($"{_logPrefix} {callerName}: {message}");
}

