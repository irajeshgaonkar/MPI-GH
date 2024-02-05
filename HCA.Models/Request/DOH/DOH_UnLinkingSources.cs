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
        /// <see cref="UnLinkingSources"/>
        /// </summary>
        /// <param name="_content">Content has a Unmerging Sources class</param>
        /// <param name="trackingID">trackingID</param>
        public DOH_UnLinkingSources(Content _content, int trackingID)
        {
            content = _content;
            Trackingid = trackingID;
        }

        /// <summary>
        /// trackingID
        /// </summary>
        public int Trackingid { get; set; }

        /// <summary>
        /// Content
        /// </summary>
        public Content content { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public class Content
        {
            /// <summary>
            /// 
            /// </summary>
            public UnLinkingSources unLinkingSources { get; set; }

        }
    }
}
