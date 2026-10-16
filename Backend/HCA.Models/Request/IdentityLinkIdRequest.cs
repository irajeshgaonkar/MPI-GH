using System.Text.Json.Serialization;

namespace HCA.Models.Request
{
    public class IdentityLinkIdRequest
    {
        public string? TrackingId { get; set; }

        public string? SourceSystem { get; set; }

        public string? Agency { get; set; }

        public string? IpAddress { get; set; }

        [JsonPropertyName("content")]
        public IdentityLinkIdRequestContent? Content { get; set; }
    }

    public class IdentityLinkIdRequestContent
    {
        [JsonPropertyName("responseIdentityFormatNames")]
        public string[]? ResponseIdentityFormatNames { get; set; }

        [JsonPropertyName("identity")]
        public dynamic? Identity { get; set; }
    }
}
