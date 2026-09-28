using Amazon.CloudWatchLogs;
using Amazon.CloudWatchLogs.Model;
using HCA.AdminMetrics.Api.Models;
using HCA.AdminMetrics.Api.Options;
using HCA.Data;
using HCA.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Net;
using System.Numerics;

namespace HCA.AdminMetrics.Api.Services;

/// <summary>
/// Provides usage metrics sourced from CloudWatch Logs and onboarded system configuration.
/// </summary>
public class AdminUsageMetricsService(
    IAmazonCloudWatchLogs cloudWatchLogs,
    HcaDbContext dbContext,
    IOptions<AdminMetricsOptions> options ) : IAdminUsageMetricsService
{
    private readonly AdminUsageMetricsOptions _options = options.Value.UsageMetrics;

    /// <inheritdoc />
    public async Task<AdminUsageMetricsResponse> GetUsageMetricsAsync(int? lookbackHours, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_options.LogGroupName) || string.IsNullOrWhiteSpace(_options.QueryString))
        {
            return new AdminUsageMetricsResponse
            {
                LookbackHours = NormalizeLookbackHours(lookbackHours),
                LastUpdated = DateTime.UtcNow,
                Message = "Usage metrics query is not configured."
            };
        }

        var effectiveLookbackHours = NormalizeLookbackHours(lookbackHours);
        var endTime = DateTime.UtcNow;
        var startTime = endTime.AddHours(-effectiveLookbackHours);

        var queryId = await StartQueryAsync(startTime, endTime, cancellationToken);
        var response = await WaitForResultsAsync(queryId, cancellationToken);
        var usageRows = response.Results.Select(MapUsageRow)
            .Where(row => !string.IsNullOrWhiteSpace(row.Ip) || !string.IsNullOrWhiteSpace(row.RawSystem))
            .ToList();
        var onboardedSystems = await dbContext.OnboardedSystem
            .AsNoTracking()
            .Where(item => item.IsActive == true)
            .OrderBy(item => item.SourceSystemName)
            .ThenBy(item => item.Id)
            .ToListAsync(cancellationToken);
        var rows = AggregateRows(usageRows, onboardedSystems);

        return new AdminUsageMetricsResponse
        {
            LookbackHours = effectiveLookbackHours,
            TotalSystems = rows.Count,
            TotalCalls = rows.Sum(row => row.CallCount),
            TotalSuccessCount = rows.Sum(row => row.SuccessCount),
            TotalFailedCount = rows.Sum(row => row.FailedCount),
            TotalDistinctSourceIps = rows.Sum(row => row.SourceIpCount),
            AverageFailureRate = FloorToTwoDecimals(rows.Count == 0 ? 0 : rows.Average(row => row.FailureRate)),
            LastUpdated = endTime,
            Systems = rows
        };
    }

    private async Task<string> StartQueryAsync(DateTime startTime, DateTime endTime, CancellationToken cancellationToken)
    {
        var request = new StartQueryRequest
        {
            StartTime = new DateTimeOffset(startTime).ToUnixTimeSeconds(),
            EndTime = new DateTimeOffset(endTime).ToUnixTimeSeconds(),
            LogGroupNames = [_options.LogGroupName],
            QueryString = _options.QueryString,
            Limit = _options.Limit
        };

        var response = await cloudWatchLogs.StartQueryAsync(request, cancellationToken);
        return response.QueryId;
    }

    private async Task<GetQueryResultsResponse> WaitForResultsAsync(string queryId, CancellationToken cancellationToken)
    {
        var timeoutAt = DateTime.UtcNow.AddSeconds(Math.Max(_options.QueryTimeoutSeconds, 5));

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
                    throw new System.InvalidOperationException($"CloudWatch Logs query ended with status '{response.Status}'.");
            }

            if (DateTime.UtcNow >= timeoutAt)
            {
                throw new TimeoutException("Timed out waiting for CloudWatch Logs Insights query results.");
            }

            await Task.Delay(Math.Max(_options.PollIntervalMilliseconds, 250), cancellationToken);
        }
    }

    private UsageMetricsLogRow MapUsageRow(List<ResultField> fields)
    {
        var values = fields
            .Where(field => !string.IsNullOrWhiteSpace(field.Field))
            .GroupBy(field => field.Field, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(group => group.Key, group => group.Last().Value, StringComparer.OrdinalIgnoreCase);

        return new UsageMetricsLogRow
        {
            RawSystem = values.GetValueOrDefault("rawSystem") ?? values.GetValueOrDefault("system") ?? "Unknown",
            Ip = values.GetValueOrDefault("ip"),
            CallCount = ParseInt(values.GetValueOrDefault("callCount")),
            FailedCount = ParseInt(values.GetValueOrDefault("failedCount")),
            SuccessCount = ParseInt(values.GetValueOrDefault("successCount")),
            FirstSeen = ParseDateTime(values.GetValueOrDefault("firstSeen")),
            LastSeen = ParseDateTime(values.GetValueOrDefault("lastSeen"))
        };
    }

    private static List<AdminUsageMetricsRow> AggregateRows(List<UsageMetricsLogRow> usageRows, List<OnboardedSystemEntity> onboardedSystems)
    {
        var aggregated = new Dictionary<string, AggregatedUsageRow>(StringComparer.OrdinalIgnoreCase);

        foreach (var row in usageRows)
        {
            var resolvedSystem = ResolveSourceSystem(row, onboardedSystems);
            var aggregateKey = BuildAggregateKey(resolvedSystem.System, resolvedSystem.Tenant);
            if (!aggregated.TryGetValue(aggregateKey, out var current))
            {
                current = new AggregatedUsageRow
                {
                    System = resolvedSystem.System,
                    Tenant = resolvedSystem.Tenant,
                };
                aggregated[aggregateKey] = current;
            }

            current.CallCount += row.CallCount;
            current.FailedCount += row.FailedCount;
            current.SuccessCount += row.SuccessCount;
            current.FirstSeen = MinTimestamp(current.FirstSeen, row.FirstSeen);
            current.LastSeen = MaxTimestamp(current.LastSeen, row.LastSeen);
            if (!string.IsNullOrWhiteSpace(row.RawSystem))
            {
                current.RawCertificateCns.Add(row.RawSystem);
            }

            if (!string.IsNullOrWhiteSpace(row.Ip))
            {
                current.SourceIps.Add(row.Ip);
            }
        }

        return aggregated.Values
            .Select(item => new AdminUsageMetricsRow
            {
                System = item.System,
                Tenant = item.Tenant,
                RawCertificateCn = string.Join(", ", item.RawCertificateCns.OrderBy(value => value, StringComparer.OrdinalIgnoreCase)),
                CallCount = item.CallCount,
                FailedCount = item.FailedCount,
                SuccessCount = item.SuccessCount,
                FailureRate = FloorToTwoDecimals(item.CallCount == 0 ? 0 : (item.FailedCount * 100.0 / item.CallCount)),
                FirstSeen = item.FirstSeen,
                LastSeen = item.LastSeen,
                SourceIpCount = item.SourceIps.Count
            })
            .OrderByDescending(item => item.CallCount)
            .ThenBy(item => item.Tenant, StringComparer.OrdinalIgnoreCase)
            .ThenBy(item => item.System, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static ResolvedUsageSystem ResolveSourceSystem(UsageMetricsLogRow row, List<OnboardedSystemEntity> onboardedSystems)
    {
        if (!string.IsNullOrWhiteSpace(row.Ip))
        {
            var match = onboardedSystems.FirstOrDefault(system => IpMatchesWhitelist(row.Ip!, system));
            if (!string.IsNullOrWhiteSpace(match?.SourceSystemName))
            {
                return new ResolvedUsageSystem
                {
                    System = match.SourceSystemName!,
                    Tenant = match.Tenant ?? string.Empty,
                };
            }
        }

        return new ResolvedUsageSystem
        {
            System = string.IsNullOrWhiteSpace(row.RawSystem) ? "Unknown" : row.RawSystem,
            Tenant = string.Empty,
        };
    }

    private static string BuildAggregateKey(string system, string tenant)
    {
        return $"{system.Trim().ToLowerInvariant()}|{tenant.Trim().ToLowerInvariant()}";
    }

    private static bool IpMatchesWhitelist(string ip, OnboardedSystemEntity system)
    {
        if (!TryParseIp(ip, out var incoming))
        {
            return false;
        }

        if (!string.IsNullOrWhiteSpace(system.StartIpAddress) && !string.IsNullOrWhiteSpace(system.EndIpAddress)
            && TryParseIp(system.StartIpAddress!, out var start)
            && TryParseIp(system.EndIpAddress!, out var end))
        {
            var incomingValue = ToBigInteger(incoming);
            var startValue = ToBigInteger(start);
            var endValue = ToBigInteger(end);
            if (incomingValue >= startValue && incomingValue <= endValue)
            {
                return true;
            }
        }

        if (!string.IsNullOrWhiteSpace(system.IpCidr) && IsInCidrBlock(incoming, system.IpCidr!))
        {
            return true;
        }

        return false;
    }

    private static bool IsInCidrBlock(IPAddress address, string cidr)
    {
        var parts = cidr.Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (parts.Length != 2
            || !IPAddress.TryParse(parts[0], out var network)
            || !int.TryParse(parts[1], out var prefixLength))
        {
            return false;
        }

        var addressBytes = address.GetAddressBytes();
        var networkBytes = network.GetAddressBytes();
        if (addressBytes.Length != networkBytes.Length)
        {
            return false;
        }

        var fullBytes = prefixLength / 8;
        var remainingBits = prefixLength % 8;

        for (var index = 0; index < fullBytes; index++)
        {
            if (addressBytes[index] != networkBytes[index])
            {
                return false;
            }
        }

        if (remainingBits == 0)
        {
            return true;
        }

        var mask = (byte)~(255 >> remainingBits);
        return (addressBytes[fullBytes] & mask) == (networkBytes[fullBytes] & mask);
    }

    private static bool TryParseIp(string value, out IPAddress address)
    {
        return IPAddress.TryParse(value?.Trim(), out address!);
    }

    private static BigInteger ToBigInteger(IPAddress address)
    {
        var bytes = address.GetAddressBytes();
        Array.Reverse(bytes);
        var unsigned = new byte[bytes.Length + 1];
        Array.Copy(bytes, unsigned, bytes.Length);
        return new BigInteger(unsigned);
    }

    private static DateTime? MinTimestamp(DateTime? left, DateTime? right)
    {
        if (!left.HasValue)
        {
            return right;
        }

        if (!right.HasValue)
        {
            return left;
        }

        return left.Value <= right.Value ? left : right;
    }

    private static DateTime? MaxTimestamp(DateTime? left, DateTime? right)
    {
        if (!left.HasValue)
        {
            return right;
        }

        if (!right.HasValue)
        {
            return left;
        }

        return left.Value >= right.Value ? left : right;
    }

    private int NormalizeLookbackHours(int? lookbackHours)
    {
        if (lookbackHours is null or <= 0 )
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

    private static double ParseDouble(string? value)
    {
        return double.TryParse(value, out var parsed) ? parsed : 0;
    }

    private static DateTime? ParseDateTime(string? value)
    {
        return DateTime.TryParse(value, out var parsed) ? parsed : null;
    }

    private static double FloorToTwoDecimals(double value)
    {
        return Math.Floor(value * 100) / 100;
    }

    private sealed class UsageMetricsLogRow
    {
        public string RawSystem { get; set; } = string.Empty;
        public string? Ip { get; set; }
        public int CallCount { get; set; }
        public int FailedCount { get; set; }
        public int SuccessCount { get; set; }
        public DateTime? FirstSeen { get; set; }
        public DateTime? LastSeen { get; set; }
    }

    private sealed class AggregatedUsageRow
    {
        public string System { get; set; } = string.Empty;
        public string Tenant { get; set; } = string.Empty;
        public int CallCount { get; set; }
        public int FailedCount { get; set; }
        public int SuccessCount { get; set; }
        public DateTime? FirstSeen { get; set; }
        public DateTime? LastSeen { get; set; }
        public HashSet<string> SourceIps { get; } = new(StringComparer.OrdinalIgnoreCase);
        public HashSet<string> RawCertificateCns { get; } = new(StringComparer.OrdinalIgnoreCase);
    }

    private sealed class ResolvedUsageSystem
    {
        public string System { get; set; } = string.Empty;
        public string Tenant { get; set; } = string.Empty;
    }
}
