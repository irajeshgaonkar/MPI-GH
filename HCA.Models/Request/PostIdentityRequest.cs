using HCA.Models.Enums;
namespace HCA.Models.Request;

public class PostClientIdentityRequest : BaseRequest
{
    public PostClientIdentityRequest(string trackingId) : base(ApiCallType.VEPost, trackingId)
    {
    }

    public IList<ClientIdentityRequest> Content { get; set; }
}

public class DemographicSearchClientIdentityRequest : BaseRequest
{
    public DemographicSearchClientIdentityRequest(string trackingId) : base(ApiCallType.VEDemographicSearch, trackingId)
    {
    }

    public IdentityFilter Content { get; set; }
}