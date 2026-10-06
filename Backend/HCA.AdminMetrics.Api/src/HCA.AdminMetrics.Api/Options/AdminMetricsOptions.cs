namespace HCA.AdminMetrics.Api.Options;

/// <summary>
/// Represents the root configuration options for the Admin Metrics API.
/// </summary>
public class AdminMetricsOptions
{
    /// <summary>
    /// Gets the configuration section name.
    /// </summary>
    public const string SectionName = "AdminMetrics";

    /// <summary>
    /// Gets or sets the default dashboard lookback window in minutes.
    /// </summary>
    public int LookbackMinutes { get; set; } = 60;

    /// <summary>
    /// Gets or sets the CloudWatch aggregation period in seconds.
    /// </summary>
    public int PeriodSeconds { get; set; } = 300;

    /// <summary>
    /// Gets or sets the allowed tenant values for onboarded system management.
    /// </summary>
    public List<string> AllowedTenants { get; set; } =
    [
        "HHS Coalition",
        "Non-Coalition",
    ];

    /// <summary>
    /// Gets or sets the metric definitions keyed by logical metric name.
    /// </summary>
    public Dictionary<string, CloudWatchMetricDefinition> Metrics { get; set; } = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Gets or sets the configured dashboard thresholds.
    /// </summary>
    public AdminMetricThresholds Thresholds { get; set; } = new();

    /// <summary>
    /// Gets or sets the custom error log query options.
    /// </summary>
    public AdminCustomErrorLogOptions CustomErrorLogs { get; set; } = new();

    /// <summary>
    /// Gets or sets the usage metrics query options.
    /// </summary>
    public AdminUsageMetricsOptions UsageMetrics { get; set; } = new();
}

/// <summary>
/// Represents a CloudWatch metric definition used by the dashboard.
/// </summary>
public class CloudWatchMetricDefinition
{
    /// <summary>
    /// Gets or sets the CloudWatch namespace.
    /// </summary>
    public string Namespace { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the metric name.
    /// </summary>
    public string MetricName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the CloudWatch statistic to request.
    /// </summary>
    public string Statistic { get; set; } = "Average";

    /// <summary>
    /// Gets or sets the optional CloudWatch unit.
    /// </summary>
    public string? Unit { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the metric should be normalized to a per-minute rate.
    /// </summary>
    public bool NormalizeToPerMinute { get; set; }

    /// <summary>
    /// Gets or sets the metric dimensions.
    /// </summary>
    public Dictionary<string, string> Dimensions { get; set; } = new(StringComparer.OrdinalIgnoreCase);
}

/// <summary>
/// Represents the threshold values used to classify dashboard health states.
/// </summary>
public class AdminMetricThresholds
{
    /// <summary>
    /// Gets or sets the warning latency threshold in milliseconds.
    /// </summary>
    public double WarningLatencyMs { get; set; } = 500;

    /// <summary>
    /// Gets or sets the critical latency threshold in milliseconds.
    /// </summary>
    public double CriticalLatencyMs { get; set; } = 1000;

    /// <summary>
    /// Gets or sets the warning error-rate threshold as a percentage.
    /// </summary>
    public double WarningErrorRatePercent { get; set; } = 1;

    /// <summary>
    /// Gets or sets the critical error-rate threshold as a percentage.
    /// </summary>
    public double CriticalErrorRatePercent { get; set; } = 3;

    /// <summary>
    /// Gets or sets the warning timeout threshold.
    /// </summary>
    public double WarningTimeoutCount { get; set; } = 1;

    /// <summary>
    /// Gets or sets the critical timeout threshold.
    /// </summary>
    public double CriticalTimeoutCount { get; set; } = 5;

    /// <summary>
    /// Gets or sets the warning concurrency threshold.
    /// </summary>
    public double WarningConcurrency { get; set; } = 50;

    /// <summary>
    /// Gets or sets the critical concurrency threshold.
    /// </summary>
    public double CriticalConcurrency { get; set; } = 80;
}

/// <summary>
/// Represents configuration for usage metrics queries.
/// </summary>
public class AdminUsageMetricsOptions
{
    /// <summary>
    /// Gets or sets the default usage lookback window in hours.
    /// </summary>
    public int LookbackHours { get; set; } = 24;

    /// <summary>
    /// Gets or sets the polling interval in milliseconds while waiting for query completion.
    /// </summary>
    public int PollIntervalMilliseconds { get; set; } = 1000;

    /// <summary>
    /// Gets or sets the query timeout in seconds.
    /// </summary>
    public int QueryTimeoutSeconds { get; set; } = 20;

    /// <summary>
    /// Gets or sets the maximum number of rows to request.
    /// </summary>
    public int Limit { get; set; } = 200;

    /// <summary>
    /// Gets or sets the CloudWatch Logs log group name.
    /// </summary>
    public string LogGroupName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the CloudWatch Logs Insights query string.
    /// </summary>
    public string QueryString { get; set; } = string.Empty;
}

/// <summary>
/// Represents configuration for custom application error log queries.
/// </summary>
public class AdminCustomErrorLogOptions
{
    /// <summary>
    /// Gets or sets the polling interval in milliseconds while waiting for query completion.
    /// </summary>
    public int PollIntervalMilliseconds { get; set; } = 1000;

    /// <summary>
    /// Gets or sets the query timeout in seconds.
    /// </summary>
    public int QueryTimeoutSeconds { get; set; } = 20;

    /// <summary>
    /// Gets or sets the maximum number of rows to request.
    /// </summary>
    public int Limit { get; set; } = 50;

    /// <summary>
    /// Gets or sets the CloudWatch Logs log group name.
    /// </summary>
    public string LogGroupName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the CloudWatch Logs Insights query string.
    /// </summary>
    public string QueryString { get; set; } = string.Empty;
}
