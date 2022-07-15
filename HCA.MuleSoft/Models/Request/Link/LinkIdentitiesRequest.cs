using HCA.Models.MuleSoft;

namespace HCA.MuleSoft.Models.Request.Link;

public class LinkIdentitiesRequest : BaseRequest<LinkingSources>
{
    public LinkIdentitiesRequest(string trackingId, LinkingSources linkingSources) : base(trackingId, linkingSources)
    {
    }
}

