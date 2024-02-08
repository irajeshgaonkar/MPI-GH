using HCA.Models.MuleSoft;
using HCA.Models.MuleSoft.Response;

namespace HCA.Models.Response
{
   /// <summary>
   /// 
   /// </summary>
    public class DeleteSourceIdentityResponse : BaseResponse
    {
        /// <summary>
        /// Delete Identities response content <see cref="Source"/>
        /// </summary>
        public Source Content { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public class DOH_DeleteSourceIdentityResponse : BaseResponse
    {
        /// <summary>
        /// Delete Identities response content 
        /// </summary>
        public dynamic Content { get; set; }
    }
}
