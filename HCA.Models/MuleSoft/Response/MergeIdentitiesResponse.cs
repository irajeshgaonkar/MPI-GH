namespace HCA.Models.MuleSoft.Response;

/// <summary>
/// Merge identities reponse
/// </summary>
public class MergeIdentitiesResponse : MuleSoftResponse
{
    /// <summary>
    /// Merge identities response content <see cref="MergeIdentitiesResponseContent"/>
    /// </summary>
    public MergeIdentitiesResponseContent Content { get; set; }
}

