namespace HCA.Models.Request.DOH
{
    // TODO: create base request class with shared properties

    /// <summary>
    /// DOH_EnrichDemographicQueryRequest Model
    /// </summary>
    public class DOH_EnrichDemographicQueryRequest
    {
        /// <summary>
        /// Tracking Id for the request
        /// </summary>
        public string TrackingId { get; set; } = "";

        /// <summary>
        /// Source System name in the context
        /// </summary>
        public string? SourceSystem { get; set; } = "";

        /// <summary>
        /// Agency in the context
        /// </summary>
        public string? Agency { get; set; }

        /// <summary>
        /// IpAddress of the caller
        /// </summary>
        public string? IpAddress { get; set; }

        /// <summary>
        /// Identity content to query Enrich data
        /// </summary>
        public ContentEnrich Content { get; set; }
    }

    /// <summary>
    /// Identity content to query Enrich data
    /// </summary>
    public class ContentEnrich
    {
        /// <summary>
        /// Response Identity Format options
        /// DEFAULT, GROUP_BY_SOURCE, ""
        /// </summary>
        public string[] ResponseIdentityFormatNames { get; set; }
        /// <summary>
        /// Identity in the context
        /// </summary>
        public dynamic Identity { get; set; }
    }

}
