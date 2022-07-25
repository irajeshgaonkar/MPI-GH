using HCA.Models.Enums;
using HCA.Models.MuleSoft;

namespace HCA.Models.Request;

public class MergeClientIdentityRequest : BaseRequest
{
    public MergeClientIdentityRequest(string trackingId) : base(ApiCallType.VEMerge, trackingId)
    {
    }

    public MergingSources Content { get; set; }
}

