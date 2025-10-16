namespace HCA.Models.Verato.Response;

/// <summary>
/// Post Identity Response Content
/// </summary>
public class PostIdentityResponse : VeratoResponse
{
    /// <summary>
    /// Post Identity response content <see cref="PostIdentityResponseContent"/>
    /// </summary>
    public PostIdentityResponseContent Content { get; set; }
}


public class DeleteIdentityResponse : VeratoResponse
{
    /// <summary>
    /// Post Identity response content <see cref="DeleteIdentityResponse"/>
    /// </summary>
    public DeleteIdentityResponseContent Content { get; set; }
}



public class DOH_DeleteIdentityResponse : VeratoResponse
{
    /// <summary>
    /// Post Identity response content <see cref="DeleteIdentityResponse"/>
    /// </summary>
    public DeleteIdentityResponseContent Content { get; set; }
}

