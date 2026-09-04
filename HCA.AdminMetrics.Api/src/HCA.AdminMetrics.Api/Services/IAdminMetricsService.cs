using HCA.AdminMetrics.Api.Models;

namespace HCA.AdminMetrics.Api.Services;

/// <summary>
/// Defines operations for retrieving MPI Health dashboard metrics.
/// </summary>
public interface IAdminMetricsService
{
    /// <summary>
    /// Gets the aggregated admin dashboard payload for the requested lookback window.
    /// </summary>
    /// <param name="lookbackMinutes">The optional lookback window in minutes.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The admin dashboard response.</returns>
    Task<AdminDashboardResponse> GetDashboardAsync(int? lookbackMinutes, CancellationToken cancellationToken);
}
