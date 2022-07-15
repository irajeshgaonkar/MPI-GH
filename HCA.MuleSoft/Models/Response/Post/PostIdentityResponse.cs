namespace HCA.MuleSoft.Models.Response.Post;

/// <summary>
/// Post Identity Response Content
/// </summary>
public class PostIdentityResponse : BaseResponse<PostIdentityResponseContent>
{
    /// <summary>
    /// <see cref="PostIdentityResponse"/>
    /// </summary>
    /// <param name="trackingId">Tracking Id for the request and response</param>
    /// <param name="auditId">Unique Id provided by verato</param>
    /// <param name="retryableError">whether retyable error or not</param>
    /// <param name="message">Message, if any</param>
    /// <param name="errors">Human readable errors, if any</param>
    /// <param name="content">response content</param>
    public PostIdentityResponse(string trackingId, Guid auditId, bool retryableError, string message, List<string> errors, PostIdentityResponseContent content)
            : base(trackingId, auditId, retryableError, message, errors, content)
    {
    }
}

public class DemoGraphicSearchResponse: BaseResponse<DemographicSearchContent>
{
    public DemoGraphicSearchResponse(string trackingId, Guid auditId, bool retryableError, string message, List<string> errors, DemographicSearchContent content)
            : base(trackingId, auditId, retryableError, message, errors, content)
    {
    }

}

public class DemographicSearchContent
{
    public List<PostIdentityResponseContent> SearchResults { get; set; }
}



