namespace HCA.Infrastructure.Http;

/// <summary>
/// Delay Calculator for http requests retries
/// </summary>
public interface IDelayCaculator
{
    /// <summary>
    /// Calculates delay
    /// </summary>
    /// <param name="attemptNumber">current retry attempt number</param>
    /// <returns>delay for the next request</returns>
    int Calculate(int attemptNumber);
}

