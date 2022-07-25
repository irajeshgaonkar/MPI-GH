namespace HCA.Models.MuleSoft.Request;

/// <summary>
/// Un link identities request
/// </summary>
public class UnLinkIdentitiesRequest : MuleSoftRequest
{
    /// <summary>
    /// <see cref="UnLinkIdentitiesRequest"/>
    /// </summary>
    /// <param name="trackingId">Tracking id request</param>
    public UnLinkIdentitiesRequest(string trackingId, UnLinkingSources content) : base(trackingId)
    {
        Content = content;
    }

    /// <summary>
    /// Un linking sources
    /// </summary>
    public UnLinkingSources Content { get; set; }
}

