using HCA.Models.Verato.Response;

namespace HCA.Models.Response;

public class MergeClientIdentityResponse : BaseResponse
{
    /// <summary>
    /// Merge identities response content <see cref="MergeIdentitiesResponseContent"/>
    /// </summary>
    public MergeIdentitiesResponseContent Content { get; set; }
}

public class DOH_MergeClientIdentityResponse : BaseResponse
{
    /// <summary>
    /// Merge identities response content <see cref="MergeIdentitiesResponseContent"/>
    /// </summary>
    public dynamic Content { get; set; }
}
