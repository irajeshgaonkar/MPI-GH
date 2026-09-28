using HCA.Models.Enums;

namespace HCA.Models.Request
{
    /// <summary>
    /// Request to query client identity using native ID
    /// </summary>
    public class NativeIdQueryClientIdentityRequest: BaseRequest
    {
        /// <summary>
        /// Constructor for NativeIdQueryClientIdentityRequest
        /// </summary>
        /// <param name="trackingId"></param>
        public NativeIdQueryClientIdentityRequest(string trackingId) : base(ApiCallType.VENativeIdQuery, trackingId)
        {
        }

        /// <summary>
        /// Content of the request containing native ID query details
        /// </summary>
        public NativeIdQueryContent Content { get; set; }
    }

    /// <summary>
    /// Content for the NativeIdQueryClientIdentityRequest
    /// </summary>
    public class NativeIdQueryContent
    {
        /// <summary>
        /// ResponseIdentityFormatNames
        /// </summary>
        public string[] ResponseIdentityFormatNames { get; set; } = ["DEFAULT"];

        /// <summary>
        /// Source of the NativeIdQuery
        /// </summary>
        public NativeIdRequestSource Source { get; set; } = new NativeIdRequestSource();
    }

    /// <summary>
    /// Source of the native ID request
    /// </summary>
    public class NativeIdRequestSource
    {
        /// <summary>
        /// Source Name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// NativeId
        /// </summary>
        public string Id { get; set; }
    }
}
