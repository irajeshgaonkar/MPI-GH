
namespace HCA.Models.Verato.Response;

/// <summary>
/// Link Identities response
/// </summary>
public class LinkIdentitiesResponse : VeratoResponse
{
    /// <summary>
    /// Link Identities response content <see cref="LinkIdentitiesResponseContent"/>
    /// </summary>
    public LinkIdentitiesResponseContent Content { get; set; }
}

public class DOH_LinkIdentitiesResponse : VeratoResponse
{
    /// <summary>
    /// Link Identities response content <see cref="LinkIdentitiesResponseContent"/>
    /// </summary>
    public dynamic Content { get; set; }
}