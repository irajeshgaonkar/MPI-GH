namespace HCA.Models.Request.DOH
{
    // TODO: make a base class that API queries inherit from for shared data/logic (trackingId, sourceSystem, etc)
    /// <summary>
    /// 
    /// </summary>
    public class DOH_DemographicQueryRequest
    {
        /// <summary>
        /// 
        /// </summary>
        public string trackingid { get; set; } = "";

        /// <summary>
        /// 
        /// </summary>
        public string? SourceSystem { get; set; } = "";
        /// <summary>
        /// 
        /// </summary>
        public string? Agency { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string? IpAddress { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public Content content { get; set; }

    }

    /// <summary>
    /// 
    /// </summary>
    public class Content
    {
        /// <summary>
        /// 
        /// </summary>
        public string[] responseIdentityFormatNames { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public dynamic identity { get; set; }
    }
}