using HCA.Models.Verato.Response;

namespace HCA.Models.Response;

public class UnMergeClientIdentityResponse : BaseResponse
{
    /// <summary>
    /// Un merged identities response content <see cref="UnMergeIdentitiesResponseContent"/>
    /// </summary>
    public UnMergeIdentitiesResponseContent Content { get; set; }
}


public class DOH_UnMergeClientIdentityResponse : BaseResponse
{
    /// <summary>
    /// Un merged identities response content <see cref="UnMergeIdentitiesResponseContent"/>
    /// </summary>
    public dynamic Content { get; set; }
}
