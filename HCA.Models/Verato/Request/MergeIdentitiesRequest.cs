
using HCA.Models.Verato.Request;

namespace HCA.Models.Verato.Request;

/// <summary>
/// Merge Identities Request
/// </summary>
public class MergeIdentitiesRequest : VeratoRequest
{
    /// <summary>
    /// <see cref="MergeIdentitiesRequest"/>
    /// </summary>
    /// <param name="trackingId">Tracking id for the request</param>
    public MergeIdentitiesRequest(string trackingId, MergingSources content) : base(trackingId)
    {
        Content = content;
    }

    /// <summary>
    /// Merging Sources
    /// </summary>
    public MergingSources Content { get; set; }
}
