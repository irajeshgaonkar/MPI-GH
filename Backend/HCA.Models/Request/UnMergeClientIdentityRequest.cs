using HCA.Models.Enums;
using HCA.Models.Verato;

namespace HCA.Models.Request;

public class UnMergeClientIdentityRequest : BaseRequest
{
    public UnMergeClientIdentityRequest(string trackingId) : base(ApiCallType.VEUnMerge, trackingId)
    {
    }

    public UnMergingSources Content { get; set; }
}


public class DOH_UnMergeClientIdentityRequest : BaseRequest
{
    public DOH_UnMergeClientIdentityRequest(string trackingId) : base(ApiCallType.DOH_VEUnMerge, trackingId)
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
    public UnMergingSources Content { get; set; }
}

