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

public class DOH_UnLinkClientIdentityRequest : BaseRequest
{
    public DOH_UnLinkClientIdentityRequest(string trackingId) : base(ApiCallType.DOH_VEUnLink, trackingId)
    {
    }

    public UnLinkingSources Content { get; set; }
}

