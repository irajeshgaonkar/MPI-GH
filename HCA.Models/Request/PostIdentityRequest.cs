using HCA.Models.Enums;
using HCA.Models.MuleSoft;

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