using Amazon.CloudWatchLogs;
using Amazon.CloudWatchLogs.Model;
using HCA.AdminMetrics.Api.Models;
using HCA.AdminMetrics.Api.Options;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace HCA.AdminMetrics.Api.Services;

/// <summary>
/// Provides API traffic analytics sourced from CloudWatch Logs Insights.
/// </summary>
public class AdminApiTrafficService : IAdminApiTrafficService
{
    /// <summary>
    /// Fixed lookback for the busiest-hours chart (independent of page lookback).
    /// </summary>
    private const int BusiestHoursLookbackHours = 24;

    /// <summary>
    /// Fixed lookback for the busiest-days chart (independent of page lookback).
    /// </summary>
    private const int BusiestDaysLookbackDays = 30;

    /// <summary>
    /// Fixed lookback for the busiest-months chart (independent of page lookback).
    /// </summary>
    private const int BusiestMonthsLookbackDays = 180;

    private readonly IAmazonCloudWatchLogs _cloudWatchLogs;
    private readonly IMemoryCache _cache;
    private readonly AdminApiTrafficOptions _options;
    private readonly ILogger<AdminApiTrafficService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="AdminApiTrafficService"/> class.
    /// </summary>
    public AdminApiTrafficService(
        IAmazonCloudWatchLogs cloudWatchLogs,
        IMemoryCache cache,
        IOptions<AdminMetricsOptions> options,
        ILogger<AdminApiTrafficService> logger)
    {
        _cloudWatchLogs = cloudWatchLogs;
        _cache = cache;
        _options = options.Value.ApiTraffic;
        _logger = logger;
    }

    /// <inheritdoc />
    public Task<AdminApiTrafficMetricResponse<AdminApiTrafficDayCount>> GetCallsPerDayAsync(
        int? lookbackHours,
        bool refresh,
        CancellationToken cancellationToken)
    {
        return GetMetricAsync(
            AdminApiTrafficMetric.CallsPerDay,
            lookbackHours,
            refresh,
            results => MapDayCounts(results, byApiCalls: false),
            cancellationToken);
    }

    /// <inheritdoc />
    public Task<AdminApiTrafficMetricResponse<AdminApiTrafficEndpointCount>> GetTopEndpointsAsync(
        int? lookbackHours,
        bool refresh,
        CancellationToken cancellationToken)
    {
        return GetMetricAsync(
            AdminApiTrafficMetric.TopEndpoints,
            lookbackHours,
            refresh,
            MapEndpointCounts,
            cancellationToken);
    }

    /// <inheritdoc />
    public Task<AdminApiTrafficMetricResponse<AdminApiTrafficHourCount>> GetBusiestHoursAsync(
        bool refresh,
        CancellationToken cancellationToken)
    {
        return GetMetricAsync(
            AdminApiTrafficMetric.BusiestHours,
            lookbackHours: null,
            refresh,
            MapHourCounts,
            cancellationToken);
    }

    /// <inheritdoc />
    public Task<AdminApiTrafficMetricResponse<AdminApiTrafficDayCount>> GetBusiestDaysAsync(
        bool refresh,
        CancellationToken cancellationToken)
    {
        return GetMetricAsync(
            AdminApiTrafficMetric.BusiestDays,
            lookbackHours: null,
            refresh,
            results => MapDayCounts(results, byApiCalls: true),
            cancellationToken);
    }

    /// <inheritdoc />
    public Task<AdminApiTrafficMetricResponse<AdminApiTrafficMonthCount>> GetBusiestMonthsAsync(
        bool refresh,
        CancellationToken cancellationToken)
    {
        return GetMetricAsync(
            AdminApiTrafficMetric.BusiestMonths,
            lookbackHours: null,
            refresh,
            MapMonthCounts,
            cancellationToken);
    }

    private async Task<AdminApiTrafficMetricResponse<T>> GetMetricAsync<T>(
        AdminApiTrafficMetric metric,
        int? lookbackHours,
        bool refresh,
        Func<List<List<ResultField>>, IReadOnlyList<T>> mapResults,
        CancellationToken cancellationToken)
    {
        var endTime = DateTime.UtcNow;
        var effectiveLookbackHours = ResolveEffectiveLookbackHours(metric, lookbackHours);

        var cacheKey = BuildCacheKey(metric, effectiveLookbackHours);
        if (!refresh && _cache.TryGetValue(cacheKey, out AdminApiTrafficMetricResponse<T>? cached) && cached is not null)
        {
            _logger.LogDebug("API traffic cache hit for '{CacheKey}'.", cacheKey);
            return cached;
        }

        if (refresh)
        {
            _logger.LogDebug("API traffic cache bypass (refresh) for '{CacheKey}'.", cacheKey);
        }
        else
        {
            _logger.LogDebug("API traffic cache miss for '{CacheKey}'.", cacheKey);
        }

        var response = new AdminApiTrafficMetricResponse<T>
        {
            LookbackHours = effectiveLookbackHours,
            LastUpdated = endTime
        };

        if (string.IsNullOrWhiteSpace(_options.LogGroupName))
        {
            response.Message = "API traffic query is not configured.";
            return response;
        }

        var queryString = ResolveQueryString(metric);
        if (string.IsNullOrWhiteSpace(queryString))
        {
            response.Message = "API traffic query is not configured.";
            return response;
        }

        var startTime = endTime.AddHours(-effectiveLookbackHours);

        try
        {
            var queryId = await StartQueryAsync(startTime, endTime, queryString, cancellationToken);
            var results = await WaitForResultsAsync(queryId, cancellationToken);
            response.Items = mapResults(results.Results);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "API traffic Insights query '{Metric}' failed.", metric);
            response.Message = $"API traffic query failed for {metric}.";
            response.Items = Array.Empty<T>();
        }

        if (string.IsNullOrWhiteSpace(response.Message))
        {
            var cacheSeconds = Math.Max(_options.CacheSeconds, 1);
            _cache.Set(
                cacheKey,
                response,
                new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(cacheSeconds)
                });
            _logger.LogDebug("API traffic cache store for '{CacheKey}' ({CacheSeconds}s).", cacheKey, cacheSeconds);
        }

        return response;
    }

    private static string BuildCacheKey(AdminApiTrafficMetric metric, int effectiveLookbackHours)
    {
        return $"api-traffic:{metric}:{effectiveLookbackHours}";
    }

    private int ResolveEffectiveLookbackHours(AdminApiTrafficMetric metric, int? lookbackHours)
    {
        return metric switch
        {
            AdminApiTrafficMetric.BusiestHours => BusiestHoursLookbackHours,
            AdminApiTrafficMetric.BusiestDays => BusiestDaysLookbackDays * 24,
            AdminApiTrafficMetric.BusiestMonths => BusiestMonthsLookbackDays * 24,
            _ => NormalizeLookbackHours(lookbackHours)
        };
    }

    private string? ResolveQueryString(AdminApiTrafficMetric metric)
    {
        return metric switch
        {
            AdminApiTrafficMetric.CallsPerDay => _options.CallsPerDayQuery,
            AdminApiTrafficMetric.TopEndpoints => _options.TopEndpointsQuery,
            AdminApiTrafficMetric.BusiestHours => _options.BusiestHoursQuery,
            AdminApiTrafficMetric.BusiestDays => _options.BusiestDaysQuery,
            AdminApiTrafficMetric.BusiestMonths => _options.BusiestMonthsQuery,
            _ => null
        };
    }

    private async Task<string> StartQueryAsync(
        DateTime startTime,
        DateTime endTime,
        string queryString,
        CancellationToken cancellationToken)
    {
        var request = new StartQueryRequest
        {
            StartTime = new DateTimeOffset(startTime).ToUnixTimeSeconds(),
            EndTime = new DateTimeOffset(endTime).ToUnixTimeSeconds(),
            LogGroupNames = [_options.LogGroupName],
            QueryString = queryString,
            Limit = _options.Limit
        };

        var response = await _cloudWatchLogs.StartQueryAsync(request, cancellationToken);
        return response.QueryId;
    }

    private async Task<GetQueryResultsResponse> WaitForResultsAsync(string queryId, CancellationToken cancellationToken)
    {
        var timeoutAt = DateTime.UtcNow.AddSeconds(Math.Max(_options.QueryTimeoutSeconds, 5));

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

            await Task.Delay(Math.Max(_options.PollIntervalMilliseconds, 250), cancellationToken);
        }
    }

    private static IReadOnlyList<AdminApiTrafficDayCount> MapDayCounts(
        List<List<ResultField>> results,
        bool byApiCalls)
    {
        var rows = results
            .Select(MapFieldDictionary)
            .Select(values => new AdminApiTrafficDayCount
            {
                Day = ParseDateTime(values.GetValueOrDefault("day")) ?? DateTime.MinValue,
                ApiCalls = ParseInt(values.GetValueOrDefault("apiCalls"))
            })
            .Where(row => row.Day != DateTime.MinValue)
            .ToList();

        return byApiCalls
            ? rows.OrderByDescending(row => row.ApiCalls).ThenByDescending(row => row.Day).ToList()
            : rows.OrderByDescending(row => row.Day).ToList();
    }

    private static IReadOnlyList<AdminApiTrafficHourCount> MapHourCounts(List<List<ResultField>> results)
    {
        return results
            .Select(MapFieldDictionary)
            .Select(values => new AdminApiTrafficHourCount
            {
                Hour = ParseDateTime(values.GetValueOrDefault("hour")) ?? DateTime.MinValue,
                ApiCalls = ParseInt(values.GetValueOrDefault("apiCalls"))
            })
            .Where(row => row.Hour != DateTime.MinValue)
            .OrderByDescending(row => row.ApiCalls)
            .ThenByDescending(row => row.Hour)
            .ToList();
    }

    private static IReadOnlyList<AdminApiTrafficMonthCount> MapMonthCounts(List<List<ResultField>> results)
    {
        return results
            .Select(MapFieldDictionary)
            .Select(values => new AdminApiTrafficMonthCount
            {
                Month = ParseDateTime(values.GetValueOrDefault("month")) ?? DateTime.MinValue,
                ApiCalls = ParseInt(values.GetValueOrDefault("apiCalls"))
            })
            .Where(row => row.Month != DateTime.MinValue)
            .OrderBy(row => row.Month)
            .ToList();
    }

    private static IReadOnlyList<AdminApiTrafficEndpointCount> MapEndpointCounts(List<List<ResultField>> results)
    {
        return results
            .Select(MapFieldDictionary)
            .Select(values => new AdminApiTrafficEndpointCount
            {
                Path = values.GetValueOrDefault("path") ?? string.Empty,
                Calls = ParseInt(values.GetValueOrDefault("calls"))
            })
            .Where(row => !string.IsNullOrWhiteSpace(row.Path))
            .OrderByDescending(row => row.Calls)
            .ThenBy(row => row.Path, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static Dictionary<string, string?> MapFieldDictionary(List<ResultField> fields)
    {
        return fields
            .Where(field => !string.IsNullOrWhiteSpace(field.Field))
            .GroupBy(field => field.Field!, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(group => group.Key, group => (string?)group.Last().Value, StringComparer.OrdinalIgnoreCase);
    }

    private int NormalizeLookbackHours(int? lookbackHours)
    {
        if (lookbackHours is null || lookbackHours <= 0)
        {
            return Math.Max(_options.LookbackHours, 1);
        }

        return Math.Abs(lookbackHours.Value);
    }

    private static int ParseInt(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return 0;
        }

        if (int.TryParse(value, out var intValue))
        {
            return intValue;
        }

        if (double.TryParse(value, out var doubleValue))
        {
            return (int)Math.Floor(doubleValue);
        }

        return 0;
    }

    private static DateTime? ParseDateTime(string? value)
    {
        return DateTime.TryParse(value, out var parsed) ? DateTime.SpecifyKind(parsed, DateTimeKind.Utc) : null;
    }
}
