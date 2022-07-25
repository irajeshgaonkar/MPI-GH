
namespace HCA.Models.MuleSoft.Response;

/// <summary>
/// Demographic search response
/// </summary>
public class DemoGraphicSearchResponse: MuleSoftResponse
{
    /// <summary>
    /// Demogrpahic search content <see cref="DemographicSearchContent"/>
    /// </summary>
    public DemographicSearchContent Content { get; set; }
}



