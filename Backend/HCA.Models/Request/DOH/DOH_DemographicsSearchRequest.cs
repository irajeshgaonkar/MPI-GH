namespace HCA.Models.Request.DOH
{
    public class DOH_DemographicsSearchRequest
    {
        /// <summary>
        /// 
        /// </summary>
        public string? Trackingid { get; set; } = "";
        /// <summary>
        /// 
        /// </summary>
        public string? SourceSystem { get; set; }
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
        public ContentSearch content { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public class ContentSearch
    {
        /// <summary>
        /// Defaults to 'DEFAULT'
        /// </summary>
        public string[] responseIdentityFormatNames { get; set; } = ["DEFAULT"];
        /// <summary>
        /// 
        /// </summary>
        public double matchScoreThreshold { get; set; } = 0.0;
        /// <summary>
        /// 
        /// </summary>
        public int maxSearchResults { get; set; } = 10;

        /// <summary>
        /// 
        /// </summary>
        public dynamic identity { get; set; }
    }

}
