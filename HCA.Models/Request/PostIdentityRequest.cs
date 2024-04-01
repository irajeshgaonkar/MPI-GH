using HCA.Models.Enums;
using HCA.Models.MuleSoft;
using HCA.Models.MuleSoft.Request;

namespace HCA.Models.Request;

public class PostClientIdentityRequest : BaseRequest
{
    public PostClientIdentityRequest(string trackingId) : base(ApiCallType.VEPost, trackingId)
    {
    }

    public IList<ClientIdentityRequest> Content { get; set; }
}

public class DeleteClientIdentityRequest : BaseRequest
{
    public DeleteClientIdentityRequest(string trackingId) : base(ApiCallType.VEDelete, trackingId)
    {
    }

    public Source Content { get; set; }
}

public class DOH_DeleteClientIdentityRequest : BaseRequest
{
    public DOH_DeleteClientIdentityRequest(string trackingId = "") : base(ApiCallType.DOH_VEDelete, trackingId)
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
    /// <summary>
    /// 
    /// </summary>
    public string? IpAddress { get; set; }
    public DeleteIdentyRequestContent Content { get; set; }
}

public class DOH_PostClientIdentityRequest : BaseRequest
{
    public DOH_PostClientIdentityRequest(string trackingId = "") : base(ApiCallType.DOH_VEPost, trackingId)
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
    /// <summary>
    /// 
    /// </summary>
    public string? IpAddress { get; set; }
    public DOH_PostIdentityRequestContent Content { get; set; }
}