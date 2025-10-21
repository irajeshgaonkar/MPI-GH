namespace HCA.Models.Verato.Response;

/// <summary>
/// Demographic search response
/// </summary>
public class DemographicSearchResponse: VeratoResponse
{
    /// <summary>
    /// Demographic search content <see cref="DemographicSearchContent"/>
    /// </summary>
    public DemographicSearchContent Content { get; set; }
}

public class DOH_DemographicSearchResponse : VeratoResponse
{
    /// <summary>
    /// Demographic search content <see cref="DemographicSearchContent"/>
    /// </summary>
    public DemographicSearchContent Content { get; set; }
}

/// <summary>
/// Demographic search response
/// </summary>
public class DemographicQueryResponse : VeratoResponse
{
    /// <summary>
    /// Demographic search content <see cref="DemographicQueryResponseContent"/>
    /// </summary>
    public DemographicQueryResponseContent Content { get; set; }
}

/// <summary>
/// 
/// </summary>
public class DOH_DemographicQueryResponse : VeratoResponse
{
    /// <summary>
    /// Demographic search content <see cref="DemographicQueryResponseContent"/>
    /// </summary>
    public dynamic Content { get; set; }
}



