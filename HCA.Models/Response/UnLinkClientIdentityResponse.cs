using HCA.Models.Verato.Response;

namespace HCA.Models.Response;

public class UnLinkClientIdentityResponse : BaseResponse
{
    /// <summary>
    /// un linke identities response content <see cref="UnLinkIdentitiesResponseContent"/>
    /// </summary>
    public UnLinkIdentitiesResponseContent Content { get; set; }
}


public class DOH_UnLinkClientIdentityResponse : BaseResponse
{
    /// <summary>
    /// un linke identities response content <see cref="UnLinkIdentitiesResponseContent"/>
    /// </summary>
    public dynamic Content { get; set; }
}
