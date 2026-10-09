using Newtonsoft.Json;

namespace HCA.Models.Response
{
    /// <summary>
    /// Entra token response
    /// </summary>
    public class EntraToken
    {
        /// <summary>
        /// Access token in entra response
        /// </summary>
        [JsonProperty("access_token")]
        public string AccessToken { get; set; }

        /// <summary>
        /// Expiry of the access token
        /// </summary>
        [JsonProperty("expires_in")]
        public int ExpiresIn { get; set; }
    }
}
