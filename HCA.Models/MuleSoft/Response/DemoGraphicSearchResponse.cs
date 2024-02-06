
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

public class DOH_DemoGraphicSearchResponse : MuleSoftResponse
{
    /// <summary>
    /// Demogrpahic search content <see cref="DemographicSearchContent"/>
    /// </summary>
    public DemographicSearchContent Content { get; set; }
}

/// <summary>
/// Demographic search response
/// </summary>
public class DemoGraphicQueryResponse : MuleSoftResponse
{
    /// <summary>
    /// Demogrpahic search content <see cref="DemographicQueryResponseContent"/>
    /// </summary>
    public DemographicQueryResponseContent Content { get; set; }
}

/// <summary>
/// 
/// </summary>
public class DOH_DemoGraphicQueryResponse : MuleSoftResponse
{
    /// <summary>
    /// Demogrpahic search content <see cref="DemographicQueryResponseContent"/>
    /// </summary>
    public dynamic Content { get; set; }
}



