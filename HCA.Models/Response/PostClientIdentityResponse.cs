using HCA.Models.MuleSoft.Response;

namespace HCA.Models.Response;

public class PostClientIdentityResponse : BaseResponse
{
    /// <summary>
    /// Post Identity response content <see cref="PostIdentityResponseContent"/>
    /// </summary>
    public PostIdentityResponseContent Content { get; set; }
}

public class DeleteClientIdentityResponse : BaseResponse
{
    /// <summary>
    /// Post Identity response content <see cref="PostIdentityResponseContent"/>
    /// </summary>
    public DeleteIdentityResponseContent Content { get; set; }
}
