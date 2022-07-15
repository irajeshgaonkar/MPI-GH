using System;
using HCA.Models.MuleSoft;

namespace HCA.MuleSoft.Models.Request.Merge;  

public class MergeIdentitiesRequest : BaseRequest<MergingSources>
{
    public MergeIdentitiesRequest(string trackingId, MergingSources linkingSources) : base(trackingId, linkingSources)
    {
    }
}

public class UnMergeIdentitiesRequest : BaseRequest<UnMergingSources>
{
    public UnMergeIdentitiesRequest(string trackingId, UnMergingSources linkingSources) : base(trackingId, linkingSources)
    {
    }
}
