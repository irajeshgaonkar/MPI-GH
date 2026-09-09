using System.Net;

namespace HCA.Infrastructure.Configurations
{
    public class RetryOptions
    {
        public int MaxRetries { get; set; }
        public HttpStatusCode[] ReTriableStatusCode { get; set; }
        public int MaxDelayInSeconds { get; set; }
    }
}
