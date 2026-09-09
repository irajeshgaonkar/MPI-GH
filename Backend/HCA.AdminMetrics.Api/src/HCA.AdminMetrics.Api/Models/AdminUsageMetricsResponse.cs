namespace HCA.AdminMetrics.Api.Models;

/// <summary>
/// Represents the top-level usage metrics response.
/// </summary>
public class AdminUsageMetricsResponse
{
    /// <summary>
    /// Gets or sets the lookback window in hours.
    /// </summary>
    public int LookbackHours { get; set; }

    /// <summary>
    /// Gets or sets the total number of systems represented in the response.
    /// </summary>
    public int TotalSystems { get; set; }

    /// <summary>
    /// Gets or sets the total number of calls across all systems.
    /// </summary>
    public int TotalCalls { get; set; }

    /// <summary>
    /// Gets or sets the total number of successful calls across all systems.
    /// </summary>
    public int TotalSuccessCount { get; set; }

    /// <summary>
    /// Gets or sets the total number of failed calls across all systems.
    /// </summary>
    public int TotalFailedCount { get; set; }

    /// <summary>
    /// Gets or sets the total number of distinct source IPs observed.
    /// </summary>
    public int TotalDistinctSourceIps { get; set; }

    /// <summary>
    /// Gets or sets the average failure rate across all systems.
    /// </summary>
    public double AverageFailureRate { get; set; }

    /// <summary>
    /// Gets or sets the UTC timestamp when the response was generated.
    /// </summary>
    public DateTime LastUpdated { get; set; }

    /// <summary>
    /// Gets or sets an optional informational message when usage metrics are unavailable or partially configured.
    /// </summary>
    public string? Message { get; set; }

    /// <summary>
    /// Gets or sets the usage metric rows grouped by system.
    /// </summary>
    public List<AdminUsageMetricsRow> Systems { get; set; } = [];
}

/// <summary>
/// Represents usage metrics for a single system.
/// </summary>
public class AdminUsageMetricsRow
{
    /// <summary>
    /// Gets or sets the resolved system name.
    /// </summary>
    public string System { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the tenant associated with the system.
    /// </summary>
    public string Tenant { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the raw certificate common name values associated with the system.
    /// </summary>
    public string RawCertificateCn { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the number of calls observed for the system.
    /// </summary>
    public int CallCount { get; set; }

    /// <summary>
    /// Gets or sets the number of failed calls observed for the system.
    /// </summary>
    public int FailedCount { get; set; }

    /// <summary>
    /// Gets or sets the number of successful calls observed for the system.
    /// </summary>
    public int SuccessCount { get; set; }

    /// <summary>
    /// Gets or sets the failure rate for the system.
    /// </summary>
    public double FailureRate { get; set; }

    /// <summary>
    /// Gets or sets the first observed timestamp for the system.
    /// </summary>
    public DateTime? FirstSeen { get; set; }

    /// <summary>
    /// Gets or sets the last observed timestamp for the system.
    /// </summary>
    public DateTime? LastSeen { get; set; }

    /// <summary>
    /// Gets or sets the count of distinct source IPs for the system.
    /// </summary>
    public int SourceIpCount { get; set; }
}
