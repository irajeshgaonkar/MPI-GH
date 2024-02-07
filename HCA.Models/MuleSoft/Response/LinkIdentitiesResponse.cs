
namespace HCA.Models.MuleSoft.Response;

/// <summary>
/// Link Identities response
/// </summary>
public class LinkIdentitiesResponse : MuleSoftResponse
{
    /// <summary>
    /// Link Identities response content <see cref="LinkIdentitiesResponseContent"/>
    /// </summary>
    public LinkIdentitiesResponseContent Content { get; set; }
}

public class DOH_LinkIdentitiesResponse : MuleSoftResponse
{
    /// <summary>
    /// Link Identities response content <see cref="LinkIdentitiesResponseContent"/>
    /// </summary>
    public dynamic Content { get; set; }
}