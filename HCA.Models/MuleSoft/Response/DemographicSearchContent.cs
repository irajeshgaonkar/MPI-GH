
namespace HCA.Models.MuleSoft.Response;

/// <summary>
/// Demographic search content
/// </summary>
public class DemographicSearchContent
{
    /// <summary>
    /// collection of search results <see cref="PostIdentityResponseContent"/>
    /// </summary>
    public List<PostIdentityResponseContent> SearchResults { get; set; }
}



