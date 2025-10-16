using HCA.Models.Verato.Request;

namespace HCA.Models.Verato.Request;

/// <summary>
/// Un merge identities request
/// </summary>
public class UnMergeIdentitiesRequest : VeratoRequest
{
    /// <summary>
    /// <see cref="UnMergeIdentitiesRequest"/>
    /// </summary>
    /// <param name="trackingId">Tracking id</param>
    public UnMergeIdentitiesRequest(string trackingId, UnMergingSources content) : base(trackingId)
    {
        Content = content;
    }

    /// <summary>
    /// Un merging sources
    /// </summary>
    public UnMergingSources Content { get; set; }
}
