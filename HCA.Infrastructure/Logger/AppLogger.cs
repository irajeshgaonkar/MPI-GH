using Amazon.Lambda.Core;

namespace HCA.Infrastructure.Logger;

public class ConsoleAppAppLogger : ILogger
{

    public void LogInformation(string message) =>
        WriteLine(message, ConsoleColor.Blue);

    public void LogError(Exception ex) =>
        WriteLine(ex.StackTrace ?? "", ConsoleColor.Red);

    public void LogCritical(string message) =>
        WriteLine(message, ConsoleColor.DarkYellow);

    public void LogDebug(string message) =>
        WriteLine(message, ConsoleColor.Green);

    public void LogTrace(string message) =>
        WriteLine(message, ConsoleColor.Cyan);

    public void LogWarning(string message) =>
        WriteLine(message, ConsoleColor.Yellow);

    private void WriteLine(string message, ConsoleColor color)
    {
        var previousColor = Console.ForegroundColor;
        Console.ForegroundColor = color;
        Console.WriteLine(message);
        Console.ForegroundColor = previousColor;
    }
}

