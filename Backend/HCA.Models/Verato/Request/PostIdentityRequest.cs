using HCA.Models.Request.DOH;
using HCA.Models.Verato.Request;

namespace HCA.Models.Verato.Request;

/// <summary>
/// Post Identity Request
/// </summary>
public class PostIdentityRequest : VeratoRequest
{
    /// <summary>
    /// Post identity request
    /// </summary>
    /// <param name="trackingId">Tracking id for the request</param>
    public PostIdentityRequest(string trackingId, PostIdentityRequestContent content) : base(trackingId)
    {
        Content = content;
    }

    /// <summary>
    /// Post Identity request content
    /// </summary>
    public PostIdentityRequestContent Content { get; set; }
}


/// <summary>
/// Demographic Search Request
/// </summary>
public class DemographicSearchRequest : VeratoRequest
{
    /// <summary>
    /// Demographic Search Request
    /// </summary>
    /// <param name="trackingId">Tracking id for the request</param>
    public DemographicSearchRequest(string trackingId, ContentSearch content) : base(trackingId)
    {
        Content = content;
    }

    /// <summary>
    /// Post Identity request content
    /// </summary>
    public ContentSearch Content { get; set; }
}
