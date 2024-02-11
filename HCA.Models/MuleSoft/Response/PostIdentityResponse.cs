namespace HCA.Models.MuleSoft.Response;

/// <summary>
/// Post Identity Response Content
/// </summary>
public class PostIdentityResponse : MuleSoftResponse
{
    /// <summary>
    /// Post Identity response content <see cref="PostIdentityResponseContent"/>
    /// </summary>
    public PostIdentityResponseContent Content { get; set; }
}


public class DeleteIdentityResponse : MuleSoftResponse
{
    /// <summary>
    /// Post Identity response content <see cref="DeleteIdentityResponse"/>
    /// </summary>
    public DeleteIdentityResponseContent Content { get; set; }
}



public class DOH_DeleteIdentityResponse : MuleSoftResponse
{
    /// <summary>
    /// Post Identity response content <see cref="DeleteIdentityResponse"/>
    /// </summary>
    public DeleteIdentityResponseContent Content { get; set; }
}

