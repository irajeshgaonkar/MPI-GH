using HCA.Models.Enums;
using HCA.Models.MuleSoft;

namespace HCA.Models.Request;

public class UnMergeClientIdentityRequest : BaseRequest
{
    public UnMergeClientIdentityRequest(string trackingId) : base(ApiCallType.VEUnMerge, trackingId)
    {
    }

    public UnMergingSources Content { get; set; }
}

