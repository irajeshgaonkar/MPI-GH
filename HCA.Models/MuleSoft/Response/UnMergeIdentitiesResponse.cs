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

public class DOH_UnMergeIdentitiesResponse : MuleSoftResponse
{
    /// <summary>
    /// Un merged identities response content <see cref="UnMergeIdentitiesResponseContent"/>
    /// </summary>
    public dynamic Content { get; set; }
}



