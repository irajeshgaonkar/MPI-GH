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
    public class DOH_MergingSources
    {
        /// <summary>
        /// <see cref="UnMergingSources"/>
        /// </summary>
        /// <param name="content">Unmerging Sources</param>
        /// <param name="trackingID">trackingID</param>
        public DOH_MergingSources(Content _content, int trackingID)
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
            public MergingSources MergingSources { get; set; }

        }
    }
}
