using System.Data;
using System.Data.Common;
using System.Text.Json;
using HCA.AdminMetrics.Api.Models;
using HCA.Data;
using HCA.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace HCA.AdminMetrics.Api.Services;

/// <summary>
/// Provides report-oriented analytics and materialized view refresh operations for administration pages.
/// </summary>
public class AdminReportsService( HcaDbContext dbContext ) : IAdminReportsService
{
    private static readonly string[] MaterializedViews =
    [
        "coalitionmpi.mv_report_source_system_quality",
        "coalitionmpi.mv_report_source_system_protected_population",
        "coalitionmpi.mv_report_mpi_link_clusters",
        "coalitionmpi.mv_report_source_fragmentation",
        "coalitionmpi.mv_report_stewardship_request_events",
        "coalitionmpi.mv_report_batch_file_summary",
        "coalitionmpi.mv_report_batch_error_events",
        "coalitionmpi.mv_report_manual_stewardship_queue",
        "coalitionmpi.mv_report_system_reference",
        "coalitionmpi.mv_report_data_sharing_coverage",
        "coalitionmpi.mv_report_link_id_ingest_trend"
    ];

    private const int DefaultLookbackDays = 30;
    private const int StaleThresholdDays = 30;
    private const int DashboardSourceLimit = 20;
    private const int DashboardClusterLimit = 10;
    private const int DashboardFragmentationLimit = 10;
    private const int DashboardFileLimit = 12;
    private const int DashboardSourceSummaryLimit = 10;
    private const int DashboardErrorLimit = 10;
    private const int DashboardUserLimit = 10;
    private const int DashboardIdentityLimit = 10;
    private const int DashboardLinkOperationLimit = 20;
    private const int DashboardSystemLimit = 12;

    private readonly HcaDbContext _dbContext = dbContext;

    /// <inheritdoc />
    public async Task<AdminReportsDashboardResponse> GetDashboardAsync(int? lookbackDays, CancellationToken cancellationToken)
    {
        var effectiveLookbackDays = NormalizeLookbackDays(lookbackDays);
        var dataQuality = await GetSourceSystemDataQualityAsync(cancellationToken);
        var linkage = await GetMpiLinkageEffectivenessAsync(effectiveLookbackDays, cancellationToken);
        var batch = await GetBatchIntakeReliabilityAsync(effectiveLookbackDays, cancellationToken);
        var stewardship = await GetManualStewardshipAnalyticsAsync(effectiveLookbackDays, cancellationToken);
        var sharing = await GetDataSharingCoverageAsync(cancellationToken);
        var sharingSystems = sharing.Systems.Take(DashboardSystemLimit).ToList();
        var sharingSystemIds = sharingSystems.Select(item => item.Id).ToHashSet();

        return new AdminReportsDashboardResponse
        {
            LookbackDays = effectiveLookbackDays,
            StaleThresholdDays = StaleThresholdDays,
            LastUpdated = DateTime.UtcNow,
            DataQuality = new SourceSystemDataQualityReport
            {
                TotalSources = dataQuality.TotalSources,
                TotalActiveIdentities = dataQuality.TotalActiveIdentities,
                TotalDeletedIdentities = dataQuality.TotalDeletedIdentities,
                Sources = dataQuality.Sources.Take(DashboardSourceLimit).ToList()
            },
            Linkage = new MpiLinkageEffectivenessReport
            {
                UniqueLinkIds = linkage.UniqueLinkIds,
                AverageSourceIdentitiesPerLinkId = linkage.AverageSourceIdentitiesPerLinkId,
                MultiSourceLinkIds = linkage.MultiSourceLinkIds,
                MultiSourceLinkDetails = linkage.MultiSourceLinkDetails.Take(DashboardClusterLimit).ToList(),
                RecentActivity = linkage.RecentActivity,
                HighestFragmentationSources = linkage.HighestFragmentationSources.Take(DashboardFragmentationLimit).ToList(),
                LinkIdIngestTrend = linkage.LinkIdIngestTrend
            },
            BatchIntake = new BatchIntakeReliabilityReport
            {
                TotalFiles = batch.TotalFiles,
                FailedFiles = batch.FailedFiles,
                RejectedRecords = batch.RejectedRecords,
                AverageProcessingMinutes = batch.AverageProcessingMinutes,
                Files = batch.Files.Take(DashboardFileLimit).ToList(),
                SourceSummaries = batch.SourceSummaries.Take(DashboardSourceSummaryLimit).ToList(),
                TopErrors = batch.TopErrors.Take(DashboardErrorLimit).ToList()
            },
            Stewardship = new ManualStewardshipAnalyticsReport
            {
                TotalQueuedRecords = stewardship.TotalQueuedRecords,
                DistinctUsers = stewardship.DistinctUsers,
                DistinctTouchedIdentities = stewardship.DistinctTouchedIdentities,
                TopUsers = stewardship.TopUsers.Take(DashboardUserLimit).ToList(),
                MostTouchedIdentities = stewardship.MostTouchedIdentities.Take(DashboardIdentityLimit).ToList(),
                DownstreamOutcomes = stewardship.DownstreamOutcomes,
                LinkIdOperations = stewardship.LinkIdOperations.Take(DashboardLinkOperationLimit).ToList()
            },
            DataSharing = new DataSharingCoverageReport
            {
                TotalSystems = sharing.TotalSystems,
                ActiveSystems = sharing.ActiveSystems,
                ActiveMappings = sharing.ActiveMappings,
                InactiveMappings = sharing.InactiveMappings,
                GapCount = sharing.GapCount,
                Systems = sharingSystems,
                Matrix = sharing.Matrix
                    .Where(item => sharingSystemIds.Contains(item.SourceSystemId) && sharingSystemIds.Contains(item.AllowedSystemId))
                    .ToList()
            }
        };
    }

    /// <inheritdoc />
    public async Task<SourceSystemDataQualityReport> GetSourceSystemDataQualityAsync(CancellationToken cancellationToken)
    {
        var qualityRows = await QueryListAsync(
            """
            SELECT
                source_system_name,
                agency,
                total_identities,
                active_identities,
                deleted_identities,
                stale_identities,
                minimum_data_set_qualified_count,
                missing_dob_count,
                missing_ssn_count,
                missing_gender_count,
                missing_address_count,
                missing_communication_count,
                protected_population_count
            FROM coalitionmpi.mv_report_source_system_quality
            ORDER BY stale_identities DESC, deleted_identities DESC, source_system_name
            """,
            reader => new MaterializedSourceSystemQualityRow
            {
                SourceSystemName = GetString(reader, "source_system_name"),
                Agency = GetString(reader, "agency"),
                TotalIdentities = GetInt32(reader, "total_identities"),
                ActiveIdentities = GetInt32(reader, "active_identities"),
                DeletedIdentities = GetInt32(reader, "deleted_identities"),
                StaleIdentities = GetInt32(reader, "stale_identities"),
                MinimumDataSetQualifiedCount = GetInt32(reader, "minimum_data_set_qualified_count"),
                MissingDobCount = GetInt32(reader, "missing_dob_count"),
                MissingSsnCount = GetInt32(reader, "missing_ssn_count"),
                MissingGenderCount = GetInt32(reader, "missing_gender_count"),
                MissingAddressCount = GetInt32(reader, "missing_address_count"),
                MissingCommunicationCount = GetInt32(reader, "missing_communication_count"),
                ProtectedPopulationCount = GetInt32(reader, "protected_population_count")
            },
            cancellationToken);

        var protectedBreakdown = await QueryListAsync(
            """
            SELECT
                source_system_name,
                agency,
                protected_population_type,
                population_count
            FROM coalitionmpi.mv_report_source_system_protected_population
            ORDER BY source_system_name, agency, population_count DESC, protected_population_type
            """,
            reader => new MaterializedProtectedPopulationRow
            {
                SourceSystemName = GetString(reader, "source_system_name"),
                Agency = GetString(reader, "agency"),
                Type = GetString(reader, "protected_population_type"),
                Count = GetInt32(reader, "population_count")
            },
            cancellationToken);

        var protectedLookup = protectedBreakdown
            .GroupBy(item => $"{item.SourceSystemName}|{item.Agency}", StringComparer.OrdinalIgnoreCase)
            .ToDictionary(
                group => group.Key,
                group => group
                    .Take(5)
                    .Select(item => new ProtectedPopulationBreakdown
                    {
                        Type = item.Type,
                        Count = item.Count
                    })
                    .ToList(),
                StringComparer.OrdinalIgnoreCase);

        var sources = qualityRows
            .Select(item =>
            {
                var completenessMeasurements = item.TotalIdentities * 5d;
                var missingMeasurements = item.MissingDobCount
                    + item.MissingSsnCount
                    + item.MissingGenderCount
                    + item.MissingAddressCount
                    + item.MissingCommunicationCount;
                var key = $"{item.SourceSystemName}|{item.Agency}";

                return new SourceSystemQualityRow
                {
                    SourceSystemName = item.SourceSystemName,
                    Agency = item.Agency,
                    TotalIdentities = item.TotalIdentities,
                    ActiveIdentities = item.ActiveIdentities,
                    DeletedIdentities = item.DeletedIdentities,
                    DeleteRate = Percent(item.DeletedIdentities, item.TotalIdentities),
                    StaleIdentities = item.StaleIdentities,
                    StaleRate = Percent(item.StaleIdentities, item.ActiveIdentities),
                    CompletenessScore = FloorToTwoDecimals(completenessMeasurements == 0
                        ? 0
                        : ((completenessMeasurements - missingMeasurements) / completenessMeasurements) * 100),
                    MinimumDataSetQualifiedCount = item.MinimumDataSetQualifiedCount,
                    MinimumDataSetScore = Percent(item.MinimumDataSetQualifiedCount, item.TotalIdentities),
                    MissingDobCount = item.MissingDobCount,
                    MissingSsnCount = item.MissingSsnCount,
                    MissingGenderCount = item.MissingGenderCount,
                    MissingAddressCount = item.MissingAddressCount,
                    MissingCommunicationCount = item.MissingCommunicationCount,
                    ProtectedPopulationCount = item.ProtectedPopulationCount,
                    ProtectedPopulationBreakdown = protectedLookup.GetValueOrDefault(key, [])
                };
            })
            .OrderByDescending(row => row.StaleRate)
            .ThenByDescending(row => row.DeleteRate)
            .ThenBy(row => row.SourceSystemName)
            .ToList();

        return new SourceSystemDataQualityReport
        {
            TotalSources = sources.Count,
            TotalActiveIdentities = sources.Sum(item => item.ActiveIdentities),
            TotalDeletedIdentities = sources.Sum(item => item.DeletedIdentities),
            Sources = sources
        };
    }

    /// <inheritdoc />
    public async Task<MpiLinkageEffectivenessReport> GetMpiLinkageEffectivenessAsync(int? lookbackDays, CancellationToken cancellationToken)
    {
        var effectiveLookbackDays = NormalizeLookbackDays(lookbackDays);
        var lookbackCutoff = DateTime.UtcNow.AddDays(-effectiveLookbackDays);

        var summary = await QuerySingleAsync(
            """
            SELECT
                COUNT(*) AS unique_link_ids,
                COALESCE(AVG(source_identity_count), 0) AS average_source_identities_per_link_id,
                COUNT(*) FILTER (WHERE distinct_source_systems > 1) AS multi_source_link_ids
            FROM coalitionmpi.mv_report_mpi_link_clusters
            """,
            reader => new
            {
                UniqueLinkIds = GetInt32(reader, "unique_link_ids"),
                AverageSourceIdentitiesPerLinkId = GetDouble(reader, "average_source_identities_per_link_id"),
                MultiSourceLinkIds = GetInt32(reader, "multi_source_link_ids")
            },
            cancellationToken) ?? new
        {
            UniqueLinkIds = 0,
            AverageSourceIdentitiesPerLinkId = 0d,
            MultiSourceLinkIds = 0
        };

        var multiSourceLinkDetails = await QueryListAsync(
            """
            SELECT
                mpi_link_id,
                source_identity_count,
                distinct_source_systems,
                source_system_names
            FROM coalitionmpi.mv_report_mpi_link_clusters
            WHERE distinct_source_systems > 1
            ORDER BY source_identity_count DESC, distinct_source_systems DESC, mpi_link_id
            LIMIT 25
            """,
            reader => new LinkClusterRow
            {
                MpiLinkId = GetString(reader, "mpi_link_id"),
                SourceIdentityCount = GetInt32(reader, "source_identity_count"),
                DistinctSourceSystems = GetInt32(reader, "distinct_source_systems"),
                SourceSystems = SplitPipeDelimited(GetString(reader, "source_system_names"))
            },
            cancellationToken);

        var recentActivity = await QueryListAsync(
            """
            SELECT
                request_date,
                COUNT(*) FILTER (WHERE operation_type = 'merge') AS merge_count,
                COUNT(*) FILTER (WHERE operation_type = 'unmerge') AS unmerge_count
            FROM coalitionmpi.mv_report_stewardship_request_events
            WHERE request_date_time >= @cutoff
              AND operation_type IN ('merge', 'unmerge')
            GROUP BY request_date
            ORDER BY request_date
            """,
            reader => new OperationTrendPoint
            {
                Date = GetDateTime(reader, "request_date"),
                Merge = FloorToTwoDecimals(GetDouble(reader, "merge_count")),
                Unmerge = FloorToTwoDecimals(GetDouble(reader, "unmerge_count"))
            },
            cancellationToken,
            ("@cutoff", lookbackCutoff));

        var fragmentation = await QueryListAsync(
            """
            SELECT
                source_system_name,
                total_active_identities,
                identities_in_shared_link_ids,
                fragmentation_rate
            FROM coalitionmpi.mv_report_source_fragmentation
            ORDER BY fragmentation_rate DESC, identities_in_shared_link_ids DESC, source_system_name
            LIMIT 10
            """,
            reader => new SourceFragmentationRow
            {
                SourceSystemName = GetString(reader, "source_system_name"),
                TotalActiveIdentities = GetInt32(reader, "total_active_identities"),
                IdentitiesInSharedLinkIds = GetInt32(reader, "identities_in_shared_link_ids"),
                FragmentationRate = GetDouble(reader, "fragmentation_rate")
            },
            cancellationToken);

        var linkIdIngestTrend = await QueryListAsync(
            """
            SELECT
                ingest_date,
                source_system_name,
                incoming_records,
                new_person_records,
                already_in_mpi_records
            FROM coalitionmpi.mv_report_link_id_ingest_trend
            WHERE ingest_date >= @cutoff::date
            ORDER BY ingest_date, source_system_name
            """,
            reader => new LinkIdIngestTrendRow
            {
                Date = GetDateTime(reader, "ingest_date"),
                SourceSystemName = GetString(reader, "source_system_name"),
                IncomingRecords = GetInt32(reader, "incoming_records"),
                NewPersonRecords = GetInt32(reader, "new_person_records"),
                AlreadyInMpiRecords = GetInt32(reader, "already_in_mpi_records")
            },
            cancellationToken,
            ("@cutoff", lookbackCutoff));

        return new MpiLinkageEffectivenessReport
        {
            UniqueLinkIds = summary.UniqueLinkIds,
            AverageSourceIdentitiesPerLinkId = FloorToTwoDecimals(summary.AverageSourceIdentitiesPerLinkId),
            MultiSourceLinkIds = summary.MultiSourceLinkIds,
            MultiSourceLinkDetails = multiSourceLinkDetails,
            RecentActivity = recentActivity,
            HighestFragmentationSources = fragmentation,
            LinkIdIngestTrend = linkIdIngestTrend
        };
    }

    /// <inheritdoc />
    public async Task<BatchIntakeReliabilityReport> GetBatchIntakeReliabilityAsync(int? lookbackDays, CancellationToken cancellationToken)
    {
        var effectiveLookbackDays = NormalizeLookbackDays(lookbackDays);
        var lookbackCutoff = DateTime.UtcNow.AddDays(-effectiveLookbackDays);
        var onboardedSystems = await _dbContext.OnboardedSystem
            .AsNoTracking()
            .Where(item => item.IsActive == true)
            .OrderBy(item => item.SourceSystemName)
            .ThenBy(item => item.Tenant)
            .ThenBy(item => item.Id)
            .ToListAsync(cancellationToken);

        var summary = await QuerySingleAsync(
            """
            SELECT
                COUNT(*) AS total_files,
                COUNT(*) FILTER (WHERE normalized_status NOT IN ('succeeded', 'success', 'completed')) AS failed_files,
                COALESCE(SUM(reject_count), 0) AS rejected_records,
                COALESCE(AVG(processing_minutes), 0) AS average_processing_minutes
            FROM coalitionmpi.mv_report_batch_file_summary
            WHERE request_date_time >= @cutoff
            """,
            reader => new
            {
                TotalFiles = GetInt32(reader, "total_files"),
                FailedFiles = GetInt32(reader, "failed_files"),
                RejectedRecords = GetInt32(reader, "rejected_records"),
                AverageProcessingMinutes = GetDouble(reader, "average_processing_minutes")
            },
            cancellationToken,
            ("@cutoff", lookbackCutoff)) ?? new
        {
            TotalFiles = 0,
            FailedFiles = 0,
            RejectedRecords = 0,
            AverageProcessingMinutes = 0d
        };

        var files = await QueryListAsync(
            """
            SELECT
                request_id,
                file_name,
                source_system_name,
                records_count,
                status,
                processing_minutes,
                reject_count,
                top_error_message,
                request_date_time
            FROM coalitionmpi.mv_report_batch_file_summary
            WHERE request_date_time >= @cutoff
            ORDER BY request_date_time DESC
            LIMIT 12
            """,
            reader => new BatchFileRow
            {
                RequestId = GetString(reader, "request_id"),
                FileName = GetString(reader, "file_name"),
                SourceSystemName = GetString(reader, "source_system_name"),
                Tenant = string.Empty,
                RecordsCount = GetInt32(reader, "records_count"),
                Status = GetString(reader, "status"),
                ProcessingMinutes = GetDouble(reader, "processing_minutes"),
                RejectCount = GetInt32(reader, "reject_count"),
                TopErrorMessage = GetString(reader, "top_error_message"),
                RequestDateTime = GetDateTime(reader, "request_date_time")
            },
            cancellationToken,
            ("@cutoff", lookbackCutoff));

        var sourceSummaries = await QueryListAsync(
            """
            SELECT
                source_system_name,
                COUNT(*) AS total_files,
                COUNT(*) FILTER (WHERE normalized_status NOT IN ('succeeded', 'success', 'completed')) AS failed_files,
                COALESCE(SUM(reject_count), 0) AS rejected_records,
                COALESCE(AVG(processing_minutes), 0) AS average_processing_minutes
            FROM coalitionmpi.mv_report_batch_file_summary
            WHERE request_date_time >= @cutoff
            GROUP BY source_system_name
            ORDER BY failed_files DESC, rejected_records DESC, source_system_name
            LIMIT 10
            """,
            reader => new BatchSourceSummaryRow
            {
                SourceSystemName = GetString(reader, "source_system_name"),
                Tenant = string.Empty,
                TotalFiles = GetInt32(reader, "total_files"),
                FailedFiles = GetInt32(reader, "failed_files"),
                RejectedRecords = GetInt32(reader, "rejected_records"),
                AverageProcessingMinutes = FloorToTwoDecimals(GetDouble(reader, "average_processing_minutes"))
            },
            cancellationToken,
            ("@cutoff", lookbackCutoff));

        foreach (var file in files)
        {
            file.Tenant = ResolveTenantForSourceSystem(file.SourceSystemName, onboardedSystems);
        }

        foreach (var sourceSummary in sourceSummaries)
        {
            sourceSummary.Tenant = ResolveTenantForSourceSystem(sourceSummary.SourceSystemName, onboardedSystems);
        }

        var topErrors = await QueryListAsync(
            """
            SELECT
                source_system_name,
                message,
                COUNT(*) AS error_count
            FROM coalitionmpi.mv_report_batch_error_events
            WHERE request_date_time >= @cutoff
            GROUP BY source_system_name, message
            ORDER BY error_count DESC, source_system_name, message
            LIMIT 10
            """,
            reader => new BatchErrorRow
            {
                SourceSystemName = GetString(reader, "source_system_name"),
                Message = GetString(reader, "message"),
                Count = GetInt32(reader, "error_count")
            },
            cancellationToken,
            ("@cutoff", lookbackCutoff));

        return new BatchIntakeReliabilityReport
        {
            TotalFiles = summary.TotalFiles,
            FailedFiles = summary.FailedFiles,
            RejectedRecords = summary.RejectedRecords,
            AverageProcessingMinutes = FloorToTwoDecimals(summary.AverageProcessingMinutes),
            Files = files,
            SourceSummaries = sourceSummaries,
            TopErrors = topErrors
        };
    }

    private static string ResolveTenantForSourceSystem(string? sourceSystemName, List<OnboardedSystemEntity> onboardedSystems)
    {
        if (string.IsNullOrWhiteSpace(sourceSystemName))
        {
            return string.Empty;
        }

        var tenants = onboardedSystems
            .Where(item => string.Equals(item.SourceSystemName, sourceSystemName.Trim(), StringComparison.OrdinalIgnoreCase))
            .Select(item => item.Tenant?.Trim())
            .Where(item => !string.IsNullOrWhiteSpace(item))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(item => item, StringComparer.OrdinalIgnoreCase)
            .ToList();

        return tenants.Count == 0 ? string.Empty : string.Join(", ", tenants);
    }

    /// <inheritdoc />
    public async Task<ManualStewardshipAnalyticsReport> GetManualStewardshipAnalyticsAsync(int? lookbackDays, CancellationToken cancellationToken)
    {
        var effectiveLookbackDays = NormalizeLookbackDays(lookbackDays);
        var lookbackCutoff = DateTime.UtcNow.AddDays(-effectiveLookbackDays);

        var queueSummary = await QuerySingleAsync(
            """
            SELECT
                COUNT(*) AS total_queued_records,
                COUNT(DISTINCT user_name) AS distinct_users,
                COUNT(DISTINCT client_identity_id) AS distinct_touched_identities
            FROM coalitionmpi.mv_report_manual_stewardship_queue
            """,
            reader => new
            {
                TotalQueuedRecords = GetInt32(reader, "total_queued_records"),
                DistinctUsers = GetInt32(reader, "distinct_users"),
                DistinctTouchedIdentities = GetInt32(reader, "distinct_touched_identities")
            },
            cancellationToken) ?? new
        {
            TotalQueuedRecords = 0,
            DistinctUsers = 0,
            DistinctTouchedIdentities = 0
        };

        var topUsers = await QueryListAsync(
            """
            SELECT
                user_name,
                COUNT(*) AS modify_count
            FROM coalitionmpi.mv_report_manual_stewardship_queue
            GROUP BY user_name
            ORDER BY modify_count DESC, user_name
            LIMIT 10
            """,
            reader => new UserModifySummaryRow
            {
                UserName = GetString(reader, "user_name"),
                ModifyCount = GetInt32(reader, "modify_count")
            },
            cancellationToken);

        var touchedIdentities = await QueryListAsync(
            """
            SELECT
                client_identity_id,
                mpi_link_id,
                source_system_name,
                source_system_id,
                COUNT(*) AS touch_count
            FROM coalitionmpi.mv_report_manual_stewardship_queue
            GROUP BY client_identity_id, mpi_link_id, source_system_name, source_system_id
            ORDER BY touch_count DESC, source_system_name, source_system_id
            LIMIT 10
            """,
            reader => new TouchedIdentityRow
            {
                ClientIdentityId = GetInt32(reader, "client_identity_id"),
                MpiLinkId = GetString(reader, "mpi_link_id"),
                SourceSystemName = GetString(reader, "source_system_name"),
                SourceSystemId = GetString(reader, "source_system_id"),
                TouchCount = GetInt32(reader, "touch_count")
            },
            cancellationToken);

        var outcomes = await QueryListAsync(
            """
            SELECT
                operation_type,
                normalized_status,
                COUNT(*) AS outcome_count
            FROM coalitionmpi.mv_report_stewardship_request_events
            WHERE request_date_time >= @cutoff
              AND operation_type IN ('link', 'unlink', 'merge', 'unmerge', 'delete')
            GROUP BY operation_type, normalized_status
            ORDER BY operation_type, outcome_count DESC
            """,
            reader => new StewardshipOutcomeRow
            {
                OperationType = GetString(reader, "operation_type"),
                Status = GetString(reader, "normalized_status"),
                Count = GetInt32(reader, "outcome_count")
            },
            cancellationToken,
            ("@cutoff", lookbackCutoff));

        var linkIdOperationRequests = await QueryListAsync(
            """
            SELECT
                request_date_time,
                tracking_id,
                LOWER(BTRIM(COALESCE(status, 'unknown'))) AS normalized_status,
                CASE
                    WHEN LOWER(BTRIM(COALESCE(api_call_type, ''))) LIKE '%unmerge%' THEN 'unmerge'
                    WHEN LOWER(BTRIM(COALESCE(api_call_type, ''))) LIKE '%unlink%' THEN 'unlink'
                    WHEN LOWER(BTRIM(COALESCE(api_call_type, ''))) LIKE '%merge%' THEN 'merge'
                    WHEN LOWER(BTRIM(COALESCE(api_call_type, ''))) LIKE '%link%' THEN 'link'
                    WHEN LOWER(BTRIM(COALESCE(api_call_type, ''))) LIKE '%delete%' THEN 'delete'
                    ELSE LOWER(BTRIM(COALESCE(api_call_type, '')))
                END AS operation_type,
                message,
                response_json
            FROM coalitionmpi.user_requests
            WHERE request_date_time >= @cutoff
              AND (
                    LOWER(BTRIM(COALESCE(api_call_type, ''))) LIKE '%link%'
                 OR LOWER(BTRIM(COALESCE(api_call_type, ''))) LIKE '%unlink%'
                 OR LOWER(BTRIM(COALESCE(api_call_type, ''))) LIKE '%merge%'
                 OR LOWER(BTRIM(COALESCE(api_call_type, ''))) LIKE '%unmerge%'
                 OR LOWER(BTRIM(COALESCE(api_call_type, ''))) LIKE '%delete%'
              )
            ORDER BY request_date_time DESC
            LIMIT 100
            """,
            reader => new MaterializedLinkIdOperationRequest
            {
                RequestDateTime = GetDateTime(reader, "request_date_time"),
                TrackingId = GetString(reader, "tracking_id"),
                Status = GetString(reader, "normalized_status"),
                OperationType = GetString(reader, "operation_type"),
                Message = GetString(reader, "message"),
                ResponseJson = GetString(reader, "response_json")
            },
            cancellationToken,
            ("@cutoff", lookbackCutoff));

        var linkIdOperations = linkIdOperationRequests
            .Select(BuildLinkIdOperationRow)
            .Where(item => item is not null)
            .Cast<LinkIdOperationRow>()
            .OrderByDescending(item => item.RequestDateTime)
            .ToList();

        return new ManualStewardshipAnalyticsReport
        {
            TotalQueuedRecords = queueSummary.TotalQueuedRecords,
            DistinctUsers = queueSummary.DistinctUsers,
            DistinctTouchedIdentities = queueSummary.DistinctTouchedIdentities,
            TopUsers = topUsers,
            MostTouchedIdentities = touchedIdentities,
            DownstreamOutcomes = outcomes,
            LinkIdOperations = linkIdOperations
        };
    }

    /// <inheritdoc />
    public async Task<DataSharingCoverageReport> GetDataSharingCoverageAsync(CancellationToken cancellationToken)
    {
        var systems = await QueryListAsync(
            """
            SELECT
                id,
                source_system_name,
                agency_name,
                is_active
            FROM coalitionmpi.mv_report_system_reference
            ORDER BY source_system_name
            """,
            reader => new SystemReferenceRow
            {
                Id = GetInt32(reader, "id"),
                SourceSystemName = GetString(reader, "source_system_name"),
                AgencyName = GetString(reader, "agency_name"),
                IsActive = GetBoolean(reader, "is_active")
            },
            cancellationToken);

        var matrix = await QueryListAsync(
            """
            SELECT
                source_system_id,
                source_system_name,
                allowed_system_id,
                allowed_system_name,
                status,
                data_sharing_level
            FROM coalitionmpi.mv_report_data_sharing_coverage
            ORDER BY source_system_name, allowed_system_name
            """,
            reader => new DataSharingMatrixRow
            {
                SourceSystemId = GetInt32(reader, "source_system_id"),
                SourceSystemName = GetString(reader, "source_system_name"),
                AllowedSystemId = GetInt32(reader, "allowed_system_id"),
                AllowedSystemName = GetString(reader, "allowed_system_name"),
                Status = GetString(reader, "status"),
                DataSharingLevel = GetString(reader, "data_sharing_level")
            },
            cancellationToken);

        return new DataSharingCoverageReport
        {
            TotalSystems = systems.Count,
            ActiveSystems = systems.Count(system => system.IsActive),
            ActiveMappings = matrix.Count(item => item.Status == "active"),
            InactiveMappings = matrix.Count(item => item.Status == "inactive"),
            GapCount = matrix.Count(item => item.Status == "gap"),
            Systems = systems,
            Matrix = matrix
        };
    }

    /// <inheritdoc />
    public async Task<AdminMaterializedViewRefreshResponse> RefreshMaterializedViewsAsync(bool useConcurrentRefresh, CancellationToken cancellationToken)
    {
        var startedAtUtc = DateTime.UtcNow;
        var startedAt = DateTimeOffset.UtcNow;
        var refreshedViews = new List<string>(MaterializedViews.Length);

        await using var connection = _dbContext.Database.GetDbConnection();
        if (connection.State != ConnectionState.Open)
        {
            await connection.OpenAsync(cancellationToken);
        }

        foreach (var viewName in MaterializedViews)
        {
            await using var command = connection.CreateCommand();
            command.CommandText = useConcurrentRefresh
                ? $"REFRESH MATERIALIZED VIEW CONCURRENTLY {viewName};"
                : $"REFRESH MATERIALIZED VIEW {viewName};";
            command.CommandType = CommandType.Text;
            command.CommandTimeout = 0;

            await command.ExecuteNonQueryAsync(cancellationToken);
            refreshedViews.Add(viewName);
        }

        var completedAtUtc = DateTime.UtcNow;
        var completedAt = DateTimeOffset.UtcNow;

        return new AdminMaterializedViewRefreshResponse
        {
            Success = true,
            UsedConcurrentRefresh = useConcurrentRefresh,
            StartedAtUtc = startedAtUtc,
            CompletedAtUtc = completedAtUtc,
            DurationMilliseconds = (long)(completedAt - startedAt).TotalMilliseconds,
            RefreshedViews = refreshedViews
        };
    }

    private async Task<List<T>> QueryListAsync<T>(
        string sql,
        Func<DbDataReader, T> map,
        CancellationToken cancellationToken,
        params (string Name, object? Value)[] parameters)
    {
        var connection = _dbContext.Database.GetDbConnection();
        var shouldClose = connection.State != ConnectionState.Open;

        if (shouldClose)
        {
            await connection.OpenAsync(cancellationToken);
        }

        try
        {
            using var command = BuildCommand(connection, sql, parameters);
            using var reader = await command.ExecuteReaderAsync(cancellationToken);
            var results = new List<T>();

            while (await reader.ReadAsync(cancellationToken))
            {
                results.Add(map(reader));
            }

            return results;
        }
        finally
        {
            if (shouldClose)
            {
                await connection.CloseAsync();
            }
        }
    }

    private async Task<T?> QuerySingleAsync<T>(
        string sql,
        Func<DbDataReader, T> map,
        CancellationToken cancellationToken,
        params (string Name, object? Value)[] parameters)
    {
        var results = await QueryListAsync(sql, map, cancellationToken, parameters);
        return results.Count == 0 ? default : results[0];
    }

    private static DbCommand BuildCommand(DbConnection connection, string sql, params (string Name, object? Value)[] parameters)
    {
        var command = connection.CreateCommand();
        command.CommandText = sql;

        foreach (var (name, value) in parameters)
        {
            var parameter = command.CreateParameter();
            parameter.ParameterName = name;
            parameter.Value = value ?? DBNull.Value;
            command.Parameters.Add(parameter);
        }

        return command;
    }

    private static string GetString(DbDataReader reader, string columnName)
    {
        var ordinal = reader.GetOrdinal(columnName);
        return reader.IsDBNull(ordinal) ? string.Empty : reader.GetString(ordinal);
    }

    private static int GetInt32(DbDataReader reader, string columnName)
    {
        var ordinal = reader.GetOrdinal(columnName);
        if (reader.IsDBNull(ordinal))
        {
            return 0;
        }

        return Convert.ToInt32(reader.GetValue(ordinal));
    }

    private static double GetDouble(DbDataReader reader, string columnName)
    {
        var ordinal = reader.GetOrdinal(columnName);
        if (reader.IsDBNull(ordinal))
        {
            return 0;
        }

        return Convert.ToDouble(reader.GetValue(ordinal));
    }

    private static bool GetBoolean(DbDataReader reader, string columnName)
    {
        var ordinal = reader.GetOrdinal(columnName);
        return !reader.IsDBNull(ordinal) && Convert.ToBoolean(reader.GetValue(ordinal));
    }

    private static DateTime GetDateTime(DbDataReader reader, string columnName)
    {
        var ordinal = reader.GetOrdinal(columnName);
        if (reader.IsDBNull(ordinal))
        {
            return DateTime.MinValue;
        }

        var value = reader.GetValue(ordinal);
        return value switch
        {
            DateTime dateTime => dateTime,
            DateOnly dateOnly => dateOnly.ToDateTime(TimeOnly.MinValue),
            _ => Convert.ToDateTime(value)
        };
    }

    private static List<string> SplitPipeDelimited(string value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? []
            : value.Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();
    }

    private static double Percent(int numerator, int denominator)
    {
        if (denominator <= 0)
        {
            return 0;
        }

        return FloorToTwoDecimals((double)numerator / denominator * 100);
    }

    private static double FloorToTwoDecimals(double value)
    {
        return Math.Floor(value * 100) / 100;
    }

    private static int NormalizeLookbackDays(int? lookbackDays)
    {
        if (lookbackDays is null or <= 0 )
        {
            return DefaultLookbackDays;
        }

        return Math.Abs(lookbackDays.Value);
    }

    private static LinkIdOperationRow? BuildLinkIdOperationRow(MaterializedLinkIdOperationRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.ResponseJson))
        {
            return null;
        }

        try
        {
            using var document = JsonDocument.Parse(request.ResponseJson);
            if (!document.RootElement.TryGetProperty("Content", out var content) || content.ValueKind != JsonValueKind.Object)
            {
                return null;
            }

            var row = new LinkIdOperationRow
            {
                RequestDateTime = request.RequestDateTime,
                OperationType = request.OperationType,
                Status = request.Status,
                TrackingId = request.TrackingId,
                Message = request.Message
            };

            switch (request.OperationType)
            {
                case "link":
                case "merge":
                    row.PrimaryMpiLinkId = GetJsonString(content, "LinkId");
                    break;
                case "unlink":
                    row.PrimaryMpiLinkId = GetJsonString(content, "UnlinkedId");
                    row.SecondaryMpiLinkId = GetJsonString(content, "UnlinkedFromId");
                    break;
                case "unmerge":
                    row.PrimaryMpiLinkId = GetJsonString(content, "UnmergedId");
                    row.SecondaryMpiLinkId = GetJsonString(content, "UnmergedFromId");
                    break;
                case "delete":
                    var deleted = GetJsonStringArray(content, "LinkIdsDeleted");
                    var modified = GetJsonStringArray(content, "LinkIdsModified");
                    row.PrimaryMpiLinkId = deleted.Count == 0 ? string.Empty : deleted[0];
                    row.SecondaryMpiLinkId = modified.Count == 0 ? string.Empty : modified[0];
                    row.AffectedLinkIds = string.Join(", ", deleted.Concat(modified).Distinct(StringComparer.OrdinalIgnoreCase));
                    break;
            }

            if (string.IsNullOrWhiteSpace(row.AffectedLinkIds))
            {
                row.AffectedLinkIds = string.Join(", ",
                    new[] { row.PrimaryMpiLinkId, row.SecondaryMpiLinkId }
                        .Where(value => !string.IsNullOrWhiteSpace(value))
                        .Distinct(StringComparer.OrdinalIgnoreCase));
            }

            return string.IsNullOrWhiteSpace(row.AffectedLinkIds) ? null : row;
        }
        catch
        {
            return null;
        }
    }

    private static string GetJsonString(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out var property) || property.ValueKind == JsonValueKind.Null)
        {
            return string.Empty;
        }

        return property.ValueKind == JsonValueKind.String
            ? property.GetString() ?? string.Empty
            : property.ToString();
    }

    private static List<string> GetJsonStringArray(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out var property) || property.ValueKind != JsonValueKind.Array)
        {
            return [];
        }

        return property
            .EnumerateArray()
            .Select(item => item.ValueKind == JsonValueKind.String ? item.GetString() ?? string.Empty : item.ToString())
            .Where(item => !string.IsNullOrWhiteSpace(item))
            .ToList();
    }

    private sealed class MaterializedSourceSystemQualityRow
    {
        public string SourceSystemName { get; set; } = string.Empty;
        public string Agency { get; set; } = string.Empty;
        public int TotalIdentities { get; set; }
        public int ActiveIdentities { get; set; }
        public int DeletedIdentities { get; set; }
        public int StaleIdentities { get; set; }
        public int MinimumDataSetQualifiedCount { get; set; }
        public int MissingDobCount { get; set; }
        public int MissingSsnCount { get; set; }
        public int MissingGenderCount { get; set; }
        public int MissingAddressCount { get; set; }
        public int MissingCommunicationCount { get; set; }
        public int ProtectedPopulationCount { get; set; }
    }

    private sealed class MaterializedProtectedPopulationRow
    {
        public string SourceSystemName { get; set; } = string.Empty;
        public string Agency { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public int Count { get; set; }
    }

    private sealed class MaterializedLinkIdOperationRequest
    {
        public DateTime RequestDateTime { get; set; }
        public string TrackingId { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string OperationType { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string ResponseJson { get; set; } = string.Empty;
    }
}
