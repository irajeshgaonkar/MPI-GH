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


public class DOH_MergeClientIdentityRequest : BaseRequest
{
    public DOH_MergeClientIdentityRequest(string trackingId) : base(ApiCallType.DOH_VEMerge, trackingId)
    {
    }
    /// <summary>
    /// 
    /// </summary>
    public string SourceSystem { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public string Agency { get; set; }
    public MergingSources Content { get; set; }
}

