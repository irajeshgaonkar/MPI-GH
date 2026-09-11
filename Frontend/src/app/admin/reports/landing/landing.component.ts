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
import {
  buildLinkIdIngestChartData,
  buildLinkIdIngestSourceRows,
  buildLinkIdIngestView,
  extractLinkIdIngestSourceOptions,
  filterLinkIdIngestViewByLookback,
  filterTrendRowsByLookback,
  formatIngestCountWithPercent,
  LINK_ID_INGEST_PAGE_SIZE,
  LinkIdIngestChartPoint,
  LinkIdIngestDailyRow,
  LinkIdIngestSourceRow,
  paginateItems,
  syncSelectedSources,
} from "./link-id-ingest.util";

type SortDirection = "asc" | "desc";

type ReportsExportSectionId =
  | "overview"
  | "data-quality"
  | "incoming-match-trend"
  | "mpi-linkage"
  | "batch-intake"
  | "stewardship"
  | "data-sharing";

interface ReportsExportSectionOption {
  id: ReportsExportSectionId;
  label: string;
}

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
  readonly linkIdIngestVolumeSeries: MetricChartSeries[] = [
    { key: "newPerson", label: "New Link ID", color: "#0b84f3" },
    { key: "alreadyInMpi", label: "Already in MPI", color: "#00a389" },
  ];
  readonly linkIdIngestMatchRateSeries: MetricChartSeries[] = [
    { key: "alreadyInMpiPercent", label: "Already in MPI %", color: "#d14343" },
  ];
  readonly linkIdIngestPageSize = LINK_ID_INGEST_PAGE_SIZE;
  readonly reportsExportSections: ReportsExportSectionOption[] = [
    { id: "overview", label: "Overview" },
    { id: "incoming-match-trend", label: "Incoming Match Trend" },
    { id: "data-quality", label: "Source System Data Quality" },
    { id: "mpi-linkage", label: "MPI Linkage" },
    { id: "batch-intake", label: "Batch Intake Reliability" },
    { id: "stewardship", label: "Manual Stewardship" },
    { id: "data-sharing", label: "Data Sharing Coverage" },
  ];

  loading = false;
  refreshing = false;
  exporting = false;
  exportDialogOpen = false;
  exportSelectAll = false;
  selectedExportSections = new Set<ReportsExportSectionId>();
  loadError = "";
  selectedLookbackDays = 30;
  dataQualitySearch = "";
  linkageSearch = "";
  batchSearch = "";
  stewardshipSearch = "";
  dataSharingSearch = "";
  selectedCoverageSourceSystemId: number | null = null;
  selectedCoverageStatusFilter = "all";
  selectedLinkIdIngestSources: string[] = [];
  selectedLinkIdIngestLookbackDays = 30;
  linkIdIngestSourceSearch = "";
  linkIdIngestSourceMenuOpen = false;
  linkIdIngestDayPageIndex = 0;
  linkIdIngestSourcePageIndex = 0;
  linkIdIngestChartPoints: LinkIdIngestChartPoint[] = [];
  private linkIdIngestSourcesInitialized = false;
  dashboard: AdminReportsDashboardResponse = this.buildEmptyDashboard();
  sortStates: Record<string, SortState> = {
    dataQuality: { key: "sourceSystemName", direction: "asc" },
    linkIdIngest: { key: "date", direction: "asc" },
    linkIdIngestSource: { key: "incomingRecords", direction: "desc" },
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
    linkage: "Shows MPI link IDs that span multiple source systems, along with fragmentation, incoming match trends, and recent merge or unmerge activity that may signal identity quality issues.",
    linkIdIngest: "Shows first-time records received by MPI and whether each created a new Link ID or matched someone already in MPI. Matching is evaluated across all sources, even when one source is selected.",
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
        const previousOptions = extractLinkIdIngestSourceOptions(this.dashboard.linkage.linkIdIngestTrend || []);
        const previouslyAllSelected = this.linkIdIngestSourcesInitialized
          && this.isLinkIdIngestAllSourcesSelectedAgainst(previousOptions, this.selectedLinkIdIngestSources);
        this.dashboard = response;
        this.ensureCoverageSourceSelection();
        this.syncLinkIdIngestSourceSelection(previouslyAllSelected);
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

  openExportDialog(): void {
    if (this.exporting || this.loading) {
      return;
    }

    this.selectedExportSections = new Set(this.reportsExportSections.map((section) => section.id));
    this.exportSelectAll = true;
    this.exportDialogOpen = true;
  }

  closeExportDialog(): void {
    if (this.exporting) {
      return;
    }

    this.exportDialogOpen = false;
  }

  isExportSectionSelected(id: ReportsExportSectionId): boolean {
    return this.selectedExportSections.has(id);
  }

  toggleExportSelectAll(checked: boolean): void {
    this.exportSelectAll = checked;
    this.selectedExportSections = checked
      ? new Set(this.reportsExportSections.map((section) => section.id))
      : new Set();
  }

  toggleExportSection(id: ReportsExportSectionId, checked: boolean): void {
    const next = new Set(this.selectedExportSections);
    if (checked) {
      next.add(id);
    } else {
      next.delete(id);
    }

    this.selectedExportSections = next;
    this.exportSelectAll = this.reportsExportSections.every((section) => next.has(section.id));
  }

  canRunExport(): boolean {
    return this.selectedExportSections.size > 0 && !this.exporting && !this.loading;
  }

  async exportPdf(): Promise<void> {
    if (!this.canRunExport()) {
      return;
    }

    this.exporting = true;
    try {
      const selected = this.selectedExportSections;
      const doc = new jsPDF({
        orientation: "landscape",
        unit: "pt",
        format: "letter",
      });
      const margin = 28;
      const pageWidth = doc.internal.pageSize.getWidth();
      const contentWidth = pageWidth - margin * 2;
      let y = this.renderExportCover(doc, margin, contentWidth);

      if (selected.has("overview")) {
        y = this.appendExportSection(doc, "Overview", y, margin, [
          ["Metric", "Value", "Detail"],
          ...this.overviewCards().map((card) => [card.label, card.value, card.detail]),
        ]);
      }

      if (selected.has("incoming-match-trend")) {
        y = this.appendIncomingMatchTrendExport(doc, y, margin);
      }

      if (selected.has("data-quality")) {
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
      }

      if (selected.has("mpi-linkage")) {
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
      }

      if (selected.has("batch-intake")) {
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
      }

      if (selected.has("stewardship")) {
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
      }

      if (selected.has("data-sharing")) {
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
      }

      const safeDate = new Date().toISOString().slice(0, 10);
      doc.save(`mpi-reports-${safeDate}.pdf`);
      this.exportDialogOpen = false;
    } catch (error) {
      const message = error instanceof Error ? error.message : "Unable to export reports.";
      this.sharedService.showToast(message);
    } finally {
      this.exporting = false;
    }
  }

  private appendIncomingMatchTrendExport(doc: jsPDF, startY: number, margin: number): number {
    const ingestView = this.linkIdIngestView();
    const ingestFilterLabel = this.linkIdIngestSourceFilterLabel();
    let y = this.appendExportSection(doc, `Incoming Match Trend (${ingestFilterLabel})`, startY, margin, [
      ["Metric", "Value"],
      [this.linkIdIngestSummaryHeadline(), ""],
      [
        "Incoming",
        `${ingestView.totals.incomingRecords}`,
      ],
      [
        "New Link ID",
        formatIngestCountWithPercent(ingestView.totals.newPersonRecords, ingestView.totals.newPersonPercent),
      ],
      [
        "Already in MPI",
        formatIngestCountWithPercent(ingestView.totals.alreadyInMpiRecords, ingestView.totals.alreadyInMpiPercent),
      ],
      [
        "Match rate",
        `${ingestView.totals.alreadyInMpiPercent}%`,
      ],
    ]);

    y = this.appendExportSection(doc, "Incoming Match Trend By Day", y, margin, [
      ["Date", "Incoming", "New Link ID", "Already in MPI"],
      ...this.linkIdIngestRows().map((row) => [
        this.formatIngestDate(row.date),
        `${row.incomingRecords}`,
        formatIngestCountWithPercent(row.newPersonRecords, row.newPersonPercent),
        formatIngestCountWithPercent(row.alreadyInMpiRecords, row.alreadyInMpiPercent),
      ]),
      [
        "Total",
        `${ingestView.totals.incomingRecords}`,
        formatIngestCountWithPercent(ingestView.totals.newPersonRecords, ingestView.totals.newPersonPercent),
        formatIngestCountWithPercent(ingestView.totals.alreadyInMpiRecords, ingestView.totals.alreadyInMpiPercent),
      ],
    ]);

    if (this.showLinkIdIngestBySource()) {
      const sourceRows = this.linkIdIngestSourceRows();
      y = this.appendExportSection(doc, "Incoming Match Trend By Source", y, margin, [
        ["Source", "Incoming", "New Link ID", "Already in MPI", "Match rate"],
        ...sourceRows.map((row) => [
          row.sourceSystemName,
          `${row.incomingRecords}`,
          formatIngestCountWithPercent(row.newPersonRecords, row.newPersonPercent),
          formatIngestCountWithPercent(row.alreadyInMpiRecords, row.alreadyInMpiPercent),
          `${row.alreadyInMpiPercent}%`,
        ]),
        [
          "Total",
          `${ingestView.totals.incomingRecords}`,
          formatIngestCountWithPercent(ingestView.totals.newPersonRecords, ingestView.totals.newPersonPercent),
          formatIngestCountWithPercent(ingestView.totals.alreadyInMpiRecords, ingestView.totals.alreadyInMpiPercent),
          `${ingestView.totals.alreadyInMpiPercent}%`,
        ],
      ]);
    }

    return y;
  }

  onLookbackChange(value: string): void {
    this.selectedLookbackDays = Number(value);
    if (this.selectedLinkIdIngestLookbackDays > this.selectedLookbackDays) {
      this.selectedLinkIdIngestLookbackDays = this.selectedLookbackDays;
    }
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

  linkIdIngestView() {
    const baseView = buildLinkIdIngestView(
      this.dashboard.linkage.linkIdIngestTrend || [],
      this.selectedLinkIdIngestSources,
    );

    return filterLinkIdIngestViewByLookback(baseView, this.selectedLinkIdIngestLookbackDays);
  }

  linkIdIngestSourceOptions(): string[] {
    return extractLinkIdIngestSourceOptions(this.dashboard.linkage.linkIdIngestTrend || []);
  }

  linkIdIngestFilteredSourceOptions(): string[] {
    const search = this.linkIdIngestSourceSearch.trim().toLowerCase();
    const options = this.linkIdIngestSourceOptions();
    if (!search) {
      return options;
    }

    return options.filter((source) => source.toLowerCase().includes(search));
  }

  linkIdIngestRows(): LinkIdIngestDailyRow[] {
    const rows = this.linkIdIngestView().dailyRows;
    return this.sortItems(rows, this.sortStates["linkIdIngest"], (row, key) => this.linkIdIngestSortValue(row, key));
  }

  linkIdIngestPagedRows(): LinkIdIngestDailyRow[] {
    return paginateItems(
      this.linkIdIngestRows(),
      this.linkIdIngestDayPageIndex,
      this.linkIdIngestPageSize,
    ).pageItems;
  }

  linkIdIngestDayPagerLabel(): string {
    return this.pagerLabel(this.linkIdIngestRows().length, this.linkIdIngestDayPageIndex, "days");
  }

  linkIdIngestSourceRows(): LinkIdIngestSourceRow[] {
    const lookbackRows = filterTrendRowsByLookback(
      this.dashboard.linkage.linkIdIngestTrend || [],
      this.selectedLinkIdIngestLookbackDays,
    );
    const rows = buildLinkIdIngestSourceRows(lookbackRows, this.selectedLinkIdIngestSources);
    return this.sortItems(
      rows,
      this.sortStates["linkIdIngestSource"],
      (row, key) => this.linkIdIngestSourceSortValue(row, key),
    );
  }

  linkIdIngestPagedSourceRows(): LinkIdIngestSourceRow[] {
    return paginateItems(
      this.linkIdIngestSourceRows(),
      this.linkIdIngestSourcePageIndex,
      this.linkIdIngestPageSize,
    ).pageItems;
  }

  linkIdIngestSourcePagerLabel(): string {
    return this.pagerLabel(this.linkIdIngestSourceRows().length, this.linkIdIngestSourcePageIndex, "sources");
  }

  linkIdIngestChartData(): LinkIdIngestChartPoint[] {
    return this.linkIdIngestChartPoints;
  }

  linkIdIngestHasData(): boolean {
    return this.selectedLinkIdIngestSources.length > 0
      && this.linkIdIngestView().totals.incomingRecords > 0;
  }

  linkIdIngestHasNoSourcesSelected(): boolean {
    return this.linkIdIngestSourceOptions().length > 0
      && this.selectedLinkIdIngestSources.length === 0;
  }

  showLinkIdIngestBySource(): boolean {
    return this.linkIdIngestHasData() && this.selectedLinkIdIngestSources.length !== 1;
  }

  isLinkIdIngestAllSourcesSelected(): boolean {
    const options = this.linkIdIngestSourceOptions();
    return options.length > 0
      && this.selectedLinkIdIngestSources.length === options.length
      && options.every((source) => this.selectedLinkIdIngestSources.includes(source));
  }

  linkIdIngestSourceFilterLabel(): string {
    if (this.isLinkIdIngestAllSourcesSelected()) {
      return "All sources";
    }

    if (this.selectedLinkIdIngestSources.length === 0) {
      return "No sources";
    }

    if (this.selectedLinkIdIngestSources.length <= 3) {
      return this.selectedLinkIdIngestSources.join(", ");
    }

    return `${this.selectedLinkIdIngestSources.length} sources`;
  }

  linkIdIngestSummaryHeadline(): string {
    const totals = this.linkIdIngestView().totals;
    const lookbackLabel = `${this.selectedLinkIdIngestLookbackDays} days`;
    if (this.isLinkIdIngestAllSourcesSelected()) {
      return `${totals.incomingRecords} records in the last ${lookbackLabel}`;
    }

    return `${totals.incomingRecords} records from ${this.linkIdIngestSourceFilterLabel()} in the last ${lookbackLabel}`;
  }

  onLinkIdIngestSelectAllChange(checked: boolean): void {
    this.selectedLinkIdIngestSources = checked ? [...this.linkIdIngestSourceOptions()] : [];
    this.resetLinkIdIngestPages();
    this.refreshLinkIdIngestChartData();
  }

  onLinkIdIngestSourceToggle(source: string, checked: boolean): void {
    if (checked) {
      if (!this.selectedLinkIdIngestSources.includes(source)) {
        this.selectedLinkIdIngestSources = [...this.selectedLinkIdIngestSources, source];
      }
    } else {
      this.selectedLinkIdIngestSources = this.selectedLinkIdIngestSources.filter((item) => item !== source);
    }

    this.resetLinkIdIngestPages();
    this.refreshLinkIdIngestChartData();
  }

  isLinkIdIngestSourceSelected(source: string): boolean {
    return this.selectedLinkIdIngestSources.includes(source);
  }

  toggleLinkIdIngestSourceMenu(): void {
    this.linkIdIngestSourceMenuOpen = !this.linkIdIngestSourceMenuOpen;
    if (!this.linkIdIngestSourceMenuOpen) {
      this.linkIdIngestSourceSearch = "";
    }
  }

  closeLinkIdIngestSourceMenu(): void {
    this.linkIdIngestSourceMenuOpen = false;
    this.linkIdIngestSourceSearch = "";
  }

  onLinkIdIngestSourceMenuFocusOut(event: FocusEvent): void {
    const nextTarget = event.relatedTarget as Node | null;
    const currentTarget = event.currentTarget as HTMLElement | null;
    if (currentTarget && nextTarget && currentTarget.contains(nextTarget)) {
      return;
    }

    this.closeLinkIdIngestSourceMenu();
  }

  onLinkIdIngestLookbackChange(value: number): void {
    const days = Number(value);
    this.selectedLinkIdIngestLookbackDays = days;
    this.resetLinkIdIngestPages();
    this.refreshLinkIdIngestChartData();

    if (days > this.selectedLookbackDays) {
      this.selectedLookbackDays = days;
      this.loadReports();
    }
  }

  canGoLinkIdIngestDayPrev(): boolean {
    return this.linkIdIngestDayPageIndex > 0;
  }

  canGoLinkIdIngestDayNext(): boolean {
    return this.linkIdIngestDayPageIndex < this.pageCountFor(this.linkIdIngestRows().length) - 1;
  }

  goLinkIdIngestDayPrev(): void {
    if (this.canGoLinkIdIngestDayPrev()) {
      this.linkIdIngestDayPageIndex -= 1;
    }
  }

  goLinkIdIngestDayNext(): void {
    if (this.canGoLinkIdIngestDayNext()) {
      this.linkIdIngestDayPageIndex += 1;
    }
  }

  canGoLinkIdIngestSourcePrev(): boolean {
    return this.linkIdIngestSourcePageIndex > 0;
  }

  canGoLinkIdIngestSourceNext(): boolean {
    return this.linkIdIngestSourcePageIndex < this.pageCountFor(this.linkIdIngestSourceRows().length) - 1;
  }

  goLinkIdIngestSourcePrev(): void {
    if (this.canGoLinkIdIngestSourcePrev()) {
      this.linkIdIngestSourcePageIndex -= 1;
    }
  }

  goLinkIdIngestSourceNext(): void {
    if (this.canGoLinkIdIngestSourceNext()) {
      this.linkIdIngestSourcePageIndex += 1;
    }
  }

  formatIngestDate(value: string): string {
    if (!value || value === "total") {
      return "Total";
    }

    return new Date(`${value}T00:00:00`).toLocaleDateString();
  }

  formatIngestBucket(count: number, percent: number): string {
    return formatIngestCountWithPercent(count, percent);
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
    } else {
      this.sortStates[table] = {
        key,
        direction: current.direction === "asc" ? "desc" : "asc",
      };
    }

    if (table === "linkIdIngest") {
      this.linkIdIngestDayPageIndex = 0;
    }

    if (table === "linkIdIngestSource") {
      this.linkIdIngestSourcePageIndex = 0;
    }
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

  trackByIngestDate(_: number, item: LinkIdIngestDailyRow): string {
    return item.date;
  }

  trackByIngestSource(_: number, item: LinkIdIngestSourceRow): string {
    return item.sourceSystemName;
  }

  private syncLinkIdIngestSourceSelection(previouslyAllSelected: boolean): void {
    const options = extractLinkIdIngestSourceOptions(this.dashboard.linkage.linkIdIngestTrend || []);
    this.selectedLinkIdIngestSources = syncSelectedSources(
      this.selectedLinkIdIngestSources,
      options,
      previouslyAllSelected,
      !this.linkIdIngestSourcesInitialized,
    );
    this.linkIdIngestSourcesInitialized = true;
    this.resetLinkIdIngestPages();
    this.refreshLinkIdIngestChartData();
  }

  private refreshLinkIdIngestChartData(): void {
    const view = this.linkIdIngestView();
    if (!view.dailyRows.length || this.selectedLinkIdIngestSources.length === 0) {
      this.linkIdIngestChartPoints = [];
      return;
    }

    const includeBreakdown = this.selectedLinkIdIngestSources.length !== 1;
    const lookbackRows = filterTrendRowsByLookback(
      this.dashboard.linkage.linkIdIngestTrend || [],
      this.selectedLinkIdIngestLookbackDays,
    );

    this.linkIdIngestChartPoints = buildLinkIdIngestChartData(
      view.dailyRows,
      lookbackRows,
      this.selectedLinkIdIngestSources,
      includeBreakdown,
    );
  }

  private isLinkIdIngestAllSourcesSelectedAgainst(options: string[], selected: string[]): boolean {
    return options.length > 0
      && selected.length === options.length
      && options.every((source) => selected.includes(source));
  }

  private resetLinkIdIngestPages(): void {
    this.linkIdIngestDayPageIndex = 0;
    this.linkIdIngestSourcePageIndex = 0;
  }

  private pageCountFor(totalCount: number): number {
    return totalCount === 0 ? 0 : Math.ceil(totalCount / this.linkIdIngestPageSize);
  }

  private pagerLabel(totalCount: number, pageIndex: number, unit: string): string {
    if (totalCount === 0) {
      return `Showing 0 of 0 ${unit}`;
    }

    const start = pageIndex * this.linkIdIngestPageSize + 1;
    const end = Math.min(totalCount, (pageIndex + 1) * this.linkIdIngestPageSize);
    return `Showing ${start}–${end} of ${totalCount} ${unit}`;
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

  private linkIdIngestSortValue(row: LinkIdIngestDailyRow, key: string): string | number {
    switch (key) {
      case "incomingRecords": return row.incomingRecords;
      case "newPersonRecords": return row.newPersonRecords;
      case "alreadyInMpiRecords": return row.alreadyInMpiRecords;
      case "newPersonPercent": return row.newPersonPercent;
      case "alreadyInMpiPercent": return row.alreadyInMpiPercent;
      default: return row.date;
    }
  }

  private linkIdIngestSourceSortValue(row: LinkIdIngestSourceRow, key: string): string | number {
    switch (key) {
      case "incomingRecords": return row.incomingRecords;
      case "newPersonRecords": return row.newPersonRecords;
      case "alreadyInMpiRecords": return row.alreadyInMpiRecords;
      case "alreadyInMpiPercent": return row.alreadyInMpiPercent;
      default: return row.sourceSystemName;
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
        linkIdIngestTrend: [],
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
