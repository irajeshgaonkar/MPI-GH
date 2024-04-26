using System.Runtime.CompilerServices;

namespace HCA.Infrastructure.Logger;

public class ConsoleAppAppLogger : IAppLogger
{

    public void LogInformation(string message, [CallerMemberName] string callerName = "")
    {
        WriteLine(message, ConsoleColor.Blue);
    }

    public void LogError(Exception ex, string? message = null, [CallerMemberName] string callerName = "") =>
        WriteLine(ex.ToString() + " " + message ?? "", ConsoleColor.Red);

    public void LogCritical(string message, [CallerMemberName] string callerName = "") =>
        WriteLine(message, ConsoleColor.DarkYellow);

    public void LogDebug(string message, [CallerMemberName] string callerName = "") =>
        WriteLine(message, ConsoleColor.Green);

    public void LogTrace(string message, [CallerMemberName] string callerName = "") =>
        WriteLine(message, ConsoleColor.Cyan);

    public void LogWarning(string message, [CallerMemberName] string callerName = "") =>
        WriteLine(message, ConsoleColor.Yellow);

    private void WriteLine(string message, ConsoleColor color)
    {
        var previousColor = Console.ForegroundColor;
        Console.ForegroundColor = color;
        Console.WriteLine(message);
        Console.ForegroundColor = previousColor;
    }
}

