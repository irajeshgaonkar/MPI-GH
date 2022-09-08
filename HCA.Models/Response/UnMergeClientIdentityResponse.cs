using HCA.Models.MuleSoft.Response;

namespace HCA.Models.Response;

public class UnMergeClientIdentityResponse : BaseResponse
{
    /// <summary>
    /// Un merged identities response content <see cref="UnMergeIdentitiesResponseContent"/>
    /// </summary>
    public UnMergeIdentitiesResponseContent Content { get; set; }
}
