using HCA.Models.MuleSoft.Response;

namespace HCA.Models.Response;

public class LinkClientIdentityResponse : BaseResponse
{
    /// <summary>
    /// Link Identities response content <see cref="LinkIdentitiesResponseContent"/>
    /// </summary>
    public LinkIdentitiesResponseContent Content { get; set; }
}
