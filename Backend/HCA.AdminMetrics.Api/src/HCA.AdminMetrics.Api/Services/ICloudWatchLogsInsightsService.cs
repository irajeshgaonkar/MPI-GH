using Amazon.CloudWatchLogs.Model;

namespace HCA.AdminMetrics.Api.Services;

/// <summary>
/// Defines CloudWatch Logs Insights query operations.
/// </summary>
public interface ICloudWatchLogsInsightsService
{
    /// <summary>
    /// Starts a CloudWatch Logs Insights query and waits for completion.
    /// </summary>
    Task<GetQueryResultsResponse> QueryAsync(
        DateTime startTime,
        DateTime endTime,
        string logGroupName,
        string queryString,
        int limit,
        int queryTimeoutSeconds,
        int pollIntervalMilliseconds,
        CancellationToken cancellationToken);
}
