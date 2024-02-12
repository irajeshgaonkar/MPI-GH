using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HCA.Models.Request.DOH
{
    public class DOH_DemographicsSearchRequest
    {
        /// <summary>
        /// 
        /// </summary>
        public string trackingid { get; set; } = "";
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
        /// 
        /// </summary>
        public string[] responseIdentityFormatNames { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public double matchScoreThreshold { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int maxSearchResults { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public dynamic identity { get; set; }
    }

}
