using HCA.Models.Verato.Request;

namespace HCA.Models.Verato.Request
{
    /// <summary>
    /// Represents a request to query native IDs
    /// </summary>
    /// <param name="trackingId"></param>
    /// <param name="content"></param>
    public class NativeIdQueryRequest( string trackingId, NativeIdRequestContent content ) : VeratoRequest( trackingId )
    {

        /// <summary>
        /// Content of the native ID request.
        /// </summary>
        public NativeIdRequestContent Content { get; set; } = content;
    }


    /// <summary>
    /// Represents the content of a native ID request.
    /// </summary>
    public class NativeIdRequestContent
    {
        /// <summary>
        /// ResponseIdentityFormatNames
        /// </summary>
        public string[] ResponseIdentityFormatNames { get; set; } = ["DEFAULT"];

        /// <summary>
        /// List of native IDs to query.
        /// </summary>
        public NativeIdRequestSource Source { get; set; } = new NativeIdRequestSource();

    }

    /// <summary>
    /// Represents the source of the native ID request.
    /// </summary>
    public class NativeIdRequestSource
    {
        /// <summary>
        /// Name of the source
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// NativeId for the source
        /// </summary>
        public string Id { get; set; }
    }
}