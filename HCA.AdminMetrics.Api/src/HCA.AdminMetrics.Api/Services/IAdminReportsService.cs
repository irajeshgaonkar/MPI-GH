using HCA.AdminMetrics.Api.Models;

namespace HCA.AdminMetrics.Api.Services;

/// <summary>
/// Defines operations for retrieving report-oriented administration analytics.
/// </summary>
public interface IAdminReportsService
{
    /// <summary>
    /// Gets the aggregated reports dashboard payload.
    /// </summary>
    /// <param name="lookbackDays">The optional lookback window in days.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The reports dashboard response.</returns>
    Task<AdminReportsDashboardResponse> GetDashboardAsync(int? lookbackDays, CancellationToken cancellationToken);

    /// <summary>
    /// Gets the source system data quality report.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The data quality report.</returns>
    Task<SourceSystemDataQualityReport> GetSourceSystemDataQualityAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Gets the MPI linkage effectiveness report.
    /// </summary>
    /// <param name="lookbackDays">The optional lookback window in days.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The linkage effectiveness report.</returns>
    Task<MpiLinkageEffectivenessReport> GetMpiLinkageEffectivenessAsync(int? lookbackDays, CancellationToken cancellationToken);

    /// <summary>
    /// Gets the batch intake reliability report.
    /// </summary>
    /// <param name="lookbackDays">The optional lookback window in days.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The batch intake reliability report.</returns>
    Task<BatchIntakeReliabilityReport> GetBatchIntakeReliabilityAsync(int? lookbackDays, CancellationToken cancellationToken);

    /// <summary>
    /// Gets the manual stewardship analytics report.
    /// </summary>
    /// <param name="lookbackDays">The optional lookback window in days.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The manual stewardship analytics report.</returns>
    Task<ManualStewardshipAnalyticsReport> GetManualStewardshipAnalyticsAsync(int? lookbackDays, CancellationToken cancellationToken);

    /// <summary>
    /// Gets the data sharing coverage report.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The data sharing coverage report.</returns>
    Task<DataSharingCoverageReport> GetDataSharingCoverageAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Refreshes the administration materialized views.
    /// </summary>
    /// <param name="useConcurrentRefresh">A value indicating whether concurrent refresh should be used.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The materialized view refresh result.</returns>
    Task<AdminMaterializedViewRefreshResponse> RefreshMaterializedViewsAsync(bool useConcurrentRefresh, CancellationToken cancellationToken);
}
