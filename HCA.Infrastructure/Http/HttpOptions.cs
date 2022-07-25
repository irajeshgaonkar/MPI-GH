namespace HCA.Infrastructure.Http;

/// <summary>
/// Http options
/// </summary>
public class HttpOptions
{
    /// <summary>
    /// Base Http Option
    /// </summary>
    public string BaseUrl { get; set; }

    /// <summary>
    /// Common headers for the requests
    /// </summary>
    public Dictionary<string, string> CommonHeaders { get; set; }
}

