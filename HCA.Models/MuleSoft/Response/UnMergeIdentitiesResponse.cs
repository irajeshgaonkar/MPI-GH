namespace HCA.Models.MuleSoft.Response;

/// <summary>
/// Un merge identities response
/// </summary>
public class UnMergeIdentitiesResponse : MuleSoftResponse
{
    /// <summary>
    /// Un merged identities response content <see cref="UnMergeIdentitiesResponseContent"/>
    /// </summary>
    public UnMergeIdentitiesResponseContent Content { get; set; }
}



