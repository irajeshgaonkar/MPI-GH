namespace HCA.Models.MuleSoft.Request;

/// <summary>
/// Post Identity Request
/// </summary>
public class PostIdentityRequest : MuleSoftRequest
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

