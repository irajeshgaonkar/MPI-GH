using Amazon.CloudWatch;
using Amazon.CloudWatch.Model;
using Amazon.CloudWatchLogs;
using Amazon.CloudWatchLogs.Model;
using HCA.Data;
using HCA.AdminMetrics.Api.Models;
using HCA.AdminMetrics.Api.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace HCA.AdminMetrics.Api.Services;

/// <summary>
/// Provides MPI Health dashboard metrics sourced from CloudWatch and supporting data stores.
/// </summary>
public class AdminMetricsService(
    IAmazonCloudWatch cloudWatch,
    IAmazonCloudWatchLogs cloudWatchLogs,
    HcaDbContext dbContext,
    IOptions<AdminMetricsOptions> options,
    ILogger<AdminMetricsService> logger ) : IAdminMetricsService
{
    private const string AvgLatencyKey = "AvgLatency";
    private const string P95LatencyKey = "P95Latency";
    private const string RequestCountKey = "RequestCount";
    private const string Error4xxKey = "Error4xx";
    private const string Error5xxKey = "Error5xx";
    private const string ConcurrencyKey = "Concurrency";
    private const string TimeoutCountKey = "TimeoutCount";
    private const string DependencyLatencyKey = "DependencyLatency";
    private readonly AdminMetricsOptions _options = options.Value;

    /// <inheritdoc />
    public async Task<AdminDashboardResponse> GetDashboardAsync(int? lookbackMinutes, CancellationToken cancellationToken)
    {
        if (_options.Metrics.Count == 0)
        {
            logger.LogWarning("Admin dashboard requested but no CloudWatch metrics are configured.");
            return BuildUnavailableDashboard("CloudWatch metric definitions are missing.");
        }

        var endTime = DateTime.UtcNow;
        var effectiveLookbackMinutes = NormalizeLookbackMinutes(lookbackMinutes);
        var startTime = endTime.AddMinutes(-effectiveLookbackMinutes);
        var metricSeries = await QueryCloudWatchAsync(startTime, endTime, cancellationToken);

        var latency = BuildLatencyTrend(metricSeries);
        var throughput = BuildThroughputTrend(metricSeries);
        var errors = BuildErrorTrend(metricSeries);
        var timeouts = BuildTimeoutTrend(metricSeries);
        var concurrency = BuildConcurrencyTrend(metricSeries);
        var customErrors = await QueryCustomErrorsAsync(startTime, endTime, cancellationToken);
        var veratoErrors = await QueryVeratoErrorsAsync(startTime, endTime, effectiveLookbackMinutes, cancellationToken);

        var latestLatency = GetLastByIndexOrDefault(latency);
        var latestThroughput = GetLastByIndexOrDefault(throughput);
        var latestError = GetLastByIndexOrDefault(errors);
        var latestTimeout = GetLastByIndexOrDefault(timeouts);
        var latestConcurrency = GetLastByIndexOrDefault(concurrency);
        var dependencyLatency = LatestValue(metricSeries, DependencyLatencyKey);

        var errorRate = CalculateErrorRate(latestError, latestThroughput);
        var healthStatus = DetermineHealthStatus(
            latestLatency?.P95 ?? 0,
            errorRate,
            latestTimeout?.Count ?? 0,
            latestConcurrency?.Used ?? 0);

        var alerts = BuildAlerts(latency, throughput, errors, timeouts, concurrency, dependencyLatency).ToList();
        if (veratoErrors.Summary.TotalErrors > 0)
        {
            alerts.Add(new AdminAlert
            {
                Severity = veratoErrors.Summary.TotalErrors >= 10 ? "critical" : "warning",
                Message = $"Verato failed requests recorded: {veratoErrors.Summary.TotalErrors} in the selected window."
            });
        }

        return new AdminDashboardResponse
        {
            Summary = new AdminSummary
            {
                HealthStatus = healthStatus,
                AvgLatencyMs = FloorToTwoDecimals(latestLatency?.Avg ?? 0),
                P95LatencyMs = FloorToTwoDecimals(latestLatency?.P95 ?? 0),
                ErrorRate = FloorToTwoDecimals(errorRate),
                RequestsPerMinute = FloorToTwoDecimals(latestThroughput?.Rpm ?? 0),
                LambdaConcurrency = FloorToTwoDecimals(latestConcurrency?.Used ?? 0),
                TimeoutCount = FloorToTwoDecimals(latestTimeout?.Count ?? 0),
                DependencyLatencyMs = FloorToTwoDecimals(dependencyLatency)
            },
            Thresholds = new AdminConfiguredThresholds
            {
                WarningLatencyMs = _options.Thresholds.WarningLatencyMs,
                CriticalLatencyMs = _options.Thresholds.CriticalLatencyMs,
                WarningErrorRatePercent = _options.Thresholds.WarningErrorRatePercent,
                CriticalErrorRatePercent = _options.Thresholds.CriticalErrorRatePercent,
                WarningTimeoutCount = _options.Thresholds.WarningTimeoutCount,
                CriticalTimeoutCount = _options.Thresholds.CriticalTimeoutCount,
                WarningConcurrency = _options.Thresholds.WarningConcurrency,
                CriticalConcurrency = _options.Thresholds.CriticalConcurrency
            },
            Trends = new AdminTrendCollection
            {
                Latency = latency,
                Throughput = throughput,
                Errors = errors,
                Timeouts = timeouts,
                Concurrency = concurrency
            },
            CustomErrors = customErrors,
            VeratoErrors = veratoErrors,
            Alerts = alerts,
            LastUpdated = endTime
        };
    }

    private async Task<Dictionary<string, List<MetricSample>>> QueryCloudWatchAsync(
        DateTime startTime,
        DateTime endTime,
        CancellationToken cancellationToken)
    {
        var queryMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var queries = new List<MetricDataQuery>();

        foreach (var entry in _options.Metrics)
        {
            if (string.IsNullOrWhiteSpace(entry.Value.Namespace) || string.IsNullOrWhiteSpace(entry.Value.MetricName))
            {
                continue;
            }

            var queryId = $"m{entry.Key.ToLowerInvariant()}";
            queryMap[queryId] = entry.Key;

            queries.Add(new MetricDataQuery
            {
                Id = queryId,
                ReturnData = true,
                MetricStat = new MetricStat
                {
                    Period = _options.PeriodSeconds,
                    Stat = entry.Value.Statistic,
                    Metric = new Metric
                    {
                        Namespace = entry.Value.Namespace,
                        MetricName = entry.Value.MetricName,
                        Dimensions = entry.Value.Dimensions.Select(d => new Dimension
                        {
                            Name = d.Key,
                            Value = d.Value
                        }).ToList()
                    }
                }
            });
        }

        var request = new GetMetricDataRequest
        {
            StartTime = startTime,
            EndTime = endTime,
            ScanBy = ScanBy.TimestampAscending,
            MetricDataQueries = queries
        };

        var aggregated = new Dictionary<string, List<MetricSample>>(StringComparer.OrdinalIgnoreCase);
        GetMetricDataResponse response;
        string? nextToken = null;

        do
        {
            request.NextToken = nextToken;
            response = await cloudWatch.GetMetricDataAsync(request, cancellationToken);
            nextToken = response.NextToken;

            foreach (var result in response.MetricDataResults)
            {
                if (!queryMap.TryGetValue(result.Id, out var metricKey))
                {
                    continue;
                }

                if (!aggregated.TryGetValue(metricKey, out var samples))
                {
                    samples = [];
                    aggregated[metricKey] = samples;
                }

                for (var i = 0; i < Math.Min(result.Timestamps.Count, result.Values.Count); i++)
                {
                    var value = result.Values[i];
                    if (_options.Metrics.TryGetValue(metricKey, out var definition) && definition.NormalizeToPerMinute && _options.PeriodSeconds > 60)
                    {
                        value /= (_options.PeriodSeconds / 60d);
                    }

                    samples.Add(new MetricSample(result.Timestamps[i], value));
                }
            }
        } while (!string.IsNullOrWhiteSpace(nextToken));

        return aggregated.ToDictionary(
            entry => entry.Key,
            entry => entry.Value
                .OrderBy(item => item.Timestamp)
                .GroupBy(item => item.Timestamp)
                .Select(group => group.Last())
                .ToList(),
            StringComparer.OrdinalIgnoreCase);
    }

    private static List<LatencyTrendPoint> BuildLatencyTrend(Dictionary<string, List<MetricSample>> metricSeries)
    {
        var merged = new SortedDictionary<DateTime, LatencyTrendPoint>();

        foreach (var sample in GetMetricSamples(metricSeries, AvgLatencyKey))
        {
            merged[sample.Timestamp] = new LatencyTrendPoint
            {
                Timestamp = sample.Timestamp,
                Avg = FloorToTwoDecimals(sample.Value),
                P95 = 0
            };
        }

        foreach (var sample in GetMetricSamples(metricSeries, P95LatencyKey))
        {
            merged.TryGetValue(sample.Timestamp, out var existing);
            merged[sample.Timestamp] = new LatencyTrendPoint
            {
                Timestamp = sample.Timestamp,
                Avg = existing?.Avg ?? 0,
                P95 = FloorToTwoDecimals(sample.Value)
            };
        }

        return merged.Values.ToList();
    }

    private static List<ThroughputTrendPoint> BuildThroughputTrend(Dictionary<string, List<MetricSample>> metricSeries) =>
        GetMetricSamples(metricSeries, RequestCountKey)
            .Select(sample => new ThroughputTrendPoint
            {
                Timestamp = sample.Timestamp,
                Rpm = FloorToTwoDecimals(sample.Value)
            })
            .ToList();

    private static List<ErrorTrendPoint> BuildErrorTrend(Dictionary<string, List<MetricSample>> metricSeries)
    {
        var merged = new SortedDictionary<DateTime, ErrorTrendPoint>();

        foreach (var sample in GetMetricSamples(metricSeries, Error4xxKey))
        {
            merged[sample.Timestamp] = new ErrorTrendPoint
            {
                Timestamp = sample.Timestamp,
                FourXx = FloorToTwoDecimals(sample.Value),
                FiveXx = 0
            };
        }

        foreach (var sample in GetMetricSamples(metricSeries, Error5xxKey))
        {
            merged.TryGetValue(sample.Timestamp, out var existing);
            merged[sample.Timestamp] = new ErrorTrendPoint
            {
                Timestamp = sample.Timestamp,
                FourXx = existing?.FourXx ?? 0,
                FiveXx = FloorToTwoDecimals(sample.Value)
            };
        }

        return merged.Values.ToList();
    }

    private static List<TimeoutTrendPoint> BuildTimeoutTrend(Dictionary<string, List<MetricSample>> metricSeries) =>
        GetMetricSamples(metricSeries, TimeoutCountKey)
            .Select(sample => new TimeoutTrendPoint
            {
                Timestamp = sample.Timestamp,
                Count = FloorToTwoDecimals(sample.Value)
            })
            .ToList();

    private static List<ConcurrencyTrendPoint> BuildConcurrencyTrend(Dictionary<string, List<MetricSample>> metricSeries) =>
        GetMetricSamples(metricSeries, ConcurrencyKey)
            .Select(sample => new ConcurrencyTrendPoint
            {
                Timestamp = sample.Timestamp,
                Used = FloorToTwoDecimals(sample.Value)
            })
            .ToList();

    private static double LatestValue(Dictionary<string, List<MetricSample>> metricSeries, string key)
    {
        var samples = GetMetricSamples(metricSeries, key);
        return samples.Count == 0 ? 0 : samples[^1].Value;
    }

    private static List<MetricSample> GetMetricSamples(Dictionary<string, List<MetricSample>> metricSeries, string key) =>
        metricSeries.TryGetValue(key, out var samples) ? samples : [];

    private static T? GetLastByIndexOrDefault<T>(List<T> values) =>
        values.Count == 0 ? default : values[^1];

    private static double CalculateErrorRate(ErrorTrendPoint? latestError, ThroughputTrendPoint? latestThroughput)
    {
        if (latestError is null || latestThroughput is null || latestThroughput.Rpm <= 0)
        {
            return 0;
        }

        return FloorToTwoDecimals(((latestError.FourXx + latestError.FiveXx) / latestThroughput.Rpm) * 100);
    }

    private string DetermineHealthStatus(double p95LatencyMs, double errorRate, double timeoutCount, double concurrency)
    {
        var thresholds = _options.Thresholds;

        if (p95LatencyMs >= thresholds.CriticalLatencyMs ||
            errorRate >= thresholds.CriticalErrorRatePercent ||
            timeoutCount >= thresholds.CriticalTimeoutCount ||
            concurrency >= thresholds.CriticalConcurrency)
        {
            return "Critical";
        }

        if (p95LatencyMs >= thresholds.WarningLatencyMs ||
            errorRate >= thresholds.WarningErrorRatePercent ||
            timeoutCount >= thresholds.WarningTimeoutCount ||
            concurrency >= thresholds.WarningConcurrency)
        {
            return "Warning";
        }

        return "Healthy";
    }

    private List<AdminAlert> BuildAlerts(
        List<LatencyTrendPoint> latency,
        List<ThroughputTrendPoint> throughput,
        List<ErrorTrendPoint> errors,
        List<TimeoutTrendPoint> timeouts,
        List<ConcurrencyTrendPoint> concurrency,
        double dependencyLatencyMs)
    {
        var alerts = new List<AdminAlert>();
        var thresholds = _options.Thresholds;

        if (latency.Count >= 2)
        {
            var latest = latency[^1];
            var baseline = latency.Take(latency.Count - 1).Average(item => item.P95);
            if (baseline > 0 && latest.P95 >= baseline * 1.15)
            {
                alerts.Add(new AdminAlert
                {
                    Severity = "warning",
                    Message = $"P95 latency increased by {FloorToTwoDecimals(((latest.P95 - baseline) / baseline) * 100)}% in the latest interval."
                });
            }
        }

        var latestError = GetLastByIndexOrDefault(errors);
        var latestThroughput = GetLastByIndexOrDefault(throughput);
        var latestErrorRate = CalculateErrorRate(latestError, latestThroughput);
        if (latestErrorRate >= thresholds.CriticalErrorRatePercent)
        {
            alerts.Add(new AdminAlert
            {
                Severity = "critical",
                Message = $"Error rate reached {FloorToTwoDecimals(latestErrorRate)}% and exceeded the critical threshold."
            });
        }
        else if (latestErrorRate >= thresholds.WarningErrorRatePercent)
        {
            alerts.Add(new AdminAlert
            {
                Severity = "warning",
                Message = $"Error rate is elevated at {FloorToTwoDecimals(latestErrorRate)}%."
            });
        }

        var latestTimeoutPoint = GetLastByIndexOrDefault(timeouts);
        var latestTimeout = latestTimeoutPoint?.Count ?? 0;
        if (latestTimeout >= thresholds.WarningTimeoutCount)
        {
            alerts.Add(new AdminAlert
            {
                Severity = latestTimeout >= thresholds.CriticalTimeoutCount ? "critical" : "warning",
                Message = $"Timeout count is {FloorToTwoDecimals(latestTimeout)} in the most recent interval."
            });
        }

        var latestConcurrencyPoint = GetLastByIndexOrDefault(concurrency);
        var latestConcurrency = latestConcurrencyPoint?.Used ?? 0;
        if (latestConcurrency >= thresholds.WarningConcurrency)
        {
            alerts.Add(new AdminAlert
            {
                Severity = latestConcurrency >= thresholds.CriticalConcurrency ? "critical" : "warning",
                Message = $"Lambda concurrency reached {FloorToTwoDecimals(latestConcurrency)}, approaching configured limits."
            });
        }

        if (dependencyLatencyMs >= thresholds.WarningLatencyMs)
        {
            alerts.Add(new AdminAlert
            {
                Severity = dependencyLatencyMs >= thresholds.CriticalLatencyMs ? "critical" : "warning",
                Message = $"Downstream dependency latency is {FloorToTwoDecimals(dependencyLatencyMs)} ms."
            });
        }

        if (alerts.Count == 0)
        {
            alerts.Add(new AdminAlert
            {
                Severity = "healthy",
                Message = "No recent anomalies detected across configured operational metrics."
            });
        }

        return alerts;
    }

    private static AdminDashboardResponse BuildUnavailableDashboard(string message) =>
        new()
        {
            Summary = new AdminSummary
            {
                HealthStatus = "Warning",
                AvgLatencyMs = 0,
                P95LatencyMs = 0,
                ErrorRate = 0,
                RequestsPerMinute = 0,
                LambdaConcurrency = 0,
                TimeoutCount = 0,
                DependencyLatencyMs = 0
            },
            Thresholds = new AdminConfiguredThresholds
            {
                WarningLatencyMs = 500,
                CriticalLatencyMs = 1000,
                WarningErrorRatePercent = 1,
                CriticalErrorRatePercent = 3,
                WarningTimeoutCount = 1,
                CriticalTimeoutCount = 5,
                WarningConcurrency = 50,
                CriticalConcurrency = 80
            },
            Trends = new AdminTrendCollection
            {
                Latency = [],
                Throughput = [],
                Errors = [],
                Timeouts = [],
                Concurrency = []
            },
            CustomErrors = new AdminCustomErrorCollection
            {
                Summary = new AdminCustomErrorSummary
                {
                    TotalErrors = 0,
                    DistinctErrorCodes = 0,
                    DistinctFunctions = 0,
                    DistinctTrackingIds = 0
                },
                Entries = []
            },
            VeratoErrors = new AdminVeratoErrorCollection
            {
                Summary = new AdminVeratoErrorSummary
                {
                    TotalErrors = 0,
                    DistinctOperations = 0,
                    DistinctTrackingIds = 0,
                    TotalRetries = 0
                },
                Trend = [],
                Entries = []
            },
            Alerts =
            [
                new AdminAlert
                {
                    Severity = "warning",
                    Message = message
                }
            ],
            LastUpdated = DateTime.UtcNow
        };

    private static double FloorToTwoDecimals(double value)
    {
        return Math.Floor(value * 100) / 100;
    }

    private async Task<AdminCustomErrorCollection> QueryCustomErrorsAsync(
        DateTime startTime,
        DateTime endTime,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_options.CustomErrorLogs.LogGroupName) || string.IsNullOrWhiteSpace(_options.CustomErrorLogs.QueryString))
        {
            return EmptyCustomErrors();
        }

        try
        {
            var queryId = await StartCustomErrorQueryAsync(startTime, endTime, cancellationToken);
            var response = await WaitForCustomErrorResultsAsync(queryId, cancellationToken);
            var entries = response.Results
                .Select(MapCustomErrorEntry)
                .Where(item => item != null)
                .Cast<AdminCustomErrorEntry>()
                .OrderByDescending(item => item.Timestamp)
                .ToList();

            return new AdminCustomErrorCollection
            {
                Summary = new AdminCustomErrorSummary
                {
                    TotalErrors = entries.Count,
                    DistinctErrorCodes = entries.Select(item => item.ErrorCode).Where(item => !string.IsNullOrWhiteSpace(item)).Distinct(StringComparer.OrdinalIgnoreCase).Count(),
                    DistinctFunctions = entries.Select(item => item.FunctionName).Where(item => !string.IsNullOrWhiteSpace(item)).Distinct(StringComparer.OrdinalIgnoreCase).Count(),
                    DistinctTrackingIds = entries.Select(item => item.TrackingId).Where(item => !string.IsNullOrWhiteSpace(item)).Distinct(StringComparer.OrdinalIgnoreCase).Count()
                },
                Entries = entries
            };
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to query MPI custom error logs from CloudWatch Logs Insights.");
            return EmptyCustomErrors();
        }
    }

    private async Task<string> StartCustomErrorQueryAsync(DateTime startTime, DateTime endTime, CancellationToken cancellationToken)
    {
        var response = await cloudWatchLogs.StartQueryAsync(new StartQueryRequest
        {
            StartTime = new DateTimeOffset(startTime).ToUnixTimeSeconds(),
            EndTime = new DateTimeOffset(endTime).ToUnixTimeSeconds(),
            LogGroupNames = [_options.CustomErrorLogs.LogGroupName],
            QueryString = _options.CustomErrorLogs.QueryString,
            Limit = _options.CustomErrorLogs.Limit
        }, cancellationToken);

        return response.QueryId;
    }

    private async Task<GetQueryResultsResponse> WaitForCustomErrorResultsAsync(string queryId, CancellationToken cancellationToken)
    {
        var timeoutAt = DateTime.UtcNow.AddSeconds(Math.Max(_options.CustomErrorLogs.QueryTimeoutSeconds, 5));

        while (true)
        {
            var response = await cloudWatchLogs.GetQueryResultsAsync(new GetQueryResultsRequest
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
                    throw new System.InvalidOperationException($"Custom error query ended with status '{response.Status}'.");
            }

            if (DateTime.UtcNow >= timeoutAt)
            {
                throw new TimeoutException("Timed out waiting for custom error log query results.");
            }

            await Task.Delay(Math.Max(_options.CustomErrorLogs.PollIntervalMilliseconds, 250), cancellationToken);
        }
    }

    private static AdminCustomErrorEntry? MapCustomErrorEntry(List<ResultField> fields)
    {
        var values = fields
            .Where(field => !string.IsNullOrWhiteSpace(field.Field))
            .GroupBy(field => field.Field, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(group => group.Key, group => group.Last().Value, StringComparer.OrdinalIgnoreCase);

        if (!DateTime.TryParse(values.GetValueOrDefault("@timestamp"), out var timestamp))
        {
            return null;
        }

        return new AdminCustomErrorEntry
        {
            Timestamp = timestamp,
            Name = values.GetValueOrDefault("Name"),
            Layer = values.GetValueOrDefault("Layer"),
            FunctionName = values.GetValueOrDefault("FunctionName") ?? "Unknown",
            ErrorCode = values.GetValueOrDefault("ErrorCode") ?? "Unknown",
            Message = values.GetValueOrDefault("Message") ?? string.Empty,
            TrackingId = values.GetValueOrDefault("TrackingId"),
            LogStream = values.GetValueOrDefault("@logStream"),
            LogName = values.GetValueOrDefault("@log")
        };
    }

    private static AdminCustomErrorCollection EmptyCustomErrors() =>
        new()
        {
            Summary = new AdminCustomErrorSummary
            {
                TotalErrors = 0,
                DistinctErrorCodes = 0,
                DistinctFunctions = 0,
                DistinctTrackingIds = 0
            },
            Entries = []
        };

    private async Task<AdminVeratoErrorCollection> QueryVeratoErrorsAsync(
        DateTime startTime,
        DateTime endTime,
        int lookbackMinutes,
        CancellationToken cancellationToken)
    {
        try
        {
            var rows = await dbContext.UserRequests
                .AsNoTracking()
                .Where(item =>
                    item.RequestDateTime >= startTime &&
                    item.RequestDateTime <= endTime &&
                    EF.Functions.ILike(item.Status, "Failed") &&
                    EF.Functions.ILike(item.ApiCallType, "VE%"))
                .Select(item => new
                {
                    item.RequestDateTime,
                    item.TrackingId,
                    item.ApiCallType,
                    item.UserName,
                    item.Status,
                    item.Message,
                    item.RetryCount,
                    item.ResponseJson
                })
                .ToListAsync(cancellationToken);

            var entries = rows
                .OrderByDescending(item => item.RequestDateTime)
                .Select(item => new AdminVeratoErrorEntry
                {
                    Timestamp = item.RequestDateTime,
                    TrackingId = item.TrackingId,
                    ApiCallType = item.ApiCallType,
                    UserName = item.UserName,
                    Status = item.Status,
                    Message = item.Message,
                    RetryCount = item.RetryCount,
                    ResponseJson = item.ResponseJson
                })
                .ToList();

            var bucketMinutes = ResolveVeratoBucketMinutes(lookbackMinutes);
            var trend = entries
                .GroupBy(item => BucketTimestamp(item.Timestamp, bucketMinutes))
                .OrderBy(group => group.Key)
                .Select(group => new AdminVeratoErrorTrendPoint
                {
                    Timestamp = group.Key,
                    Count = group.Count(),
                    Label = null
                })
                .ToList();

            return new AdminVeratoErrorCollection
            {
                Summary = new AdminVeratoErrorSummary
                {
                    TotalErrors = entries.Count,
                    DistinctOperations = entries.Select(item => item.ApiCallType).Distinct(StringComparer.OrdinalIgnoreCase).Count(),
                    DistinctTrackingIds = entries.Select(item => item.TrackingId).Distinct(StringComparer.OrdinalIgnoreCase).Count(),
                    TotalRetries = entries.Sum(item => item.RetryCount)
                },
                Trend = trend,
                Entries = entries
            };
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to query Verato errors from user_requests.");
            return EmptyVeratoErrors();
        }
    }

    private static AdminVeratoErrorCollection EmptyVeratoErrors() =>
        new()
        {
            Summary = new AdminVeratoErrorSummary
            {
                TotalErrors = 0,
                DistinctOperations = 0,
                DistinctTrackingIds = 0,
                TotalRetries = 0
            },
            Trend = [],
            Entries = []
        };

    private static int ResolveVeratoBucketMinutes(int lookbackMinutes)
    {
        if (lookbackMinutes <= 60)
        {
            return 5;
        }

        if (lookbackMinutes <= 180)
        {
            return 15;
        }

        if (lookbackMinutes <= 720)
        {
            return 30;
        }

        return 60;
    }

    private static DateTime BucketTimestamp(DateTime value, int bucketMinutes)
    {
        var minuteBucket = (value.Minute / bucketMinutes) * bucketMinutes;
        return new DateTime(value.Year, value.Month, value.Day, value.Hour, minuteBucket, 0, value.Kind);
    }

    private int NormalizeLookbackMinutes(int? lookbackMinutes)
    {
        var configuredLookback = Math.Abs(_options.LookbackMinutes);
        if (lookbackMinutes is null or <= 0 )
        {
            return configuredLookback;
        }

        return Math.Abs(lookbackMinutes.Value);
    }

    private sealed record MetricSample(DateTime Timestamp, double Value);
}
