using HCA.Models.MuleSoft;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using ThirdParty.Json.LitJson;

namespace HCA.Models.Request.DOH
{
    //public DOH_LinkingSources(Content _content, int trackingID)
    //{
    //    content = _content;
    //    TrackingID = trackingID;
    //}

    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
   
    /// <summary>
    /// 
    /// </summary>
    public class DOH_LinkingSources
    {
        /// <summary>
        /// 
        /// </summary>
        public string trackingId { get; set; }

        /// <summary>
        /// 
        /// </summary>
        //[JsonPropertyName("content")]
        public LinkingSources  content { get; set; }
    }
    }