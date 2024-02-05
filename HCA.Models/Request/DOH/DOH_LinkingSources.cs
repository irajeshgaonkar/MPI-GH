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
    public class DOH_LinkingSources
    {
        /// <summary>
        /// <see cref="LinkingSources"/>
        /// </summary>
        /// <param name="_content">Contect has an enclosed LinkingSources</param>
        /// <param name="trackingID">Tracking ID</param>
        public DOH_LinkingSources(Content _content, int trackingID)
        {
            content = _content;
            TrackingID = trackingID;
        }

        /// <summary>
        /// Content
        /// </summary>
        public Content content { get; set; }


        /// <summary>
        /// Source to be linke
        /// </summary>
        public int TrackingID { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public class Content
        {
            /// <summary>
            /// 
            /// </summary>
            public LinkingSources LinkingSources { get; set; }

        }

    }
}