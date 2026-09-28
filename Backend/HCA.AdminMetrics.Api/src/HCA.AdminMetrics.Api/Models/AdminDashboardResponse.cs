using System.Text.Json.Serialization;

namespace HCA.AdminMetrics.Api.Models;

/// <summary>
/// Represents the top-level MPI Health dashboard response.
/// </summary>
public class AdminDashboardResponse
{
    /// <summary>
    /// Gets or sets the summary metrics for the dashboard.
    /// </summary>
    public required AdminSummary Summary { get; init; }

    /// <summary>
    /// Gets or sets the configured health thresholds used to interpret the metrics.
    /// </summary>
    public required AdminConfiguredThresholds Thresholds { get; init; }

    /// <summary>
    /// Gets or sets the time-series trend collections shown on the dashboard.
    /// </summary>
    public required AdminTrendCollection Trends { get; init; }

    /// <summary>
    /// Gets or sets the custom application error information.
    /// </summary>
    public required AdminCustomErrorCollection CustomErrors { get; init; }

    /// <summary>
    /// Gets or sets the Verato error information.
    /// </summary>
    public required AdminVeratoErrorCollection VeratoErrors { get; init; }

    /// <summary>
    /// Gets or sets the generated alert collection.
    /// </summary>
    public required List<AdminAlert> Alerts { get; init; }

    /// <summary>
    /// Gets or sets the UTC timestamp when the dashboard payload was generated.
    /// </summary>
    public required DateTime LastUpdated { get; init; }
}

/// <summary>
/// Represents the top-line MPI Health summary metrics.
/// </summary>
public class AdminSummary
{
    /// <summary>
    /// Gets or sets the overall health status label.
    /// </summary>
    public required string HealthStatus { get; init; }

    /// <summary>
    /// Gets or sets the average latency in milliseconds.
    /// </summary>
    public required double AvgLatencyMs { get; init; }

    /// <summary>
    /// Gets or sets the P95 latency in milliseconds.
    /// </summary>
    public required double P95LatencyMs { get; init; }

    /// <summary>
    /// Gets or sets the error rate percentage.
    /// </summary>
    public required double ErrorRate { get; init; }

    /// <summary>
    /// Gets or sets the requests per minute value.
    /// </summary>
    public required double RequestsPerMinute { get; init; }

    /// <summary>
    /// Gets or sets the lambda concurrency value.
    /// </summary>
    public required double LambdaConcurrency { get; init; }

    /// <summary>
    /// Gets or sets the timeout count.
    /// </summary>
    public required double TimeoutCount { get; init; }

    /// <summary>
    /// Gets or sets the downstream dependency latency in milliseconds.
    /// </summary>
    public required double DependencyLatencyMs { get; init; }
}

/// <summary>
/// Represents the configured threshold values used by the dashboard.
/// </summary>
public class AdminConfiguredThresholds
{
    /// <summary>
    /// Gets or sets the warning latency threshold in milliseconds.
    /// </summary>
    public required double WarningLatencyMs { get; init; }

    /// <summary>
    /// Gets or sets the critical latency threshold in milliseconds.
    /// </summary>
    public required double CriticalLatencyMs { get; init; }

    /// <summary>
    /// Gets or sets the warning error-rate threshold as a percentage.
    /// </summary>
    public required double WarningErrorRatePercent { get; init; }

    /// <summary>
    /// Gets or sets the critical error-rate threshold as a percentage.
    /// </summary>
    public required double CriticalErrorRatePercent { get; init; }

    /// <summary>
    /// Gets or sets the warning timeout count threshold.
    /// </summary>
    public required double WarningTimeoutCount { get; init; }

    /// <summary>
    /// Gets or sets the critical timeout count threshold.
    /// </summary>
    public required double CriticalTimeoutCount { get; init; }

    /// <summary>
    /// Gets or sets the warning concurrency threshold.
    /// </summary>
    public required double WarningConcurrency { get; init; }

    /// <summary>
    /// Gets or sets the critical concurrency threshold.
    /// </summary>
    public required double CriticalConcurrency { get; init; }
}

/// <summary>
/// Represents the time-series collections used by the dashboard charts.
/// </summary>
public class AdminTrendCollection
{
    /// <summary>
    /// Gets or sets the latency trend points.
    /// </summary>
    public required List<LatencyTrendPoint> Latency { get; init; }

    /// <summary>
    /// Gets or sets the throughput trend points.
    /// </summary>
    public required List<ThroughputTrendPoint> Throughput { get; init; }

    /// <summary>
    /// Gets or sets the error trend points.
    /// </summary>
    public required List<ErrorTrendPoint> Errors { get; init; }

    /// <summary>
    /// Gets or sets the timeout trend points.
    /// </summary>
    public required List<TimeoutTrendPoint> Timeouts { get; init; }

    /// <summary>
    /// Gets or sets the concurrency trend points.
    /// </summary>
    public required List<ConcurrencyTrendPoint> Concurrency { get; init; }
}

/// <summary>
/// Represents a latency trend point.
/// </summary>
public class LatencyTrendPoint
{
    /// <summary>
    /// Gets or sets the point timestamp.
    /// </summary>
    public required DateTime Timestamp { get; init; }

    /// <summary>
    /// Gets or sets the average latency at the point.
    /// </summary>
    public required double Avg { get; init; }

    /// <summary>
    /// Gets or sets the P95 latency at the point.
    /// </summary>
    public required double P95 { get; init; }
}

/// <summary>
/// Represents a throughput trend point.
/// </summary>
public class ThroughputTrendPoint
{
    /// <summary>
    /// Gets or sets the point timestamp.
    /// </summary>
    public required DateTime Timestamp { get; init; }

    /// <summary>
    /// Gets or sets the requests-per-minute value.
    /// </summary>
    public required double Rpm { get; init; }
}

/// <summary>
/// Represents an error trend point.
/// </summary>
public class ErrorTrendPoint
{
    /// <summary>
    /// Gets or sets the point timestamp.
    /// </summary>
    public required DateTime Timestamp { get; init; }

    /// <summary>
    /// Gets or sets the 4xx error count.
    /// </summary>
    [JsonPropertyName("4xx")]
    public required double FourXx { get; init; }

    /// <summary>
    /// Gets or sets the 5xx error count.
    /// </summary>
    [JsonPropertyName("5xx")]
    public required double FiveXx { get; init; }
}

/// <summary>
/// Represents a timeout trend point.
/// </summary>
public class TimeoutTrendPoint
{
    /// <summary>
    /// Gets or sets the point timestamp.
    /// </summary>
    public required DateTime Timestamp { get; init; }

    /// <summary>
    /// Gets or sets the timeout count at the point.
    /// </summary>
    public required double Count { get; init; }
}

/// <summary>
/// Represents a concurrency trend point.
/// </summary>
public class ConcurrencyTrendPoint
{
    /// <summary>
    /// Gets or sets the point timestamp.
    /// </summary>
    public required DateTime Timestamp { get; init; }

    /// <summary>
    /// Gets or sets the concurrency used at the point.
    /// </summary>
    public required double Used { get; init; }
}

/// <summary>
/// Represents an operational alert emitted for the dashboard.
/// </summary>
public class AdminAlert
{
    /// <summary>
    /// Gets or sets the alert severity.
    /// </summary>
    public required string Severity { get; init; }

    /// <summary>
    /// Gets or sets the alert message.
    /// </summary>
    public required string Message { get; init; }
}

/// <summary>
/// Represents custom application error data included in the dashboard.
/// </summary>
public class AdminCustomErrorCollection
{
    /// <summary>
    /// Gets or sets the custom error summary.
    /// </summary>
    public required AdminCustomErrorSummary Summary { get; init; }

    /// <summary>
    /// Gets or sets the custom error entries.
    /// </summary>
    public required List<AdminCustomErrorEntry> Entries { get; init; }
}

/// <summary>
/// Represents a summary of custom application errors.
/// </summary>
public class AdminCustomErrorSummary
{
    /// <summary>
    /// Gets or sets the total error count.
    /// </summary>
    public required int TotalErrors { get; init; }

    /// <summary>
    /// Gets or sets the number of distinct error codes.
    /// </summary>
    public required int DistinctErrorCodes { get; init; }

    /// <summary>
    /// Gets or sets the number of distinct functions.
    /// </summary>
    public required int DistinctFunctions { get; init; }

    /// <summary>
    /// Gets or sets the number of distinct tracking identifiers.
    /// </summary>
    public required int DistinctTrackingIds { get; init; }
}

/// <summary>
/// Represents a custom application error entry.
/// </summary>
public class AdminCustomErrorEntry
{
    /// <summary>
    /// Gets or sets the error timestamp.
    /// </summary>
    public required DateTime Timestamp { get; init; }

    /// <summary>
    /// Gets or sets the log name.
    /// </summary>
    public string? Name { get; init; }

    /// <summary>
    /// Gets or sets the logical layer where the error occurred.
    /// </summary>
    public string? Layer { get; init; }

    /// <summary>
    /// Gets or sets the function name associated with the error.
    /// </summary>
    public required string FunctionName { get; init; }

    /// <summary>
    /// Gets or sets the error code.
    /// </summary>
    public required string ErrorCode { get; init; }

    /// <summary>
    /// Gets or sets the error message.
    /// </summary>
    public required string Message { get; init; }

    /// <summary>
    /// Gets or sets the tracking identifier associated with the error.
    /// </summary>
    public string? TrackingId { get; init; }

    /// <summary>
    /// Gets or sets the CloudWatch log stream.
    /// </summary>
    public string? LogStream { get; init; }

    /// <summary>
    /// Gets or sets the CloudWatch log group name.
    /// </summary>
    public string? LogName { get; init; }
}

/// <summary>
/// Represents Verato error data included in the dashboard.
/// </summary>
public class AdminVeratoErrorCollection
{
    /// <summary>
    /// Gets or sets the Verato error summary.
    /// </summary>
    public required AdminVeratoErrorSummary Summary { get; init; }

    /// <summary>
    /// Gets or sets the Verato error trend points.
    /// </summary>
    public required List<AdminVeratoErrorTrendPoint> Trend { get; init; }

    /// <summary>
    /// Gets or sets the Verato error entries.
    /// </summary>
    public required List<AdminVeratoErrorEntry> Entries { get; init; }
}

/// <summary>
/// Represents a summary of Verato request failures.
/// </summary>
public class AdminVeratoErrorSummary
{
    /// <summary>
    /// Gets or sets the total error count.
    /// </summary>
    public required int TotalErrors { get; init; }

    /// <summary>
    /// Gets or sets the number of distinct operations that failed.
    /// </summary>
    public required int DistinctOperations { get; init; }

    /// <summary>
    /// Gets or sets the number of distinct tracking identifiers represented.
    /// </summary>
    public required int DistinctTrackingIds { get; init; }

    /// <summary>
    /// Gets or sets the total retry count across failed requests.
    /// </summary>
    public required int TotalRetries { get; init; }
}

/// <summary>
/// Represents a Verato error trend point.
/// </summary>
public class AdminVeratoErrorTrendPoint
{
    /// <summary>
    /// Gets or sets the point timestamp.
    /// </summary>
    public required DateTime Timestamp { get; init; }

    /// <summary>
    /// Gets or sets the failed request count at the point.
    /// </summary>
    public required double Count { get; init; }

    /// <summary>
    /// Gets or sets an optional display label for the point.
    /// </summary>
    public string? Label { get; init; }
}

/// <summary>
/// Represents a failed Verato request entry.
/// </summary>
public class AdminVeratoErrorEntry
{
    /// <summary>
    /// Gets or sets the failure timestamp.
    /// </summary>
    public required DateTime Timestamp { get; init; }

    /// <summary>
    /// Gets or sets the tracking identifier.
    /// </summary>
    public required string TrackingId { get; init; }

    /// <summary>
    /// Gets or sets the API call type.
    /// </summary>
    public required string ApiCallType { get; init; }

    /// <summary>
    /// Gets or sets the user name associated with the request.
    /// </summary>
    public required string UserName { get; init; }

    /// <summary>
    /// Gets or sets the request status.
    /// </summary>
    public required string Status { get; init; }

    /// <summary>
    /// Gets or sets the failure message.
    /// </summary>
    public required string Message { get; init; }

    /// <summary>
    /// Gets or sets the retry count for the request.
    /// </summary>
    public required int RetryCount { get; init; }

    /// <summary>
    /// Gets or sets the raw response payload when available.
    /// </summary>
    public string? ResponseJson { get; init; }
}
