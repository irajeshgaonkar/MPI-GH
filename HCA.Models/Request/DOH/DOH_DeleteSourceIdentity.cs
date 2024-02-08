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
    public class DOH_DeleteSourceIdentity
    {
        /// <summary>
        /// 
        /// </summary>
        public string trackingId { get; set; }

        /// <summary>
        /// 
        /// </summary>
        //[JsonPropertyName("content")]
        public ContentD content { get; set; }
    }
    /// <summary>
    /// 
    /// </summary>
    public class ContentD
    {
        /// <summary>
        /// 
        /// </summary>
        public Source source { get; set; }
    }
}
