using System.Text.Json.Serialization;

namespace HCA.Models.Request.DOH
{
    /// <summary>
    /// IdentityExists request
    /// </summary>
    public class IdentityExistsRequest
    {
        /// <summary>
        /// Tracking ID for the request.
        /// </summary>
        public string? TrackingId { get; set; } = "";

        /// <summary>
        /// Source system that is making the request.
        /// </summary>
        public string? SourceSystem { get; set; } = "";
        /// <summary>
        /// Agency making the request
        /// </summary>
        public string? Agency { get; set; }

        /// <summary>
        /// IP address from which the request is being made
        /// </summary>
        public string? IpAddress { get; set; }

        /// <summary>
        /// content of the identity request
        /// </summary>
        [JsonPropertyName("content")]
        public IdentityExistsRequestContent Content { get; set; }
    }

    /// <summary>
    /// content of the identity exists request
    /// </summary>
    public class IdentityExistsRequestContent
    {
        /// <summary>
        /// search identity content
        /// </summary>
        [JsonPropertyName("identity")]
        public dynamic Identity { get; set; }
    }
}
