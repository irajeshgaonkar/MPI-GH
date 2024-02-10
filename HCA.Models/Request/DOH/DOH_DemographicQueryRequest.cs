using HCA.Models.MuleSoft;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HCA.Models.Request.DOH
{
    /// <summary>
    /// 
    /// </summary>
    public class DOH_DemographicQueryRequest
    {
        /// <summary>
        /// 
        /// </summary>
        public int trackingid { get; set; }
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
        public Identity identity { get; set; }
    }
}