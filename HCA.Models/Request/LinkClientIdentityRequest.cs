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


public class DOH_LinkClientIdentityRequest : BaseRequest
{
    public DOH_LinkClientIdentityRequest(string trackingId) : base(ApiCallType.DOH_VELink, trackingId)
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
    public LinkingSources Content { get; set; }
}
