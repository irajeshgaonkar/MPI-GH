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

   
}
