using HCA.Models.MuleSoft.Response;

namespace HCA.Models.Response;

public class UnLinkClientIdentityResponse : BaseResponse
{
    /// <summary>
    /// un linke identities response content <see cref="UnLinkIdentitiesResponseContent"/>
    /// </summary>
    public UnLinkIdentitiesResponseContent Content { get; set; }
}
