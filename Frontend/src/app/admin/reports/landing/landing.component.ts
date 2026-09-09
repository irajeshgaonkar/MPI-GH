import { Component, OnInit } from "@angular/core";
import jsPDF from "jspdf";
import autoTable from "jspdf-autotable";
import {
  AdminReportsDashboardResponse,
  BatchErrorRow,
  BatchFileRow,
  BatchSourceSummaryRow,
  CommonApiService,
  DataSharingMatrixRow,
  LinkIdOperationRow,
  LinkClusterRow,
  SourceSystemQualityRow,
  SourceFragmentationRow,
  StewardshipOutcomeRow,
  SystemReferenceRow,
  TouchedIdentityRow,
  UserModifySummaryRow,
} from "src/app/services/common-api.service";
import { SharedService } from "src/app/services/sharedService";
import { MetricChartSeries } from "src/app/shared/metric-chart/metric-chart.component";

type SortDirection = "asc" | "desc";

interface SortState {
  key: string;
  direction: SortDirection;
}

interface ReportOverviewCard {
  label: string;
  value: string;
  detail: string;
  tooltip: string;
}

interface DataSharingCoverageSummaryRow {
  sourceSystemId: number;
  sourceSystemName: string;
  agencyName: string;
  activeMappings: number;
  inactiveMappings: number;
  gapCount: number;
  totalTargets: number;
}

interface DataSharingRelationshipRow {
  allowedSystemId: number;
  allowedSystemName: string;
  agencyName: string;
  status: string;
  dataSharingLevel: string;
}

@Component({
  selector: "reports-landing",
  templateUrl: "./landing.component.html",
  styleUrls: ["./landing.component.scss"],
})
export class ReportsLandingComponent implements OnInit {
  readonly lookbackOptions = [
    { label: "7 days", value: 7 },
    { label: "30 days", value: 30 },
    { label: "90 days", value: 90 },
    { label: "180 days", value: 180 },
  ];

  readonly linkageSeries: MetricChartSeries[] = [
    { key: "merge", label: "Merge", color: "#0b84f3" },
    { key: "unmerge", label: "Unmerge", color: "#ff7f50" },
  ];

  loading = false;
  refreshing = false;
  exporting = false;
  loadError = "";
  selectedLookbackDays = 30;
  dataQualitySearch = "";
  linkageSearch = "";
  batchSearch = "";
  stewardshipSearch = "";
  dataSharingSearch = "";
  selectedCoverageSourceSystemId: number | null = null;
  selectedCoverageStatusFilter = "all";
  dashboard: AdminReportsDashboardResponse = this.buildEmptyDashboard();
  sortStates: Record<string, SortState> = {
    dataQuality: { key: "sourceSystemName", direction: "asc" },
    topClusters: { key: "sourceIdentityCount", direction: "desc" },
    fragmentation: { key: "fragmentationRate", direction: "desc" },
    files: { key: "requestDateTime", direction: "desc" },
    sourceReliability: { key: "failedFiles", direction: "desc" },
    topErrors: { key: "count", direction: "desc" },
    topUsers: { key: "modifyCount", direction: "desc" },
    touchedIdentities: { key: "touchCount", direction: "desc" },
    outcomes: { key: "count", direction: "desc" },
    linkIdOperations: { key: "requestDateTime", direction: "desc" },
    onboardedSystems: { key: "sourceSystemName", direction: "asc" },
    coverageSummary: { key: "gapCount", direction: "desc" },
    coverageExplorer: { key: "allowedSystemName", direction: "asc" },
  };
  readonly reportTooltips: Record<string, string> = {
    dataQuality: "Shows source-by-source data quality so leaders can see which agencies or systems are contributing stale, incomplete, deleted, or protected-population-heavy records.",
    linkage: "Shows MPI link IDs that span multiple source systems, along with fragmentation and recent merge or unmerge activity that may signal identity quality issues.",
    batchIntake: "Shows how reliably inbound files are processed, including failures, rejects, processing speed, and recurring data issues.",
    stewardship: "Shows the volume and concentration of manual review work, plus the link-ID-level operations executed after review such as link, unlink, merge, unmerge, or delete.",
    dataSharing: "Shows whether onboarded systems have the expected sharing relationships configured and where active gaps remain."
  };
  readonly metricTooltips: Record<string, string> = {
    sourcesScored: "The number of source systems currently represented in the data quality scorecard.",
    uniqueLinkIds: "The number of active MPI person clusters currently represented in the system.",
    filesInWindow: "The number of inbound files processed during the selected reporting window.",
    queuedModifyRecords: "The number of records currently sitting in the manual modify queue.",
    sharingGaps: "The number of missing active sharing relationships between active onboarded systems.",
    completeness: "A rolled-up score based on missing date of birth, SSN, gender, address, and communication values. Higher is better.",
    minimumDataSet: "The percentage of records that meet the minimum core demographic dataset by having at least 3 of these 4 fields populated: first name, last name, date of birth, and SSN.",
    staleRate: "The percentage of active records not updated within the configured stale threshold.",
    deleteRate: "The percentage of records from a source that are deleted or no longer active.",
    fragmentationRate: "The percentage of active source identities that live in shared MPI clusters rather than standing alone.",
    processingMinutes: "The time from processing start to completion for a file. Higher values suggest slower intake.",
    rejectedRecords: "The number of records rejected during batch processing due to validation or processing failure.",
    queuedRecords: "The number of records a user has placed into the stewardship workflow for manual action.",
    touches: "The number of times a given identity appears in the modify queue, which can indicate repeated cleanup effort.",
    sharingStatus: "Shows whether a source-to-allowed system relationship is active, inactive, or missing."
  };

  constructor(
    private commonApiService: CommonApiService,
    private sharedService: SharedService,
  ) {}

  ngOnInit(): void {
    this.loadReports();
  }

  loadReports(): void {
    this.loading = true;
    this.refreshing = true;
    this.commonApiService.getAdminReportsDashboard(this.selectedLookbackDays).subscribe({
      next: (response) => {
        this.dashboard = response;
        this.ensureCoverageSourceSelection();
        this.loading = false;
        this.refreshing = false;
        this.loadError = "";
      },
      error: () => {
        this.loading = false;
        this.refreshing = false;
        this.loadError = "Reporting data is currently unavailable.";
      },
    });
  }

  refreshNow(): void {
    this.loadReports();
  }

  async exportPdf(): Promise<void> {
    if (this.exporting || this.loading) {
      return;
    }

    this.exporting = true;
    try {
      const doc = new jsPDF({
        orientation: "landscape",
        unit: "pt",
        format: "letter",
      });
      const margin = 28;
      const pageWidth = doc.internal.pageSize.getWidth();
      const contentWidth = pageWidth - margin * 2;
      let y = this.renderExportCover(doc, margin, contentWidth);

      y = this.appendExportSection(doc, "Overview", y, margin, [
        ["Metric", "Value", "Detail"],
        ...this.overviewCards().map((card) => [card.label, card.value, card.detail]),
      ]);

      y = this.appendExportSection(doc, "Source System Data Quality Scorecard", y, margin, [
        ["Source", "Agency", "Completeness", "Minimum Data Set", "Stale Rate", "Delete Rate", "Missing DOB", "Missing SSN", "Missing Contact", "Protected Population"],
        ...this.dataQualityRows().map((source) => [
          source.sourceSystemName,
          source.agency,
          `${source.completenessScore}%`,
          `${source.minimumDataSetScore}% (${source.minimumDataSetQualifiedCount})`,
          `${source.staleRate}% (${source.staleIdentities})`,
          `${source.deleteRate}% (${source.deletedIdentities})`,
          `${source.missingDobCount}`,
          `${source.missingSsnCount}`,
          `${source.missingAddressCount + source.missingCommunicationCount}`,
          this.protectedDistribution(source),
        ]),
      ]);

      y = this.appendExportSection(doc, "MPI Link IDs Across Multiple Source Systems", y, margin, [
        ["MPI Link ID", "Source Identities", "Distinct Systems", "Systems"],
        ...this.topClusterRows().map((cluster) => [
          cluster.mpiLinkId,
          `${cluster.sourceIdentityCount}`,
          `${cluster.distinctSourceSystems}`,
          cluster.sourceSystems.join(", "),
        ]),
      ]);

      y = this.appendExportSection(doc, "Highest Fragmentation Sources", y, margin, [
        ["Source", "Total Active Identities", "Identities In Shared Link IDs", "Fragmentation Rate"],
        ...this.fragmentationRows().map((source) => [
          source.sourceSystemName,
          `${source.totalActiveIdentities}`,
          `${source.identitiesInSharedLinkIds}`,
          `${source.fragmentationRate}%`,
        ]),
      ]);

      y = this.appendExportSection(doc, "Recent Files", y, margin, [
        ["Request ID", "File", "Source", "Records", "Status", "Processing", "Rejects", "Top Error"],
        ...this.recentFileRows().map((file) => [
          file.requestId,
          file.fileName,
          file.sourceSystemName,
          `${file.recordsCount}`,
          file.status,
          `${file.processingMinutes} min`,
          `${file.rejectCount}`,
          file.topErrorMessage || "None",
        ]),
      ]);

      y = this.appendExportSection(doc, "Source Reliability", y, margin, [
        ["Source", "Files", "Failed", "Rejected", "Avg Min"],
        ...this.sourceReliabilityRows().map((source) => [
          source.sourceSystemName,
          `${source.totalFiles}`,
          `${source.failedFiles}`,
          `${source.rejectedRecords}`,
          `${source.averageProcessingMinutes}`,
        ]),
      ]);

      y = this.appendExportSection(doc, "Top Error Messages", y, margin, [
        ["Source", "Error", "Count"],
        ...this.topErrorRows().map((error) => [
          error.sourceSystemName,
          error.message,
          `${error.count}`,
        ]),
      ]);

      y = this.appendExportSection(doc, "Top Queue Users", y, margin, [
        ["User", "Queued Records"],
        ...this.topUserRows().map((user) => [user.userName, `${user.modifyCount}`]),
      ]);

      y = this.appendExportSection(doc, "Most Touched Identities", y, margin, [
        ["MPI Link ID", "Source", "Source ID", "Touches"],
        ...this.touchedIdentityRows().map((identity) => [
          identity.mpiLinkId,
          identity.sourceSystemName,
          identity.sourceSystemId,
          `${identity.touchCount}`,
        ]),
      ]);

      y = this.appendExportSection(doc, "Recent Downstream Outcomes", y, margin, [
        ["Operation", "Status", "Count"],
        ...this.outcomeRows().map((outcome) => [
          outcome.operationType,
          outcome.status,
          `${outcome.count}`,
        ]),
      ]);

      y = this.appendExportSection(doc, "Operations On MPI Link IDs", y, margin, [
        ["Request Time", "Operation", "Status", "Primary MPI Link ID", "Secondary MPI Link ID", "Affected Link IDs", "Tracking ID"],
        ...this.linkIdOperationRows().map((operation) => [
          this.formatDateTime(operation.requestDateTime),
          operation.operationType,
          operation.status,
          operation.primaryMpiLinkId || "--",
          operation.secondaryMpiLinkId || "--",
          operation.affectedLinkIds || "--",
          operation.trackingId,
        ]),
      ]);

      y = this.appendExportSection(doc, "Onboarded Systems", y, margin, [
        ["System", "Agency", "Status"],
        ...this.onboardedSystemRows().map((system) => [
          system.sourceSystemName,
          system.agencyName,
          system.isActive ? "Active" : "Inactive",
        ]),
      ]);

      y = this.appendExportSection(doc, "Coverage By Source System", y, margin, [
        ["Source", "Agency", "Active", "Inactive", "Gaps", "Target Systems"],
        ...this.coverageSummaryRows().map((row) => [
          row.sourceSystemName,
          row.agencyName,
          `${row.activeMappings}`,
          `${row.inactiveMappings}`,
          `${row.gapCount}`,
          `${row.totalTargets}`,
        ]),
      ]);

      this.appendExportSection(doc, "Coverage Explorer", y, margin, [
        ["Allowed System", "Agency", "Status", "Sharing Level"],
        ...this.coverageExplorerRows().map((row) => [
          row.allowedSystemName,
          row.agencyName,
          row.status === "gap" ? "Gap" : this.toTitleCase(row.status),
          row.dataSharingLevel,
        ]),
      ]);

      const safeDate = new Date().toISOString().slice(0, 10);
      doc.save(`mpi-reports-${safeDate}.pdf`);
    } catch (error) {
      const message = error instanceof Error ? error.message : "Unable to export reports.";
      this.sharedService.showToast(message);
    } finally {
      this.exporting = false;
    }
  }

  onLookbackChange(value: string): void {
    this.selectedLookbackDays = Number(value);
    this.loadReports();
  }

  overviewCards(): ReportOverviewCard[] {
    return [
      {
        label: "Sources Scored",
        value: `${this.dashboard.dataQuality.totalSources}`,
        detail: "Active scorecard coverage",
        tooltip: this.metricTooltips["sourcesScored"],
      },
      {
        label: "Unique MPI Link IDs",
        value: `${this.dashboard.linkage.uniqueLinkIds}`,
        detail: "Current active person clusters",
        tooltip: this.metricTooltips["uniqueLinkIds"],
      },
      {
        label: "Files In Window",
        value: `${this.dashboard.batchIntake.totalFiles}`,
        detail: `Last ${this.dashboard.lookbackDays} days`,
        tooltip: this.metricTooltips["filesInWindow"],
      },
      {
        label: "Queued Modify Records",
        value: `${this.dashboard.stewardship.totalQueuedRecords}`,
        detail: "Current stewardship queue",
        tooltip: this.metricTooltips["queuedModifyRecords"],
      },
      {
        label: "Sharing Gaps",
        value: `${this.dashboard.dataSharing.gapCount}`,
        detail: "Missing active sharing relationships",
        tooltip: this.metricTooltips["sharingGaps"],
      },
    ];
  }

  activeSystems(): SystemReferenceRow[] {
    const filtered = this.filterItems(
      this.allActiveSystems(),
      this.dataSharingSearch,
      (system) => `${system.sourceSystemName} ${system.agencyName}`
    );

    return this.sortItems(filtered, this.sortStates["onboardedSystems"], (system, key) => this.systemSortValue(system, key));
  }

  allActiveSystems(): SystemReferenceRow[] {
    return this.sortItems(
      this.dashboard.dataSharing.systems.filter((system) => system.isActive),
      { key: "sourceSystemName", direction: "asc" },
      (system, key) => this.systemSortValue(system, key)
    );
  }

  sharingCell(sourceSystemId: number, allowedSystemId: number): DataSharingMatrixRow | undefined {
    return this.dashboard.dataSharing.matrix.find(
      (cell) => cell.sourceSystemId === sourceSystemId && cell.allowedSystemId === allowedSystemId
    );
  }

  sharingCellLabel(sourceSystemId: number, allowedSystemId: number): string {
    const cell = this.sharingCell(sourceSystemId, allowedSystemId);
    if (!cell) {
      return "Gap";
    }

    if (cell.status === "active") {
      return cell.dataSharingLevel;
    }

    if (cell.status === "inactive") {
      return `Inactive: ${cell.dataSharingLevel}`;
    }

    return "Gap";
  }

  sharingClass(status: string | undefined): string {
    const normalized = (status || "gap").toLowerCase();
    return `sharing-pill sharing-pill--${normalized}`;
  }

  qualityClass(source: SourceSystemQualityRow): string {
    if (source.completenessScore < 75 || source.staleRate >= 20) {
      return "report-table__row report-table__row--critical";
    }

    if (source.completenessScore < 90 || source.staleRate >= 10 || source.deleteRate >= 10) {
      return "report-table__row report-table__row--warning";
    }

    return "report-table__row";
  }

  protectedDistribution(source: SourceSystemQualityRow): string {
    if (!source.protectedPopulationBreakdown.length) {
      return "None";
    }

    return source.protectedPopulationBreakdown
      .map((item) => `${item.type} (${item.count})`)
      .join(", ");
  }

  formatDateTime(value: string): string {
    return new Date(value).toLocaleString();
  }

  dataQualityRows(): SourceSystemQualityRow[] {
    const filtered = this.filterItems(
      this.dashboard.dataQuality.sources,
      this.dataQualitySearch,
      (source) => `${source.sourceSystemName} ${source.agency} ${this.protectedDistribution(source)}`
    );

    return this.sortItems(filtered, this.sortStates["dataQuality"], (source, key) => this.dataQualitySortValue(source, key));
  }

  topClusterRows(): LinkClusterRow[] {
    const filtered = this.filterItems(
      this.dashboard.linkage.multiSourceLinkDetails,
      this.linkageSearch,
      (cluster) => `${cluster.mpiLinkId} ${cluster.sourceSystems.join(" ")}`
    );

    return this.sortItems(filtered, this.sortStates["topClusters"], (cluster, key) => this.topClusterSortValue(cluster, key));
  }

  fragmentationRows(): SourceFragmentationRow[] {
    const filtered = this.filterItems(
      this.dashboard.linkage.highestFragmentationSources,
      this.linkageSearch,
      (source) => `${source.sourceSystemName}`
    );

    return this.sortItems(filtered, this.sortStates["fragmentation"], (source, key) => this.fragmentationSortValue(source, key));
  }

  recentFileRows(): BatchFileRow[] {
    const filtered = this.filterItems(
      this.dashboard.batchIntake.files,
      this.batchSearch,
      (file) => `${file.requestId} ${file.fileName} ${file.sourceSystemName} ${file.status} ${file.topErrorMessage}`
    );

    return this.sortItems(filtered, this.sortStates["files"], (file, key) => this.fileSortValue(file, key));
  }

  sourceReliabilityRows(): BatchSourceSummaryRow[] {
    const filtered = this.filterItems(
      this.dashboard.batchIntake.sourceSummaries,
      this.batchSearch,
      (source) => `${source.sourceSystemName}`
    );

    return this.sortItems(filtered, this.sortStates["sourceReliability"], (source, key) => this.sourceReliabilitySortValue(source, key));
  }

  topErrorRows(): BatchErrorRow[] {
    const filtered = this.filterItems(
      this.dashboard.batchIntake.topErrors,
      this.batchSearch,
      (error) => `${error.sourceSystemName} ${error.message}`
    );

    return this.sortItems(filtered, this.sortStates["topErrors"], (error, key) => this.topErrorSortValue(error, key));
  }

  topUserRows(): UserModifySummaryRow[] {
    const filtered = this.filterItems(
      this.dashboard.stewardship.topUsers,
      this.stewardshipSearch,
      (user) => `${user.userName}`
    );

    return this.sortItems(filtered, this.sortStates["topUsers"], (user, key) => this.topUserSortValue(user, key));
  }

  touchedIdentityRows(): TouchedIdentityRow[] {
    const filtered = this.filterItems(
      this.dashboard.stewardship.mostTouchedIdentities,
      this.stewardshipSearch,
      (identity) => `${identity.mpiLinkId} ${identity.sourceSystemName} ${identity.sourceSystemId}`
    );

    return this.sortItems(filtered, this.sortStates["touchedIdentities"], (identity, key) => this.touchedIdentitySortValue(identity, key));
  }

  outcomeRows(): StewardshipOutcomeRow[] {
    const filtered = this.filterItems(
      this.dashboard.stewardship.downstreamOutcomes,
      this.stewardshipSearch,
      (outcome) => `${outcome.operationType} ${outcome.status}`
    );

    return this.sortItems(filtered, this.sortStates["outcomes"], (outcome, key) => this.outcomeSortValue(outcome, key));
  }

  linkIdOperationRows(): LinkIdOperationRow[] {
    const filtered = this.filterItems(
      this.dashboard.stewardship.linkIdOperations,
      this.stewardshipSearch,
      (operation) => `${operation.operationType} ${operation.status} ${operation.trackingId} ${operation.primaryMpiLinkId} ${operation.secondaryMpiLinkId} ${operation.affectedLinkIds} ${operation.message}`
    );

    return this.sortItems(filtered, this.sortStates["linkIdOperations"], (operation, key) => this.linkIdOperationSortValue(operation, key));
  }

  onboardedSystemRows(): SystemReferenceRow[] {
    return this.activeSystems();
  }

  coverageSummaryRows(): DataSharingCoverageSummaryRow[] {
    const systems = this.allActiveSystems();
    const rows = systems.map((system) => {
      const relationships = systems
        .filter((allowed) => allowed.id !== system.id)
        .map((allowed) => this.sharingCell(system.id, allowed.id));

      return {
        sourceSystemId: system.id,
        sourceSystemName: system.sourceSystemName,
        agencyName: system.agencyName,
        activeMappings: relationships.filter((item) => item?.status === "active").length,
        inactiveMappings: relationships.filter((item) => item?.status === "inactive").length,
        gapCount: relationships.filter((item) => !item || item.status === "gap").length,
        totalTargets: relationships.length,
      };
    });

    return this.sortItems(rows, this.sortStates["coverageSummary"], (row, key) => this.coverageSummarySortValue(row, key));
  }

  selectedCoverageSource(): SystemReferenceRow | undefined {
    return this.allActiveSystems().find((system) => system.id === this.selectedCoverageSourceSystemId);
  }

  coverageExplorerRows(): DataSharingRelationshipRow[] {
    const source = this.selectedCoverageSource();
    if (!source) {
      return [];
    }

    const rows = this.allActiveSystems()
      .filter((allowed) => allowed.id !== source.id)
      .map((allowed) => {
        const cell = this.sharingCell(source.id, allowed.id);
        return {
          allowedSystemId: allowed.id,
          allowedSystemName: allowed.sourceSystemName,
          agencyName: allowed.agencyName,
          status: cell?.status || "gap",
          dataSharingLevel: cell?.dataSharingLevel || "Gap",
        };
      })
      .filter((row) => this.selectedCoverageStatusFilter === "all" || row.status === this.selectedCoverageStatusFilter);

    return this.sortItems(rows, this.sortStates["coverageExplorer"], (row, key) => this.coverageExplorerSortValue(row, key));
  }

  onCoverageSourceChange(value: string): void {
    this.selectedCoverageSourceSystemId = value ? Number(value) : null;
  }

  onCoverageStatusFilterChange(value: string): void {
    this.selectedCoverageStatusFilter = value || "all";
  }

  setSort(table: string, key: string): void {
    const current = this.sortStates[table];
    if (!current || current.key !== key) {
      this.sortStates[table] = { key, direction: "asc" };
      return;
    }

    this.sortStates[table] = {
      key,
      direction: current.direction === "asc" ? "desc" : "asc",
    };
  }

  sortIndicator(table: string, key: string): string {
    const current = this.sortStates[table];
    if (!current || current.key !== key) {
      return "";
    }

    return current.direction === "asc" ? "↑" : "↓";
  }

  lastUpdatedLabel(): string {
    return this.formatDateTime(this.dashboard.lastUpdated);
  }

  trackBySource(_: number, item: SourceSystemQualityRow): string {
    return `${item.sourceSystemName}-${item.agency}`;
  }

  trackBySystem(_: number, item: SystemReferenceRow): number {
    return item.id;
  }

  trackByCoverageSummary(_: number, item: DataSharingCoverageSummaryRow): number {
    return item.sourceSystemId;
  }

  trackByCoverageRelationship(_: number, item: DataSharingRelationshipRow): number {
    return item.allowedSystemId;
  }

  trackByText(_: number, item: { sourceSystemName?: string; userName?: string; requestId?: string; message?: string; mpiLinkId?: string; operationType?: string; status?: string }): string {
    return `${item.sourceSystemName || item.userName || item.requestId || item.message || item.mpiLinkId || item.operationType}-${item.status || ""}`;
  }

  trackByLinkOperation(_: number, item: LinkIdOperationRow): string {
    return `${item.requestDateTime}-${item.trackingId}-${item.operationType}-${item.affectedLinkIds}`;
  }

  private filterItems<T>(items: T[], search: string, stringify: (item: T) => string): T[] {
    const normalized = search.trim().toLowerCase();
    if (!normalized) {
      return items;
    }

    return items.filter((item) => stringify(item).toLowerCase().includes(normalized));
  }

  private sortItems<T>(items: T[], sortState: SortState, valueGetter: (item: T, key: string) => string | number): T[] {
    return [...items].sort((left, right) => {
      const leftValue = valueGetter(left, sortState.key);
      const rightValue = valueGetter(right, sortState.key);
      const comparison = this.compareValues(leftValue, rightValue);
      return sortState.direction === "asc" ? comparison : comparison * -1;
    });
  }

  private compareValues(left: string | number, right: string | number): number {
    if (typeof left === "number" && typeof right === "number") {
      return left - right;
    }

    return String(left).localeCompare(String(right), undefined, { numeric: true, sensitivity: "base" });
  }

  private dataQualitySortValue(source: SourceSystemQualityRow, key: string): string | number {
    switch (key) {
      case "agency": return source.agency;
      case "completenessScore": return source.completenessScore;
      case "minimumDataSetScore": return source.minimumDataSetScore;
      case "staleRate": return source.staleRate;
      case "deleteRate": return source.deleteRate;
      case "missingDobCount": return source.missingDobCount;
      case "missingSsnCount": return source.missingSsnCount;
      case "missingContactCount": return source.missingAddressCount + source.missingCommunicationCount;
      case "protectedPopulationCount": return source.protectedPopulationCount;
      default: return source.sourceSystemName;
    }
  }

  private topClusterSortValue(cluster: LinkClusterRow, key: string): string | number {
    switch (key) {
      case "sourceIdentityCount": return cluster.sourceIdentityCount;
      case "distinctSourceSystems": return cluster.distinctSourceSystems;
      case "sourceSystems": return cluster.sourceSystems.join(", ");
      default: return cluster.mpiLinkId;
    }
  }

  private fragmentationSortValue(source: SourceFragmentationRow, key: string): string | number {
    switch (key) {
      case "totalActiveIdentities": return source.totalActiveIdentities;
      case "identitiesInSharedLinkIds": return source.identitiesInSharedLinkIds;
      case "fragmentationRate": return source.fragmentationRate;
      default: return source.sourceSystemName;
    }
  }

  private fileSortValue(file: BatchFileRow, key: string): string | number {
    switch (key) {
      case "requestId": return file.requestId;
      case "fileName": return file.fileName;
      case "sourceSystemName": return file.sourceSystemName;
      case "recordsCount": return file.recordsCount;
      case "status": return file.status;
      case "processingMinutes": return file.processingMinutes;
      case "rejectCount": return file.rejectCount;
      case "topErrorMessage": return file.topErrorMessage;
      default: return file.requestDateTime;
    }
  }

  private sourceReliabilitySortValue(source: BatchSourceSummaryRow, key: string): string | number {
    switch (key) {
      case "totalFiles": return source.totalFiles;
      case "failedFiles": return source.failedFiles;
      case "rejectedRecords": return source.rejectedRecords;
      case "averageProcessingMinutes": return source.averageProcessingMinutes;
      default: return source.sourceSystemName;
    }
  }

  private topErrorSortValue(error: BatchErrorRow, key: string): string | number {
    switch (key) {
      case "message": return error.message;
      case "count": return error.count;
      default: return error.sourceSystemName;
    }
  }

  private topUserSortValue(user: UserModifySummaryRow, key: string): string | number {
    switch (key) {
      case "modifyCount": return user.modifyCount;
      default: return user.userName;
    }
  }

  private touchedIdentitySortValue(identity: TouchedIdentityRow, key: string): string | number {
    switch (key) {
      case "sourceSystemName": return identity.sourceSystemName;
      case "sourceSystemId": return identity.sourceSystemId;
      case "touchCount": return identity.touchCount;
      default: return identity.mpiLinkId;
    }
  }

  private outcomeSortValue(outcome: StewardshipOutcomeRow, key: string): string | number {
    switch (key) {
      case "status": return outcome.status;
      case "count": return outcome.count;
      default: return outcome.operationType;
    }
  }

  private linkIdOperationSortValue(operation: LinkIdOperationRow, key: string): string | number {
    switch (key) {
      case "operationType": return operation.operationType;
      case "status": return operation.status;
      case "trackingId": return operation.trackingId;
      case "primaryMpiLinkId": return operation.primaryMpiLinkId;
      case "secondaryMpiLinkId": return operation.secondaryMpiLinkId;
      case "affectedLinkIds": return operation.affectedLinkIds;
      default: return operation.requestDateTime;
    }
  }

  private systemSortValue(system: SystemReferenceRow, key: string): string | number {
    switch (key) {
      case "agencyName": return system.agencyName;
      case "isActive": return system.isActive ? 1 : 0;
      default: return system.sourceSystemName;
    }
  }

  private coverageSummarySortValue(row: DataSharingCoverageSummaryRow, key: string): string | number {
    switch (key) {
      case "agencyName": return row.agencyName;
      case "activeMappings": return row.activeMappings;
      case "inactiveMappings": return row.inactiveMappings;
      case "gapCount": return row.gapCount;
      case "totalTargets": return row.totalTargets;
      default: return row.sourceSystemName;
    }
  }

  private coverageExplorerSortValue(row: DataSharingRelationshipRow, key: string): string | number {
    switch (key) {
      case "agencyName": return row.agencyName;
      case "status": return row.status;
      case "dataSharingLevel": return row.dataSharingLevel;
      default: return row.allowedSystemName;
    }
  }

  private ensureCoverageSourceSelection(): void {
    const systems = this.allActiveSystems();
    if (!systems.length) {
      this.selectedCoverageSourceSystemId = null;
      return;
    }

    const exists = systems.some((system) => system.id === this.selectedCoverageSourceSystemId);
    if (!exists) {
      this.selectedCoverageSourceSystemId = systems[0].id;
    }
  }

  private renderExportCover(doc: jsPDF, margin: number, contentWidth: number): number {
    const exportedBy = localStorage.getItem("LoggedInUser") || "Unknown User";
    doc.setFontSize(16);
    doc.setFont("helvetica", "bold");
    doc.text("MPI Reports", margin, margin + 8);

    doc.setFontSize(10);
    doc.setFont("helvetica", "normal");
    const lines = [
      `Exported By: ${exportedBy}`,
      `Exported At: ${new Date().toLocaleString()}`,
      `Last Refreshed: ${this.lastUpdatedLabel()}`,
      `Analysis Window: ${this.dashboard.lookbackDays} days`,
      `Stale Threshold: ${this.dashboard.staleThresholdDays} days`,
    ];

    let y = margin + 28;
    for (const line of lines) {
      doc.text(line, margin, y);
      y += 14;
    }

    doc.setDrawColor(180, 190, 200);
    doc.line(margin, y + 4, margin + contentWidth, y + 4);
    return y + 22;
  }

  private appendExportSection(doc: jsPDF, title: string, startY: number, margin: number, rows: string[][]): number {
    const y = this.ensureExportSpace(doc, startY, margin, 54);
    doc.setFontSize(13);
    doc.setFont("helvetica", "bold");
    doc.setTextColor(25, 50, 77);
    doc.text(title, margin, y);
    doc.setFont("helvetica", "normal");
    doc.setTextColor(0, 0, 0);

    const [head, ...body] = rows;
    autoTable(doc, {
      startY: y + 10,
      margin: { left: margin, right: margin },
      head: [head],
      body: body.length ? body : [["No data available"]],
      styles: {
        fontSize: 7,
        cellPadding: 3,
        overflow: "linebreak",
      },
      headStyles: {
        fillColor: [59, 68, 95],
        textColor: 255,
        fontStyle: "bold",
      },
      alternateRowStyles: {
        fillColor: [248, 251, 253],
      },
    });

    const finalY = (doc as jsPDF & { lastAutoTable?: { finalY: number } }).lastAutoTable?.finalY;
    return (finalY ?? y) + 18;
  }

  private ensureExportSpace(doc: jsPDF, y: number, margin: number, needed: number): number {
    const pageHeight = doc.internal.pageSize.getHeight();
    if (y + needed <= pageHeight - margin) {
      return y;
    }

    doc.addPage();
    return margin;
  }

  private toTitleCase(value: string): string {
    return value ? value.charAt(0).toUpperCase() + value.slice(1).toLowerCase() : value;
  }

  private buildEmptyDashboard(): AdminReportsDashboardResponse {
    return {
      lookbackDays: this.selectedLookbackDays,
      staleThresholdDays: 30,
      lastUpdated: new Date().toISOString(),
      dataQuality: {
        totalSources: 0,
        totalActiveIdentities: 0,
        totalDeletedIdentities: 0,
        sources: [],
      },
      linkage: {
        uniqueLinkIds: 0,
        averageSourceIdentitiesPerLinkId: 0,
        multiSourceLinkIds: 0,
        multiSourceLinkDetails: [],
        recentActivity: [],
        highestFragmentationSources: [],
      },
      batchIntake: {
        totalFiles: 0,
        failedFiles: 0,
        rejectedRecords: 0,
        averageProcessingMinutes: 0,
        files: [],
        sourceSummaries: [],
        topErrors: [],
      },
      stewardship: {
        totalQueuedRecords: 0,
        distinctUsers: 0,
        distinctTouchedIdentities: 0,
        topUsers: [],
        mostTouchedIdentities: [],
        downstreamOutcomes: [],
        linkIdOperations: [],
      },
      dataSharing: {
        totalSystems: 0,
        activeSystems: 0,
        activeMappings: 0,
        inactiveMappings: 0,
        gapCount: 0,
        systems: [],
        matrix: [],
      },
    };
  }
}
