using HCA.AdminMetrics.Api.Models;

namespace HCA.AdminMetrics.Api.Services;

/// <summary>
/// Defines operations for retrieving administration usage metrics.
/// </summary>
public interface IAdminUsageMetricsService
{
    /// <summary>
    /// Gets usage metrics grouped by source system for the requested lookback window.
    /// </summary>
    /// <param name="lookbackHours">The optional lookback window in hours.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The usage metrics response.</returns>
    Task<AdminUsageMetricsResponse> GetUsageMetricsAsync(int? lookbackHours, CancellationToken cancellationToken);
}
