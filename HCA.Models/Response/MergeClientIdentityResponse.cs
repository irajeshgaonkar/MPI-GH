using HCA.Models.MuleSoft.Response;

namespace HCA.Models.Response;

public class MergeClientIdentityResponse : BaseResponse
{
    /// <summary>
    /// Merge identities response content <see cref="MergeIdentitiesResponseContent"/>
    /// </summary>
    public MergeIdentitiesResponseContent Content { get; set; }
}
