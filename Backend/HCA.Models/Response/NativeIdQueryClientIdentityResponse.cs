namespace HCA.Models.Response
{
    /// <summary>
    /// Represents the response from a native ID query for client identity.
    /// </summary>
    public class NativeIdQueryClientIdentityResponse : BaseResponse
    {
        /// <summary>
        /// The content of the response
        /// </summary>
        public dynamic Content { get; set; }

    }
}
