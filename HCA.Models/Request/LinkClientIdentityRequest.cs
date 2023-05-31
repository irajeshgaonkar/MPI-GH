using HCA.Models.Enums;
using HCA.Models.MuleSoft;

namespace HCA.Models.Request;

public class LinkClientIdentityRequest : BaseRequest
{
    public LinkClientIdentityRequest(string trackingId) : base(ApiCallType.VELink, trackingId)
    {
    }

    public LinkingSources Content { get; set; }
}
