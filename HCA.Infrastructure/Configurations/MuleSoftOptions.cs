using System.Net;

namespace HCA.Infrastructure.Configurations
{
    public class MuleSoftOptions
    {
        public string BaseUrl { get; set; }
        public AdOptions AdOptions { get; set; }
        public RetryOptions RetryOptions { get; set; }
    }

    public class AdOptions
    {
        public string Tenant { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string GrantType { get; set; }
        public string Scope { get; set; }
    }

    public class RetryOptions
    {
        public int MaxRetries { get; set; }
        public HttpStatusCode[] ReTriableStatusCode { get; set; }
        public int MaxDelayInSeconds { get; set; }
    }
}
