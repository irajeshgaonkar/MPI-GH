using HCA.AdminMetrics.Api.Constants;
using HCA.AdminMetrics.Api.Filters;
using HCA.AdminMetrics.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace HCA.AdminMetrics.Api.Controllers;

/// <summary>
/// Exposes administration endpoints for MPI Health, reporting, usage, and onboarded system management.
/// </summary>
/// <param name="adminMetricsService">The MPI Health metrics service.</param>
/// <param name="adminUsageMetricsService">The usage metrics service.</param>
/// <param name="adminReportsService">The reports service.</param>
/// <param name="adminOnboardedSystemService">The onboarded system service.</param>
[ApiController]
[Route("api/admin")]
[HcaAuthorize(Roles.Admin)]
public class AdminController(
    IAdminMetricsService adminMetricsService,
    IAdminUsageMetricsService adminUsageMetricsService,
    IAdminReportsService adminReportsService,
    IAdminOnboardedSystemService adminOnboardedSystemService ) : ControllerBase
{
    /// <summary>
    /// Gets the aggregated MPI Health dashboard payload.
    /// </summary>
    /// <param name="lookbackMinutes">The optional lookback window in minutes.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The dashboard response.</returns>
    [HttpGet("dashboard")]
    [SwaggerResponse(StatusCodes.Status200OK, "Aggregated admin metrics dashboard payload", typeof(Models.AdminDashboardResponse))]
    public async Task<IActionResult> GetDashboard([FromQuery] int? lookbackMinutes, CancellationToken cancellationToken)
    {
        return Ok(await adminMetricsService.GetDashboardAsync(lookbackMinutes, cancellationToken));
    }

    /// <summary>
    /// Gets latency trend points for the requested lookback window.
    /// </summary>
    [HttpGet("metrics/latency")]
    [SwaggerResponse(StatusCodes.Status200OK, "Latency trend", typeof(IEnumerable<Models.LatencyTrendPoint>))]
    public async Task<IActionResult> GetLatency([FromQuery] int? lookbackMinutes, CancellationToken cancellationToken)
    {
        var dashboard = await adminMetricsService.GetDashboardAsync(lookbackMinutes, cancellationToken);
        return Ok(dashboard.Trends.Latency);
    }

    /// <summary>
    /// Gets error trend points for the requested lookback window.
    /// </summary>
    [HttpGet("metrics/errors")]
    [SwaggerResponse(StatusCodes.Status200OK, "Error trend", typeof(IEnumerable<Models.ErrorTrendPoint>))]
    public async Task<IActionResult> GetErrors([FromQuery] int? lookbackMinutes, CancellationToken cancellationToken)
    {
        var dashboard = await adminMetricsService.GetDashboardAsync(lookbackMinutes, cancellationToken);
        return Ok(dashboard.Trends.Errors);
    }

    /// <summary>
    /// Gets throughput trend points for the requested lookback window.
    /// </summary>
    [HttpGet("metrics/throughput")]
    [SwaggerResponse(StatusCodes.Status200OK, "Throughput trend", typeof(IEnumerable<Models.ThroughputTrendPoint>))]
    public async Task<IActionResult> GetThroughput([FromQuery] int? lookbackMinutes, CancellationToken cancellationToken)
    {
        var dashboard = await adminMetricsService.GetDashboardAsync(lookbackMinutes, cancellationToken);
        return Ok(dashboard.Trends.Throughput);
    }

    /// <summary>
    /// Gets concurrency trend points for the requested lookback window.
    /// </summary>
    [HttpGet("metrics/concurrency")]
    [SwaggerResponse(StatusCodes.Status200OK, "Concurrency trend", typeof(IEnumerable<Models.ConcurrencyTrendPoint>))]
    public async Task<IActionResult> GetConcurrency([FromQuery] int? lookbackMinutes, CancellationToken cancellationToken)
    {
        var dashboard = await adminMetricsService.GetDashboardAsync(lookbackMinutes, cancellationToken);
        return Ok(dashboard.Trends.Concurrency);
    }

    /// <summary>
    /// Gets timeout trend points for the requested lookback window.
    /// </summary>
    [HttpGet("metrics/timeouts")]
    [SwaggerResponse(StatusCodes.Status200OK, "Timeout trend", typeof(IEnumerable<Models.TimeoutTrendPoint>))]
    public async Task<IActionResult> GetTimeouts([FromQuery] int? lookbackMinutes, CancellationToken cancellationToken)
    {
        var dashboard = await adminMetricsService.GetDashboardAsync(lookbackMinutes, cancellationToken);
        return Ok(dashboard.Trends.Timeouts);
    }

    /// <summary>
    /// Gets API usage metrics grouped by source system.
    /// </summary>
    [HttpGet("usage-metrics")]
    [SwaggerResponse(StatusCodes.Status200OK, "Usage metrics grouped by source system", typeof(Models.AdminUsageMetricsResponse))]
    public async Task<IActionResult> GetUsageMetrics([FromQuery] int? lookbackHours, CancellationToken cancellationToken)
    {
        return Ok(await adminUsageMetricsService.GetUsageMetricsAsync(lookbackHours, cancellationToken));
    }

    /// <summary>
    /// Gets the aggregated administration reports dashboard payload.
    /// </summary>
    [HttpGet("reports/dashboard")]
    [SwaggerResponse(StatusCodes.Status200OK, "Aggregated admin reports dashboard payload", typeof(Models.AdminReportsDashboardResponse))]
    public async Task<IActionResult> GetReportsDashboard([FromQuery] int? lookbackDays, CancellationToken cancellationToken)
    {
        return Ok(await adminReportsService.GetDashboardAsync(lookbackDays, cancellationToken));
    }

    /// <summary>
    /// Gets the source system data quality report.
    /// </summary>
    [HttpGet("reports/source-system-data-quality")]
    [SwaggerResponse(StatusCodes.Status200OK, "Source system data quality scorecard", typeof(Models.SourceSystemDataQualityReport))]
    public async Task<IActionResult> GetSourceSystemDataQuality(CancellationToken cancellationToken)
    {
        return Ok(await adminReportsService.GetSourceSystemDataQualityAsync(cancellationToken));
    }

    /// <summary>
    /// Gets the MPI linkage effectiveness report.
    /// </summary>
    [HttpGet("reports/mpi-linkage-effectiveness")]
    [SwaggerResponse(StatusCodes.Status200OK, "MPI linkage effectiveness report", typeof(Models.MpiLinkageEffectivenessReport))]
    public async Task<IActionResult> GetMpiLinkageEffectiveness([FromQuery] int? lookbackDays, CancellationToken cancellationToken)
    {
        return Ok(await adminReportsService.GetMpiLinkageEffectivenessAsync(lookbackDays, cancellationToken));
    }

    /// <summary>
    /// Gets the batch intake reliability report.
    /// </summary>
    [HttpGet("reports/batch-intake-reliability")]
    [SwaggerResponse(StatusCodes.Status200OK, "Batch intake reliability report", typeof(Models.BatchIntakeReliabilityReport))]
    public async Task<IActionResult> GetBatchIntakeReliability([FromQuery] int? lookbackDays, CancellationToken cancellationToken)
    {
        return Ok(await adminReportsService.GetBatchIntakeReliabilityAsync(lookbackDays, cancellationToken));
    }

    /// <summary>
    /// Gets the manual stewardship analytics report.
    /// </summary>
    [HttpGet("reports/manual-stewardship")]
    [SwaggerResponse(StatusCodes.Status200OK, "Manual stewardship workbench analytics", typeof(Models.ManualStewardshipAnalyticsReport))]
    public async Task<IActionResult> GetManualStewardshipAnalytics([FromQuery] int? lookbackDays, CancellationToken cancellationToken)
    {
        return Ok(await adminReportsService.GetManualStewardshipAnalyticsAsync(lookbackDays, cancellationToken));
    }

    /// <summary>
    /// Gets the data sharing coverage report.
    /// </summary>
    [HttpGet("reports/data-sharing-coverage")]
    [SwaggerResponse(StatusCodes.Status200OK, "Data sharing coverage map", typeof(Models.DataSharingCoverageReport))]
    public async Task<IActionResult> GetDataSharingCoverage(CancellationToken cancellationToken)
    {
        return Ok(await adminReportsService.GetDataSharingCoverageAsync(cancellationToken));
    }

    /// <summary>
    /// Refreshes the report materialized views.
    /// </summary>
    [HttpPost("reports/refresh-materialized-views")]
    [SwaggerResponse(StatusCodes.Status200OK, "Admin report materialized views refreshed", typeof(Models.AdminMaterializedViewRefreshResponse))]
    public async Task<IActionResult> RefreshMaterializedViews([FromQuery] bool useConcurrentRefresh = true, CancellationToken cancellationToken = default)
    {
        return Ok(await adminReportsService.RefreshMaterializedViewsAsync(useConcurrentRefresh, cancellationToken));
    }

    /// <summary>
    /// Gets all onboarded systems.
    /// </summary>
    [HttpGet("onboarded-systems")]
    [SwaggerResponse(StatusCodes.Status200OK, "Onboarded systems", typeof(Models.AdminOnboardedSystemListResponse))]
    public async Task<IActionResult> GetOnboardedSystems(CancellationToken cancellationToken)
    {
        return Ok(await adminOnboardedSystemService.GetAllAsync(cancellationToken));
    }

    /// <summary>
    /// Gets a single onboarded system by identifier.
    /// </summary>
    [HttpGet("onboarded-systems/{id:int}")]
    [SwaggerResponse(StatusCodes.Status200OK, "Onboarded system detail", typeof(Models.AdminOnboardedSystemDto))]
    public async Task<IActionResult> GetOnboardedSystem(int id, CancellationToken cancellationToken)
    {
        var system = await adminOnboardedSystemService.GetByIdAsync(id, cancellationToken);
        return system == null ? NotFound() : Ok(system);
    }

    /// <summary>
    /// Creates a new onboarded system.
    /// </summary>
    [HttpPost("onboarded-systems")]
    [SwaggerResponse(StatusCodes.Status200OK, "Created onboarded system", typeof(Models.AdminOnboardedSystemDto))]
    public async Task<IActionResult> CreateOnboardedSystem([FromBody] Models.AdminOnboardedSystemRequest request, CancellationToken cancellationToken)
    {
        return Ok(await adminOnboardedSystemService.CreateAsync(request, cancellationToken));
    }

    /// <summary>
    /// Updates an existing onboarded system.
    /// </summary>
    [HttpPut("onboarded-systems/{id:int}")]
    [SwaggerResponse(StatusCodes.Status200OK, "Updated onboarded system", typeof(Models.AdminOnboardedSystemDto))]
    public async Task<IActionResult> UpdateOnboardedSystem(int id, [FromBody] Models.AdminOnboardedSystemRequest request, CancellationToken cancellationToken)
    {
        return Ok(await adminOnboardedSystemService.UpdateAsync(id, request, cancellationToken));
    }

    /// <summary>
    /// Deletes an onboarded system.
    /// </summary>
    [HttpDelete("onboarded-systems/{id:int}")]
    [SwaggerResponse(StatusCodes.Status204NoContent, "Deleted onboarded system")]
    public async Task<IActionResult> DeleteOnboardedSystem(int id, CancellationToken cancellationToken)
    {
        await adminOnboardedSystemService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Updates notification settings for an onboarded system.
    /// </summary>
    [HttpPut("onboarded-systems/{id:int}/notification-settings")]
    [SwaggerResponse(StatusCodes.Status200OK, "Updated notification settings", typeof(Models.AdminOnboardedSystemDto))]
    public async Task<IActionResult> UpdateNotificationSettings(int id, [FromBody] Models.AdminUpdateNotificationSettingsRequest request, CancellationToken cancellationToken)
    {
        return Ok(await adminOnboardedSystemService.UpdateNotificationSettingsAsync(id, request.EnableNotification, cancellationToken));
    }

    /// <summary>
    /// Adds an IP whitelist entry to an onboarded system.
    /// </summary>
    [HttpPost("onboarded-systems/{id:int}/ip-whitelists")]
    [SwaggerResponse(StatusCodes.Status200OK, "Added IP whitelist entry", typeof(Models.AdminOnboardedSystemDto))]
    public async Task<IActionResult> AddIpWhitelist(int id, [FromBody] Models.AdminOnboardedSystemIpWhitelistRequest request, CancellationToken cancellationToken)
    {
        return Ok(await adminOnboardedSystemService.AddIpWhitelistAsync(id, request, cancellationToken));
    }

    /// <summary>
    /// Updates an IP whitelist entry on an onboarded system.
    /// </summary>
    [HttpPut("onboarded-systems/{id:int}/ip-whitelists/{whitelistId:int}")]
    [SwaggerResponse(StatusCodes.Status200OK, "Updated IP whitelist entry", typeof(Models.AdminOnboardedSystemDto))]
    public async Task<IActionResult> UpdateIpWhitelist(int id, int whitelistId, [FromBody] Models.AdminOnboardedSystemIpWhitelistRequest request, CancellationToken cancellationToken)
    {
        return Ok(await adminOnboardedSystemService.UpdateIpWhitelistAsync(id, whitelistId, request, cancellationToken));
    }

    /// <summary>
    /// Deletes an IP whitelist entry from an onboarded system.
    /// </summary>
    [HttpDelete("onboarded-systems/{id:int}/ip-whitelists/{whitelistId:int}")]
    [SwaggerResponse(StatusCodes.Status200OK, "Deleted IP whitelist entry", typeof(Models.AdminOnboardedSystemDto))]
    public async Task<IActionResult> DeleteIpWhitelist(int id, int whitelistId, CancellationToken cancellationToken)
    {
        return Ok(await adminOnboardedSystemService.DeleteIpWhitelistAsync(id, whitelistId, cancellationToken));
    }
}
