using System.Net;

namespace HCA.MuleSoft;

public class MuleSoftOptions
{
    public string BaseUrl { get; set; }

    public MuleSoftRetryOptions RetryOptions { get; set; }

    public AdOptions AdOptions { get; set; }
}

public class AdOptions
{
    public string Tenant { get; set; }

    public string ClientId { get; set; }

    public string ClientSecret { get; set; }

    public string GrantType { get; set; }

    public string Scope { get; set; }
}

public class MuleSoftRetryOptions
{
    public int MaxDelayInSeconds { get; set; }

    public int MaxRetries { get; set; }

    public HttpStatusCode[] ReTriableStatusCode { get; set; }
}
