namespace HCA.Infrastructure.Http;

/// <summary>
/// Calculates exponential delay for retrying, 
/// </summary>
public class ExponentialDelayCalculator : IDelayCaculator
{
    private int _maxDelayInSeconds = 2000;

    public int Calculate(int attemptNumber)
    {
        var delayInSeconds = ((1d / 2d) * (Math.Pow(2d, attemptNumber) - 1d));
        return _maxDelayInSeconds > delayInSeconds
            ? _maxDelayInSeconds
            : Convert.ToInt32(delayInSeconds);
    }
}

