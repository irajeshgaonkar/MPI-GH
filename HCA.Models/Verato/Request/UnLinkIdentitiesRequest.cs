using HCA.Models.Verato.Request;

namespace HCA.Models.Verato.Request;

/// <summary>
/// Un link identities request
/// </summary>
public class UnLinkIdentitiesRequest : VeratoRequest
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

