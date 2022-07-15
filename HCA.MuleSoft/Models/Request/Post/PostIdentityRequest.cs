namespace HCA.MuleSoft.Models.Request.Post;

/// <summary>
/// Post Identity Request
/// </summary>
public class PostIdentityRequest : BaseRequest<PostIdentityRequestContent>
{
    public PostIdentityRequest(string trackingId, PostIdentityRequestContent content) : base(trackingId, content)
    {
        Content = content;
    }
}

