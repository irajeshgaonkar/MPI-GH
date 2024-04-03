using HCA.Models.MuleSoft;

namespace HCA.Models.Request.DOH
{
    /// <summary>
    /// 
    /// </summary>
    public class DOH_UnMergingSources
    {


        /// <summary>
        /// 
        /// </summary>
        public string trackingId { get; set; } = "";
        /// <summary>
        /// 
        /// </summary>
        public string SourceSystem { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Agency { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string? IpAddress { get; set; }

        /// <summary>
        /// 
        /// </summary>
        //[JsonPropertyName("content")]
        public UnMergingSources content { get; set; }
    }
}
