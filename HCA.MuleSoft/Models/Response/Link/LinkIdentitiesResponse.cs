using HCA.Models.MuleSoft;

namespace HCA.MuleSoft.Models.Response.Link;

public class LinkIdentitiesResponse : BaseResponse<LinkIdentitiesResponseContent>
{
    public LinkIdentitiesResponse(string trackingId, Guid auditId, bool retryableError, string message, List<string> errors, LinkIdentitiesResponseContent content)
            : base(trackingId, auditId, retryableError, message, errors, content)
    {
    }
}

public class LinkIdentitiesResponseContent
{
    public string LinkId { get; set; }

    public Source LinkToSource { get; set; }
}

public class UnLinkIdentitiesResponse : BaseResponse<UnLinkIdentitiesResponseContent>
{
    public UnLinkIdentitiesResponse(string trackingId, Guid auditId, bool retryableError, string message, List<string> errors, UnLinkIdentitiesResponseContent content)
            : base(trackingId, auditId, retryableError, message, errors, content)
    {
    }
}

public class UnLinkIdentitiesResponseContent
{
    public string UnlinkedId { get; set; }

    public Source UnlinkedSource { get; set; }

    public string UnlinkedFromId { get; set; }

    public Source UnlinkedFromSource { get; set; }
}

