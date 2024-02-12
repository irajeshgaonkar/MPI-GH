using HCA.Models.Enums;
using HCA.Models.MuleSoft;
using HCA.Models.Request.DOH;

namespace HCA.Models.Request;

public class DemographicSearchClientIdentityRequest : BaseRequest
{
    public DemographicSearchClientIdentityRequest(string trackingId) : base(ApiCallType.VEDemographicSearch, trackingId)
    {
    }

    public Identity Content { get; set; }
}


public class DemographicQueryClientIdentityRequest : BaseRequest
{
    public DemographicQueryClientIdentityRequest(string trackingId) : base(ApiCallType.VEDemographicQuery, trackingId)
    {
    }

    public Identity Content { get; set; }
}

public class DOH_DemographicSearchClientIdentityRequest : BaseRequest
{
    public DOH_DemographicSearchClientIdentityRequest(string trackingId) : base(ApiCallType.DOH_VEDemographicSearch, trackingId)
    {
    }

    public ContentSearch Content { get; set; }
}


public class DOH_DemographicQueryClientIdentityRequest : BaseRequest
{
    public DOH_DemographicQueryClientIdentityRequest(string trackingId) : base(ApiCallType.DOH_VEDemographicQuery, trackingId)
    {
    }

    public Content Content { get; set; }
}