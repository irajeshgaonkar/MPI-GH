namespace HCA.Models.MuleSoft.Response;

/// <summary>
/// Un link identities response
/// </summary>
public class UnLinkIdentitiesResponse : MuleSoftResponse
{
    /// <summary>
    /// un linke identities response content <see cref="UnLinkIdentitiesResponseContent"/>
    /// </summary>
    public UnLinkIdentitiesResponseContent Content { get; set; }
}

public class DOH_UnLinkIdentitiesResponse : MuleSoftResponse
{
    /// <summary>
    /// un linke identities response content <see cref="UnLinkIdentitiesResponseContent"/>
    /// </summary>
    public dynamic Content { get; set; }
}

