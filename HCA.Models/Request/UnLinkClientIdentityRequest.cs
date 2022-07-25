using HCA.Models.Enums;
using HCA.Models.MuleSoft;

namespace HCA.Models.Request;

public class UnLinkClientIdentityRequest : BaseRequest
{
    public UnLinkClientIdentityRequest(string trackingId) : base(ApiCallType.VEUnLink, trackingId)
    {
    }

    public UnLinkingSources Content { get; set; }
}

