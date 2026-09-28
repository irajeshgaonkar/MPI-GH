using System.Runtime.CompilerServices;

namespace HCA.Infrastructure.Logger;

public interface IAppLogger
{
    void LogInformation(string message, [CallerMemberName] string callerName = "");

    void LogError(Exception ex, string? message = null, [CallerMemberName] string callerName = "");

    void LogDebug(string message, [CallerMemberName] string callerName = "");

    void LogCritical(string message, [CallerMemberName] string callerName = "");

    void LogTrace(string message, [CallerMemberName] string callerName = "");

    void LogWarning(string message, [CallerMemberName] string callerName = "");
}

