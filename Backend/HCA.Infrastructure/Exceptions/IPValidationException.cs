namespace HCA.Infrastructure.Exceptions
{
    /// <summary>
    /// IP Validation Exception
    /// </summary>
    /// <param name="message"></param>
    public class IPValidationException(string message) : Exception(message)
    {
    }
}
