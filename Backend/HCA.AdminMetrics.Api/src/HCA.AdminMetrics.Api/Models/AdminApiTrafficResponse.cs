namespace HCA.AdminMetrics.Api.Models;

/// <summary>
/// Identifies a single API traffic Insights metric.
/// </summary>
public enum AdminApiTrafficMetric
{
    /// <summary>
    /// API call counts bucketed by day.
    /// </summary>
    CallsPerDay = 0,

    /// <summary>
    /// Top API endpoint paths by call volume.
    /// </summary>
    TopEndpoints = 1,

    /// <summary>
    /// Busiest hour buckets within the lookback window.
    /// </summary>
    BusiestHours = 2,

    /// <summary>
    /// Busiest day buckets within the lookback window.
    /// </summary>
    BusiestDays = 3,

    /// <summary>
    /// Busiest month buckets over the fixed last-six-months window.
    /// </summary>
    BusiestMonths = 4
}

/// <summary>
/// Represents a single API traffic metric response from CloudWatch Logs Insights.
/// </summary>
/// <typeparam name="T">The row type for the metric items.</typeparam>
public class AdminApiTrafficMetricResponse<T>
{
    /// <summary>
    /// Gets or sets the lookback window in hours that was used for the query.
    /// </summary>
    public int LookbackHours { get; set; }

    /// <summary>
    /// Gets or sets the UTC timestamp when the response was generated.
    /// </summary>
    public DateTime LastUpdated { get; set; }

    /// <summary>
    /// Gets or sets an optional message when the metric is unavailable.
    /// </summary>
    public string? Message { get; set; }

    /// <summary>
    /// Gets or sets the metric rows.
    /// </summary>
    public IReadOnlyList<T> Items { get; set; } = Array.Empty<T>();
}

/// <summary>
/// Represents an API call count for a calendar day bucket.
/// </summary>
public class AdminApiTrafficDayCount
{
    /// <summary>
    /// Gets or sets the day bucket timestamp (UTC).
    /// </summary>
    public DateTime Day { get; set; }

    /// <summary>
    /// Gets or sets the number of API calls in the bucket.
    /// </summary>
    public int ApiCalls { get; set; }
}

/// <summary>
/// Represents an API call count for an hour bucket.
/// </summary>
public class AdminApiTrafficHourCount
{
    /// <summary>
    /// Gets or sets the hour bucket timestamp (UTC).
    /// </summary>
    public DateTime Hour { get; set; }

    /// <summary>
    /// Gets or sets the number of API calls in the bucket.
    /// </summary>
    public int ApiCalls { get; set; }
}

/// <summary>
/// Represents an API call count for a calendar month bucket.
/// </summary>
public class AdminApiTrafficMonthCount
{
    /// <summary>
    /// Gets or sets the month bucket timestamp (UTC).
    /// </summary>
    public DateTime Month { get; set; }

    /// <summary>
    /// Gets or sets the number of API calls in the bucket.
    /// </summary>
    public int ApiCalls { get; set; }
}

/// <summary>
/// Represents call volume for a single API endpoint path.
/// </summary>
public class AdminApiTrafficEndpointCount
{
    /// <summary>
    /// Gets or sets the API path.
    /// </summary>
    public string Path { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the number of calls for the path.
    /// </summary>
    public int Calls { get; set; }
}
