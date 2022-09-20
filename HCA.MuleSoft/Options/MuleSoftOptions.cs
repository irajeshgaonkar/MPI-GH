using System.Net;

namespace HCA.MuleSoft;

public class MuleSoftOptions
{
    public string BaseUrl { get; set; }

    public string ClientId { get; set; }

    public string ClientSecret { get; set; }

    public MuleSoftRetryOptions RetryOptions { get; set; }
}

public class MuleSoftRetryOptions
{
    public int MaxDelayInSeconds { get; set; }

    public int MaxRetries { get; set; }

    public HttpStatusCode[] ReTriableStatusCode { get; set; }
}
