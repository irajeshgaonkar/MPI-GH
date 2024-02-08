using HCA.Models.MuleSoft;
using HCA.Models.Enums;
using HCA.Models.Request.DOH;

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


    /// <summary>
    /// 
    /// </summary>
    public class DOH_DeleteSourceIdentityRequest : BaseRequest
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="trackingId"></param>
        /// <param name="content"></param>
        public DOH_DeleteSourceIdentityRequest(string trackingId) : base(ApiCallType.DOH_VEDelete, trackingId)
        {
        }
        /// <summary>
        /// 
        /// </summary>
        public ContentD Content { get; set; }
    }

}
