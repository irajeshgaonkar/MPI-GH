namespace HCA.Infrastructure.Logger;

public interface ILogger
{
    void LogInformation(string message);

    void LogError(Exception ex);

    void LogDebug(string message);

    void LogCritical(string message);

    void LogTrace(string message);

    void LogWarning(string message);
}

