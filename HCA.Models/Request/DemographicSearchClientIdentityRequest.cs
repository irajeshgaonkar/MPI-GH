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
    /// <summary>
    /// 
    /// </summary>
    public string SourceSystem { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public string Agency { get; set; }
    public ContentSearch Content { get; set; }
}


public class DOH_DemographicQueryClientIdentityRequest : BaseRequest
{
    public DOH_DemographicQueryClientIdentityRequest(string trackingId) : base(ApiCallType.DOH_VEDemographicQuery, trackingId)
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
    public Content Content { get; set; }
}

/// <summary>
/// 
/// </summary>
public class DOH_EnrichDemographicQueryClientIdentityRequest : BaseRequest
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="trackingId"></param>
    public DOH_EnrichDemographicQueryClientIdentityRequest(string trackingId) : base(ApiCallType.DOH_VEEnrichDemographicQuery, trackingId)
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
    public ContentEnrich Content { get; set; }
}