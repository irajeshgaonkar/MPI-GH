namespace HCA.Models.MuleSoft.Request;

/// <summary>
/// Link Identities Request
/// </summary>
public class LinkIdentitiesRequest : MuleSoftRequest
{
    /// <summary>
    /// <see cref="LinkIdentitiesRequest"/>
    /// </summary>
    /// <param name="trackingId">Tracking id for the request</param>
    public LinkIdentitiesRequest(string trackingId, LinkingSources content) : base(trackingId)
    {
        Content = content;
    }

    /// <summary>
    /// Linking Sources
    /// </summary>
    public LinkingSources Content { get; set; }
}

