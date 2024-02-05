namespace HCA.Models.MuleSoft.Request;

/// <summary>
/// Post Identity Request Content
/// </summary>
public class PostIdentityRequestContent
{
    public PostIdentityRequestContent(dynamic identity)
    {
        ResponseIdentityFormatNames = new string[] { "GROUP_BY_SOURCE" };
        //if (String.IsNullOrEmpty(ResponseIdentityFormatNames[].First()))
        //{
        //    ResponseIdentityFormatNames = new string[] { "GROUP_BY_SOURCE" };

        //}
        //else
        //{
        //    ResponseIdentityFormatNames = responseIdentityFormatNames;
        //}
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
    public dynamic Identity { get; set; }
}

