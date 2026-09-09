namespace HCA.Models.Verato.Request;

/// <summary>
/// Un merge identities request
/// </summary>
/// <remarks>
/// <see cref="UnMergeIdentitiesRequest"/>
/// </remarks>
/// <param name="trackingId">Tracking id</param>
/// <param name="content"></param>
public class UnMergeIdentitiesRequest( string trackingId, UnMergingSources content ) : VeratoRequest(trackingId)
{

    /// <summary>
    /// Un merging sources
    /// </summary>
    public UnMergingSources Content { get; set; } = content;
}
