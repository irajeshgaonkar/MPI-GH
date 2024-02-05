using HCA.Models.MuleSoft;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HCA.Models.Request.DOH
{
    public class DOH_DemographicQueryRequest
    {
        public int  trackingid { get; set; }

        public Content content { get; set; }

    }

    /// <summary>
    /// 
    /// </summary>
    public class Content
    {
        public string[] responseIdentityFormatNames { get; set; }
        public Identity identity { get; set; }
    }
    
  
}