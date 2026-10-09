namespace HCA.Models.Verato.Response;

/// <summary>
/// Merge identities reponse
/// </summary>
public class MergeIdentitiesResponse : VeratoResponse
{
    /// <summary>
    /// Merge identities response content <see cref="MergeIdentitiesResponseContent"/>
    /// </summary>
    public MergeIdentitiesResponseContent Content { get; set; }
}



public class DOH_MergeIdentitiesResponse : VeratoResponse
{
    /// <summary>
    /// Merge identities response content <see cref="MergeIdentitiesResponseContent"/>
    /// </summary>
    public dynamic Content { get; set; }
}
