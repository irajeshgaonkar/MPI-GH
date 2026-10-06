using Amazon.CloudWatchLogs;
using Amazon.CloudWatchLogs.Model;

namespace HCA.AdminMetrics.Api.Services;

/// <summary>
/// Executes CloudWatch Logs Insights queries.
/// </summary>
public class CloudWatchLogsInsightsService(IAmazonCloudWatchLogs cloudWatchLogs) : ICloudWatchLogsInsightsService
{
    private readonly IAmazonCloudWatchLogs _cloudWatchLogs = cloudWatchLogs;

    /// <inheritdoc />
    public async Task<GetQueryResultsResponse> QueryAsync(
        DateTime startTime,
        DateTime endTime,
        string logGroupName,
        string queryString,
        int limit,
        int queryTimeoutSeconds,
        int pollIntervalMilliseconds,
        CancellationToken cancellationToken)
    {
        var response = await _cloudWatchLogs.StartQueryAsync(new StartQueryRequest
        {
            StartTime = new DateTimeOffset(startTime).ToUnixTimeSeconds(),
            EndTime = new DateTimeOffset(endTime).ToUnixTimeSeconds(),
            LogGroupNames = [logGroupName],
            QueryString = queryString,
            Limit = limit
        }, cancellationToken);

        return await WaitForResultsAsync(
            response.QueryId,
            Math.Max(queryTimeoutSeconds, 5),
            Math.Max(pollIntervalMilliseconds, 250),
            cancellationToken);
    }

    private async Task<GetQueryResultsResponse> WaitForResultsAsync(
        string queryId,
        int queryTimeoutSeconds,
        int pollIntervalMilliseconds,
        CancellationToken cancellationToken)
    {
        var timeoutAt = DateTime.UtcNow.AddSeconds(queryTimeoutSeconds);

        while (true)
        {
            var response = await _cloudWatchLogs.GetQueryResultsAsync(new GetQueryResultsRequest
            {
                QueryId = queryId
            }, cancellationToken);

            switch (response.Status)
            {
                case "Complete":
                    return response;
                case "Failed":
                case "Cancelled":
                case "Timeout":
                    throw new System.InvalidOperationException($"CloudWatch Logs query ended with status '{response.Status}'.");
            }

            if (DateTime.UtcNow >= timeoutAt)
            {
                throw new TimeoutException("Timed out waiting for CloudWatch Logs Insights query results.");
            }

            await Task.Delay(pollIntervalMilliseconds, cancellationToken);
        }
    }
}
