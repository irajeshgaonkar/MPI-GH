using HCA.Models.MuleSoft;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HCA.Models.Request.DOH
{
    /// <summary>
    /// 
    /// </summary>
    public class DOH_UnLinkingSources
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
        public UnLinkingSources content { get; set; }
    }
}
