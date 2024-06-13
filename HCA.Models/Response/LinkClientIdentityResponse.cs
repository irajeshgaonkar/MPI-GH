using HCA.Models.MuleSoft.Response;

namespace HCA.Models.Response;

public class LinkClientIdentityResponse : BaseResponse
{
    /// <summary>
    /// Link Identities response content <see cref="LinkIdentitiesResponseContent"/>
    /// </summary>
    public LinkIdentitiesResponseContent Content { get; set; }
}


public class DOH_LinkClientIdentityResponse : BaseResponse
{
    /// <summary>
    /// Link Identities response content <see cref="LinkIdentitiesResponseContent"/>
    /// </summary>
    public dynamic Content { get; set; }
}
