
namespace HCA.Models.MuleSoft.Request;

/// <summary>
/// Merge Identities Request
/// </summary>
public class MergeIdentitiesRequest : MuleSoftRequest
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
