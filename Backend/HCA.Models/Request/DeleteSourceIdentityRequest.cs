using HCA.Models.Verato;
using HCA.Models.Enums;

namespace HCA.Models.Request
{
    /// <summary>
    /// 
    /// </summary>
    public class DeleteSourceIdentityRequest: BaseRequest 
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="trackingId"></param>
        /// <param name="content"></param>
        public DeleteSourceIdentityRequest(string trackingId) : base(ApiCallType.VEDelete, trackingId)
        {
        }
        /// <summary>
        /// 
        /// </summary>
        public Source Content { get; set; }

    }



}
