using System;
using HCA.Models.Enums;
using HCA.Models.MuleSoft.Response;

namespace HCA.Models.Request;

public abstract class BaseRequest
{
    public BaseRequest(ApiCallType apiCallType, string trackingId)
    {
        ApiCallType = apiCallType;
        TrackingId = trackingId;
    }

    public string TrackingId { get; set; }

    public ApiCallType ApiCallType { get; set; }
}

public abstract class BaseResponse : MuleSoftResponse
{
    public BaseResponse() { }
}

public class PostClientIdentityResponse : BaseResponse
{
    /// <summary>
    /// Post Identity response content <see cref="PostIdentityResponseContent"/>
    /// </summary>
    public PostIdentityResponseContent Content { get; set; }
}

public class DemographicSearchClientIdentityResponse : BaseResponse
{
    /// <summary>
    /// collection of search results <see cref="PostIdentityResponseContent"/>
    /// </summary>
    public List<PostIdentityResponseContent> Content { get; set; }

    public IList<ClientIdentityModel> Result { get; set; }
}

public class LinkClientIdentityResponse : BaseResponse
{
    /// <summary>
    /// Link Identities response content <see cref="LinkIdentitiesResponseContent"/>
    /// </summary>
    public LinkIdentitiesResponseContent Content { get; set; }
}

public class UnLinkClientIdentityResponse : BaseResponse
{
    /// <summary>
    /// un linke identities response content <see cref="UnLinkIdentitiesResponseContent"/>
    /// </summary>
    public UnLinkIdentitiesResponseContent Content { get; set; }
}

public class MergeClientIdentityResponse : BaseResponse
{
    /// <summary>
    /// Merge identities response content <see cref="MergeIdentitiesResponseContent"/>
    /// </summary>
    public MergeIdentitiesResponseContent Content { get; set; }
}


public class UnMergeClientIdentityResponse : BaseResponse
{
    /// <summary>
    /// Un merged identities response content <see cref="UnMergeIdentitiesResponseContent"/>
    /// </summary>
    public UnMergeIdentitiesResponseContent Content { get; set; }
}
