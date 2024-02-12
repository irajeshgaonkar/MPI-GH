using HCA.Models.MuleSoft;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        //[JsonPropertyName("content")]
        public UnMergingSources content { get; set; }
    }
}
