using HCA.AdminMetrics.Api.Models;

namespace HCA.AdminMetrics.Api.Services;

/// <summary>
/// Defines operations for retrieving API traffic analytics from CloudWatch Logs.
/// </summary>
public interface IAdminApiTrafficService
{
    /// <summary>
    /// Gets API call counts bucketed by day.
    /// </summary>
    /// <param name="lookbackHours">Optional lookback window in hours.</param>
    /// <param name="refresh">When true, bypasses the in-memory cache.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task<AdminApiTrafficMetricResponse<AdminApiTrafficDayCount>> GetCallsPerDayAsync(
        int? lookbackHours,
        bool refresh,
        CancellationToken cancellationToken);

    /// <summary>
    /// Gets the top API endpoint paths by call volume.
    /// </summary>
    Task<AdminApiTrafficMetricResponse<AdminApiTrafficEndpointCount>> GetTopEndpointsAsync(
        int? lookbackHours,
        bool refresh,
        CancellationToken cancellationToken);

    /// <summary>
    /// Gets the busiest hour buckets over the fixed last-24-hours window.
    /// </summary>
    Task<AdminApiTrafficMetricResponse<AdminApiTrafficHourCount>> GetBusiestHoursAsync(
        bool refresh,
        CancellationToken cancellationToken);

    /// <summary>
    /// Gets the busiest day buckets over the fixed last-30-days window.
    /// </summary>
    Task<AdminApiTrafficMetricResponse<AdminApiTrafficDayCount>> GetBusiestDaysAsync(
        bool refresh,
        CancellationToken cancellationToken);

    /// <summary>
    /// Gets the busiest month buckets over the fixed last-six-months window.
    /// </summary>
    Task<AdminApiTrafficMetricResponse<AdminApiTrafficMonthCount>> GetBusiestMonthsAsync(
        bool refresh,
        CancellationToken cancellationToken);
}
