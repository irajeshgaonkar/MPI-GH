using HCA.Models.Enums;
using HCA.Models.MuleSoft;

namespace HCA.Models.Request;

public class DemographicSearchClientIdentityRequest : BaseRequest
{
    public DemographicSearchClientIdentityRequest(string trackingId) : base(ApiCallType.VEDemographicSearch, trackingId)
    {
    }

    public Identity Content { get; set; }
}