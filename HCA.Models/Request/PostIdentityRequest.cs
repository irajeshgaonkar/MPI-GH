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
    public DOH_DeleteClientIdentityRequest(string trackingId) : base(ApiCallType.DOH_VEDelete, trackingId)
    {
    }

    public Source Content { get; set; }
}

public class DOH_PostClientIdentityRequest : BaseRequest
{
    public DOH_PostClientIdentityRequest(string trackingId) : base(ApiCallType.DOH_VEPost, trackingId)
    {
    }

    public PostIdentityRequestContent Content { get; set; }
}