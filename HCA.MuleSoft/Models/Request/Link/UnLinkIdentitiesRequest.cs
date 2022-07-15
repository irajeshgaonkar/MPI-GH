using HCA.Models.MuleSoft;

namespace HCA.MuleSoft.Models.Request.Link;

public class UnLinkIdentitiesRequest : BaseRequest<UnLinkingSources>
{
    public UnLinkIdentitiesRequest(string trackingId, UnLinkingSources linkingSources) : base(trackingId, linkingSources)
    {
    }
}

