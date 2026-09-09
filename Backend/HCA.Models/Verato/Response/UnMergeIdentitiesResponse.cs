namespace HCA.Models.Verato.Response;

/// <summary>
/// Un merge identities response
/// </summary>
public class UnMergeIdentitiesResponse : VeratoResponse
{
    /// <summary>
    /// Un merged identities response content <see cref="UnMergeIdentitiesResponseContent"/>
    /// </summary>
    public UnMergeIdentitiesResponseContent Content { get; set; }
}

public class DOH_UnMergeIdentitiesResponse : VeratoResponse
{
    /// <summary>
    /// Un merged identities response content <see cref="UnMergeIdentitiesResponseContent"/>
    /// </summary>
    public dynamic Content { get; set; }
}



