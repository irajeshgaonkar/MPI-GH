using HCA.Models.Verato.Response;

namespace HCA.Models.Response;

public class PostClientIdentityResponse : BaseResponse
{
    /// <summary>
    /// Post Identity response content <see cref="PostIdentityResponseContent"/>
    /// </summary>
    public PostIdentityResponseContent Content { get; set; }
}

public class DOH_PostClientIdentityResponse : BaseResponse
{
    /// <summary>
    /// Post Identity response content <see cref="PostIdentityResponseContent"/>
    /// </summary>
    public dynamic Content { get; set; }
}


public class DeleteClientIdentityResponse : BaseResponse
{
    /// <summary>
    /// Post Identity response content <see cref="PostIdentityResponseContent"/>
    /// </summary>
    public DeleteIdentityResponseContent Content { get; set; }
}

public class DOH_DeleteClientIdentityResponse : BaseResponse
{
    /// <summary>
    /// Post Identity response content <see cref="PostIdentityResponseContent"/>
    /// </summary>
    public DeleteIdentityResponseContent Content { get; set; }
}

public class CreateDataSourceClientIdentityResponse : BaseResponse
{
    public CreateDataSourceResponseContent Content { get; set; }
}
