using System;
using HCA.Models.MuleSoft;

namespace HCA.MuleSoft.Models.Response.Merge;


public class MergeIdentitiesResponse : BaseResponse<MergeIdentitiesResponseContent>
{
    public MergeIdentitiesResponse(string trackingId, Guid auditId, bool retryableError, string message, List<string> errors, MergeIdentitiesResponseContent content)
            : base(trackingId, auditId, retryableError, message, errors, content)
    {
    }
}

public class MergeIdentitiesResponseContent
{
	public string LinkId { get; set; }

	public Source Source { get; set; }
}

public class UnMergeIdentitiesResponse : BaseResponse<UnMergeIdentitiesResponseContent>
{
    public UnMergeIdentitiesResponse(string trackingId, Guid auditId, bool retryableError, string message, List<string> errors, UnMergeIdentitiesResponseContent content)
            : base(trackingId, auditId, retryableError, message, errors, content)
    {
    }
}

public class UnMergeIdentitiesResponseContent
{
	public string UnmergedId { get; set; }

	public string UnmergedFromId { get; set; }

    public Source UnmergedFromSource { get; set; }

    public Source UnmergedSource { get; set; }
}



