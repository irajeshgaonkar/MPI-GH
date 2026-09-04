namespace HCA.AdminMetrics.Api.Models;

/// <summary>
/// Represents the aggregated administration reports dashboard payload.
/// </summary>
public class AdminReportsDashboardResponse
{
    /// <summary>
    /// Gets or sets the lookback window in days.
    /// </summary>
    public int LookbackDays { get; set; }

    /// <summary>
    /// Gets or sets the stale-identity threshold in days.
    /// </summary>
    public int StaleThresholdDays { get; set; }

    /// <summary>
    /// Gets or sets the UTC timestamp when the dashboard was generated.
    /// </summary>
    public DateTime LastUpdated { get; set; }

    /// <summary>
    /// Gets or sets the data quality report.
    /// </summary>
    public SourceSystemDataQualityReport DataQuality { get; set; } = new();

    /// <summary>
    /// Gets or sets the linkage effectiveness report.
    /// </summary>
    public MpiLinkageEffectivenessReport Linkage { get; set; } = new();

    /// <summary>
    /// Gets or sets the batch intake reliability report.
    /// </summary>
    public BatchIntakeReliabilityReport BatchIntake { get; set; } = new();

    /// <summary>
    /// Gets or sets the manual stewardship analytics report.
    /// </summary>
    public ManualStewardshipAnalyticsReport Stewardship { get; set; } = new();

    /// <summary>
    /// Gets or sets the data sharing coverage report.
    /// </summary>
    public DataSharingCoverageReport DataSharing { get; set; } = new();
}

/// <summary>
/// Represents the source-system data quality report.
/// </summary>
public class SourceSystemDataQualityReport
{
    /// <summary>
    /// Gets or sets the total number of sources.
    /// </summary>
    public int TotalSources { get; set; }

    /// <summary>
    /// Gets or sets the total number of active identities.
    /// </summary>
    public int TotalActiveIdentities { get; set; }

    /// <summary>
    /// Gets or sets the total number of deleted identities.
    /// </summary>
    public int TotalDeletedIdentities { get; set; }

    /// <summary>
    /// Gets or sets the quality rows grouped by source system.
    /// </summary>
    public List<SourceSystemQualityRow> Sources { get; set; } = [];
}

/// <summary>
/// Represents data quality details for a single source system.
/// </summary>
public class SourceSystemQualityRow
{
    public string SourceSystemName { get; set; } = string.Empty;
    public string Agency { get; set; } = string.Empty;
    public int TotalIdentities { get; set; }
    public int ActiveIdentities { get; set; }
    public int DeletedIdentities { get; set; }
    public double DeleteRate { get; set; }
    public int StaleIdentities { get; set; }
    public double StaleRate { get; set; }
    public double CompletenessScore { get; set; }
    public int MinimumDataSetQualifiedCount { get; set; }
    public double MinimumDataSetScore { get; set; }
    public int MissingDobCount { get; set; }
    public int MissingSsnCount { get; set; }
    public int MissingGenderCount { get; set; }
    public int MissingAddressCount { get; set; }
    public int MissingCommunicationCount { get; set; }
    public int ProtectedPopulationCount { get; set; }
    public List<ProtectedPopulationBreakdown> ProtectedPopulationBreakdown { get; set; } = [];
}

/// <summary>
/// Represents a protected population count grouped by type.
/// </summary>
public class ProtectedPopulationBreakdown
{
    public string Type { get; set; } = string.Empty;
    public int Count { get; set; }
}

/// <summary>
/// Represents the MPI linkage effectiveness report.
/// </summary>
public class MpiLinkageEffectivenessReport
{
    public int UniqueLinkIds { get; set; }
    public double AverageSourceIdentitiesPerLinkId { get; set; }
    public int MultiSourceLinkIds { get; set; }
    public List<LinkClusterRow> MultiSourceLinkDetails { get; set; } = [];
    public List<OperationTrendPoint> RecentActivity { get; set; } = [];
    public List<SourceFragmentationRow> HighestFragmentationSources { get; set; } = [];
}

/// <summary>
/// Represents a multi-source MPI link cluster.
/// </summary>
public class LinkClusterRow
{
    public string MpiLinkId { get; set; } = string.Empty;
    public int SourceIdentityCount { get; set; }
    public int DistinctSourceSystems { get; set; }
    public List<string> SourceSystems { get; set; } = [];
}

/// <summary>
/// Represents a merge and unmerge activity trend point.
/// </summary>
public class OperationTrendPoint
{
    public DateTime Date { get; set; }
    public double Merge { get; set; }
    public double Unmerge { get; set; }
}

/// <summary>
/// Represents fragmentation details for a source system.
/// </summary>
public class SourceFragmentationRow
{
    public string SourceSystemName { get; set; } = string.Empty;
    public int TotalActiveIdentities { get; set; }
    public int IdentitiesInSharedLinkIds { get; set; }
    public double FragmentationRate { get; set; }
}

/// <summary>
/// Represents the batch intake reliability report.
/// </summary>
public class BatchIntakeReliabilityReport
{
    public int TotalFiles { get; set; }
    public int FailedFiles { get; set; }
    public int RejectedRecords { get; set; }
    public double AverageProcessingMinutes { get; set; }
    public List<BatchFileRow> Files { get; set; } = [];
    public List<BatchSourceSummaryRow> SourceSummaries { get; set; } = [];
    public List<BatchErrorRow> TopErrors { get; set; } = [];
}

/// <summary>
/// Represents a processed batch file summary.
/// </summary>
public class BatchFileRow
{
    public string RequestId { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string SourceSystemName { get; set; } = string.Empty;
    public string Tenant { get; set; } = string.Empty;
    public int RecordsCount { get; set; }
    public string Status { get; set; } = string.Empty;
    public double ProcessingMinutes { get; set; }
    public int RejectCount { get; set; }
    public string TopErrorMessage { get; set; } = string.Empty;
    public DateTime RequestDateTime { get; set; }
}

/// <summary>
/// Represents a batch reliability summary grouped by source system.
/// </summary>
public class BatchSourceSummaryRow
{
    public string SourceSystemName { get; set; } = string.Empty;
    public string Tenant { get; set; } = string.Empty;
    public int TotalFiles { get; set; }
    public int FailedFiles { get; set; }
    public int RejectedRecords { get; set; }
    public double AverageProcessingMinutes { get; set; }
}

/// <summary>
/// Represents a frequent batch processing error.
/// </summary>
public class BatchErrorRow
{
    public string SourceSystemName { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public int Count { get; set; }
}

/// <summary>
/// Represents the manual stewardship analytics report.
/// </summary>
public class ManualStewardshipAnalyticsReport
{
    public int TotalQueuedRecords { get; set; }
    public int DistinctUsers { get; set; }
    public int DistinctTouchedIdentities { get; set; }
    public List<UserModifySummaryRow> TopUsers { get; set; } = [];
    public List<TouchedIdentityRow> MostTouchedIdentities { get; set; } = [];
    public List<StewardshipOutcomeRow> DownstreamOutcomes { get; set; } = [];
    public List<LinkIdOperationRow> LinkIdOperations { get; set; } = [];
}

/// <summary>
/// Represents a user-level modify queue summary.
/// </summary>
public class UserModifySummaryRow
{
    public string UserName { get; set; } = string.Empty;
    public int ModifyCount { get; set; }
}

/// <summary>
/// Represents an identity touched during stewardship.
/// </summary>
public class TouchedIdentityRow
{
    public int ClientIdentityId { get; set; }
    public string MpiLinkId { get; set; } = string.Empty;
    public string SourceSystemName { get; set; } = string.Empty;
    public string SourceSystemId { get; set; } = string.Empty;
    public int TouchCount { get; set; }
}

/// <summary>
/// Represents an aggregated stewardship outcome count.
/// </summary>
public class StewardshipOutcomeRow
{
    public string OperationType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int Count { get; set; }
}

/// <summary>
/// Represents a detailed link-id operation record.
/// </summary>
public class LinkIdOperationRow
{
    public DateTime RequestDateTime { get; set; }
    public string OperationType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string TrackingId { get; set; } = string.Empty;
    public string PrimaryMpiLinkId { get; set; } = string.Empty;
    public string SecondaryMpiLinkId { get; set; } = string.Empty;
    public string AffectedLinkIds { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}

/// <summary>
/// Represents the data sharing coverage report.
/// </summary>
public class DataSharingCoverageReport
{
    public int TotalSystems { get; set; }
    public int ActiveSystems { get; set; }
    public int ActiveMappings { get; set; }
    public int InactiveMappings { get; set; }
    public int GapCount { get; set; }
    public List<SystemReferenceRow> Systems { get; set; } = [];
    public List<DataSharingMatrixRow> Matrix { get; set; } = [];
}

/// <summary>
/// Represents a source system reference used in data sharing coverage.
/// </summary>
public class SystemReferenceRow
{
    public int Id { get; set; }
    public string SourceSystemName { get; set; } = string.Empty;
    public string AgencyName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}

/// <summary>
/// Represents a data-sharing matrix relationship between two systems.
/// </summary>
public class DataSharingMatrixRow
{
    public int SourceSystemId { get; set; }
    public string SourceSystemName { get; set; } = string.Empty;
    public int AllowedSystemId { get; set; }
    public string AllowedSystemName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string DataSharingLevel { get; set; } = string.Empty;
}
