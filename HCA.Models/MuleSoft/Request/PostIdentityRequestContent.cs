namespace HCA.Models.MuleSoft.Request;

/// <summary>
/// Post Identity Request Content
/// </summary>
public class PostIdentityRequestContent
{
    public PostIdentityRequestContent(Identity identity)
    {

        ResponseIdentityFormatNames = new string[] { "GROUP_BY_SOURCE" };
        Identity = identity;
    }

    /// <summary>
    /// Specifies the response identity format
    /// </summary>
    /// <remarks> Available values : DEFAULT, GROUP_BY_SOURCE </remarks>
    public string[] ResponseIdentityFormatNames { get; set; }

    /// <summary>
    /// Identity request object
    /// </summary>
    public Identity Identity { get; set; }
}

