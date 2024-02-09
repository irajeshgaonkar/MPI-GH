using HCA.Models.Response;

namespace HCA.Models.MuleSoft.Response;

/// <summary>
/// Post Identity Response Content
/// </summary>
public class DOH_PostIdentityResponse : MuleSoftResponse
{
    /// <summary>
    /// Post Identity response content <see cref="PostIdentityResponseContent"/>
    /// </summary>
    public dynamic Content { get; set; }
}






