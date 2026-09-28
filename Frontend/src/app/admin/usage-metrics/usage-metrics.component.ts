import { Component, OnInit } from "@angular/core";
import { ActivatedRoute } from "@angular/router";
import { AdminUsageMetricsResponse, BatchFileRow, BatchIntakeReliabilityReport, BatchSourceSummaryRow, CommonApiService } from "src/app/services/common-api.service";
import { AdminExportDialogService, AdminExportSectionId } from "src/app/services/admin-export-dialog.service";
import { MetricChartSeries } from "src/app/shared/metric-chart/metric-chart.component";
import { buildCombinedMetrics, CombinedUsageRow } from "./admin-usage-combined.util";

type UsageSortKey = "system" | "callCount" | "successCount" | "failedCount" | "failureRate" | "sourceIpCount" | "firstSeen" | "lastSeen";
type BatchSortKey = "sourceSystemName" | "totalFiles" | "failedFiles" | "rejectedRecords" | "averageProcessingMinutes";
type BatchFileSortKey = "requestId" | "fileName" | "sourceSystemName" | "recordsCount" | "status" | "processingMinutes" | "rejectCount" | "requestDateTime";
type UsageSortDirection = "asc" | "desc";
type UsageMode = "combined" | "api" | "batch";

@Component({
  selector: "app-usage-metrics",
  templateUrl: "./usage-metrics.component.html",
  styleUrls: ["./usage-metrics.component.scss"],
})
export class UsageMetricsComponent implements OnInit {
  loading = false;
  loadError = "";
  search = "";
  tenantFilter = "all";
  mode: UsageMode = "api";
  selectedLookbackHours = 24;
  usageMetrics: AdminUsageMetricsResponse = this.emptyResponse();
  batchMetrics: BatchIntakeReliabilityReport = this.emptyBatchResponse();
  combinedMetrics: CombinedUsageRow[] = [];
  sortKey: UsageSortKey = "callCount";
  sortDirection: UsageSortDirection = "desc";
  batchSortKey: BatchSortKey = "totalFiles";
  batchSortDirection: UsageSortDirection = "desc";
  batchFileSortKey: BatchFileSortKey = "requestDateTime";
  batchFileSortDirection: UsageSortDirection = "desc";
  expandedRawCertificateSystems = new Set<string>();

  readonly lookbackOptions = [
    { label: "1 hour", value: 1 },
    { label: "6 hours", value: 6 },
    { label: "24 hours", value: 24 },
    { label: "Weekly", value: 24 * 7 },
    { label: "Monthly", value: 24 * 30 },
    { label: "3 Months", value: 24 * 90 },
    { label: "6 Months", value: 24 * 180 },
    { label: "Yearly", value: 24 * 365 },
  ];
  readonly usageVolumeSeries: MetricChartSeries[] = [
    { key: "callCount", label: "Call Count", color: "#0b84f3" },
  ];
  readonly failureRateSeries: MetricChartSeries[] = [
    { key: "failureRate", label: "Failure Rate %", color: "#d14343" },
  ];
  readonly sourceIpSeries: MetricChartSeries[] = [
    { key: "sourceIpCount", label: "Distinct Source IPs", color: "#00a389" },
  ];
  readonly batchVolumeSeries: MetricChartSeries[] = [
    { key: "totalFiles", label: "Files", color: "#0b84f3" },
  ];
  readonly batchFailureSeries: MetricChartSeries[] = [
    { key: "failedFiles", label: "Failed Files", color: "#d14343" },
  ];
  readonly batchRejectSeries: MetricChartSeries[] = [
    { key: "rejectedRecords", label: "Rejected Records", color: "#f39c12" },
  ];

  constructor(
    private commonApiService: CommonApiService,
    private route: ActivatedRoute,
    private adminExportDialog: AdminExportDialogService,
  ) {}

  ngOnInit(): void {
    this.route.data.subscribe((data) => {
      this.mode = data["usageMode"] === "combined"
        ? "combined"
        : data["usageMode"] === "batch"
          ? "batch"
          : "api";
      this.search = "";
      this.tenantFilter = "all";
      this.loadCurrentMode();
    });
  }

  filteredSystems() {
    const normalized = this.search.trim().toLowerCase();
    return this.usageMetrics.systems
    .filter((system) => {
      const matchesSearch = !normalized || [
        system.system,
        system.tenant,
        system.rawCertificateCn,
        String(system.callCount),
        String(system.failedCount),
        String(system.successCount),
        String(system.failureRate),
        String(system.sourceIpCount),
      ].join(" ").toLowerCase().includes(normalized);
      const matchesTenant = this.tenantFilter === "all" || system.tenant === this.tenantFilter;
      return matchesSearch && matchesTenant;
    })
    .sort((left, right) => this.compareSystems(left, right));
  }

  filteredCombinedSystems(): CombinedUsageRow[] {
    const normalized = this.search.trim().toLowerCase();
    return this.combinedMetrics
      .filter((system) => {
        const matchesSearch = !normalized || [
          system.system,
          system.tenant,
          system.rawCertificateCn,
          String(system.callCount),
          String(system.apiCallCount),
          String(system.batchRecordCount),
          String(system.failedCount),
          String(system.successCount),
          String(system.failureRate),
          String(system.sourceIpCount),
        ].join(" ").toLowerCase().includes(normalized);
        const matchesTenant = this.tenantFilter === "all" || system.tenant === this.tenantFilter;
        return matchesSearch && matchesTenant;
      })
      .sort((left, right) => this.compareSystems(left, right));
  }

  filteredBatchSources(): BatchSourceSummaryRow[] {
    const normalized = this.search.trim().toLowerCase();
    return this.batchMetrics.sourceSummaries
      .filter((source) => {
        const matchesSearch = !normalized || [
          source.sourceSystemName,
          source.tenant,
          String(source.totalFiles),
          String(source.failedFiles),
          String(source.rejectedRecords),
          String(source.averageProcessingMinutes),
        ].join(" ").toLowerCase().includes(normalized);
        const matchesTenant = this.tenantFilter === "all" || source.tenant === this.tenantFilter;
        return matchesSearch && matchesTenant;
      })
      .sort((left, right) => this.compareBatchSources(left, right));
  }

  filteredBatchFiles(): BatchFileRow[] {
    const normalized = this.search.trim().toLowerCase();
    return this.batchMetrics.files
      .filter((file) => {
        const matchesSearch = !normalized || [
          file.requestId,
          file.fileName,
          file.sourceSystemName,
          file.tenant,
          file.status,
          file.topErrorMessage,
          String(file.recordsCount),
          String(file.rejectCount),
          String(file.processingMinutes),
        ].join(" ").toLowerCase().includes(normalized);
        const matchesTenant = this.tenantFilter === "all" || file.tenant === this.tenantFilter;
        return matchesSearch && matchesTenant;
      })
      .sort((left, right) => this.compareBatchFiles(left, right));
  }

  tenantOptions(): string[] {
    const tenants = this.mode === "batch"
      ? [...this.batchMetrics.sourceSummaries.map((item) => item.tenant), ...this.batchMetrics.files.map((item) => item.tenant)]
      : this.mode === "combined"
        ? this.combinedMetrics.map((item) => item.tenant)
        : this.usageMetrics.systems.map((item) => item.tenant);

    return [...new Set(tenants.filter((tenant) => !!tenant).sort((left, right) => left.localeCompare(right)))];
  }

  onLookbackChange(value: string): void {
    this.selectedLookbackHours = Number(value);
    this.loadCurrentMode();
  }

  refresh(): void {
    this.loadCurrentMode();
  }

  openExportDialog(): void {
    const sectionId: AdminExportSectionId = this.mode === "combined"
      ? "combined-usage"
      : this.mode === "batch"
        ? "batch-usage"
        : "api-usage";
    this.adminExportDialog.open({ preselected: [sectionId] });
  }

  lastUpdatedLabel(): string {
    const timestamp = this.mode === "api" || this.mode === "combined"
      ? this.usageMetrics.lastUpdated
      : new Date().toISOString();
    return new Date(timestamp).toLocaleString();
  }

  sortBy(key: UsageSortKey): void {
    if (this.sortKey === key) {
      this.sortDirection = this.sortDirection === "asc" ? "desc" : "asc";
      return;
    }

    this.sortKey = key;
    this.sortDirection = key === "system" ? "asc" : "desc";
  }

  sortIndicator(key: UsageSortKey): string {
    if (this.sortKey !== key) {
      return "";
    }

    return this.sortDirection === "asc" ? "↑" : "↓";
  }

  sortBatchBy(key: BatchSortKey): void {
    if (this.batchSortKey === key) {
      this.batchSortDirection = this.batchSortDirection === "asc" ? "desc" : "asc";
      return;
    }

    this.batchSortKey = key;
    this.batchSortDirection = key === "sourceSystemName" ? "asc" : "desc";
  }

  sortBatchIndicator(key: BatchSortKey): string {
    if (this.batchSortKey !== key) {
      return "";
    }

    return this.batchSortDirection === "asc" ? "↑" : "↓";
  }

  sortBatchFilesBy(key: BatchFileSortKey): void {
    if (this.batchFileSortKey === key) {
      this.batchFileSortDirection = this.batchFileSortDirection === "asc" ? "desc" : "asc";
      return;
    }

    this.batchFileSortKey = key;
    this.batchFileSortDirection = key === "requestId" || key === "fileName" || key === "sourceSystemName" || key === "status"
      ? "asc"
      : "desc";
  }

  sortBatchFileIndicator(key: BatchFileSortKey): string {
    if (this.batchFileSortKey !== key) {
      return "";
    }

    return this.batchFileSortDirection === "asc" ? "↑" : "↓";
  }

  topCallVolumeChart() {
    const systems = this.mode === "combined" ? this.filteredCombinedSystems() : this.filteredSystems();

    return systems
      .slice()
      .sort((left, right) => right.callCount - left.callCount)
      .slice(0, 8)
      .reverse()
      .map((system, index) => ({
        timestamp: this.chartTimestamp(index),
        label: system.system,
        callCount: system.callCount,
      }));
  }

  topFailureRateChart() {
    const systems = this.mode === "combined" ? this.filteredCombinedSystems() : this.filteredSystems();

    return systems
      .filter((system) => system.callCount > 0)
      .slice()
      .sort((left, right) => right.failureRate - left.failureRate)
      .slice(0, 8)
      .reverse()
      .map((system, index) => ({
        timestamp: this.chartTimestamp(index),
        label: system.system,
        failureRate: system.failureRate,
      }));
  }

  topSourceIpChart() {
    const systems = this.mode === "combined" ? this.filteredCombinedSystems() : this.filteredSystems();

    return systems
      .slice()
      .sort((left, right) => right.sourceIpCount - left.sourceIpCount)
      .slice(0, 8)
      .reverse()
      .map((system, index) => ({
        timestamp: this.chartTimestamp(index),
        label: system.system,
        sourceIpCount: system.sourceIpCount,
      }));
  }

  topBatchVolumeChart() {
    return this.filteredBatchSources()
      .slice()
      .sort((left, right) => right.totalFiles - left.totalFiles)
      .slice(0, 8)
      .reverse()
      .map((source, index) => ({
        timestamp: this.chartTimestamp(index),
        label: source.sourceSystemName,
        totalFiles: source.totalFiles,
      }));
  }

  topBatchFailureChart() {
    return this.filteredBatchSources()
      .slice()
      .sort((left, right) => right.failedFiles - left.failedFiles)
      .slice(0, 8)
      .reverse()
      .map((source, index) => ({
        timestamp: this.chartTimestamp(index),
        label: source.sourceSystemName,
        failedFiles: source.failedFiles,
      }));
  }

  topBatchRejectChart() {
    return this.filteredBatchSources()
      .slice()
      .sort((left, right) => right.rejectedRecords - left.rejectedRecords)
      .slice(0, 8)
      .reverse()
      .map((source, index) => ({
        timestamp: this.chartTimestamp(index),
        label: source.sourceSystemName,
        rejectedRecords: source.rejectedRecords,
      }));
  }

  totalBatchRecords(): number {
    return this.batchMetrics.files.reduce((total, file) => total + file.recordsCount, 0);
  }

  combinedSystemsSeen(): number {
    return this.filteredCombinedSystems().length;
  }

  combinedTotalCalls(): number {
    return this.filteredCombinedSystems().reduce((total, item) => total + item.callCount, 0);
  }

  combinedApiCalls(): number {
    return this.filteredCombinedSystems().reduce((total, item) => total + item.apiCallCount, 0);
  }

  combinedBatchRecords(): number {
    return this.filteredCombinedSystems().reduce((total, item) => total + item.batchRecordCount, 0);
  }

  combinedFailures(): number {
    return this.filteredCombinedSystems().reduce((total, item) => total + item.failedCount, 0);
  }

  chartLabels(data: Array<{ label: string }>): string {
    return data.map((item) => item.label).join(" • ");
  }

  trackBySystem(_: number, item: { system: string; tenant?: string }): string {
    return `${item.system}-${item.tenant || ""}`;
  }

  trackByBatchSource(_: number, item: BatchSourceSummaryRow): string {
    return `${item.sourceSystemName}-${item.tenant}`;
  }

  trackByBatchFile(_: number, item: BatchFileRow): string {
    return `${item.requestId}-${item.fileName}-${item.sourceSystemName}`;
  }

  rawCertificateEntries(rawCertificateCn?: string | null): string[] {
    if (!rawCertificateCn) {
      return [];
    }

    return rawCertificateCn
      .split(", ")
      .map((item) => item.trim())
      .filter((item) => !!item);
  }

  visibleRawCertificateEntries(system: AdminUsageMetricsResponse["systems"][number]): string[] {
    const entries = this.rawCertificateEntries(system.rawCertificateCn);
    if (this.isRawCertificateExpanded(system) || entries.length <= 1) {
      return entries;
    }

    return entries.slice(0, 1);
  }

  isRawCertificateExpanded(system: AdminUsageMetricsResponse["systems"][number]): boolean {
    return this.expandedRawCertificateSystems.has(`${system.system}|${system.tenant || ""}`);
  }

  canToggleRawCertificate(system: AdminUsageMetricsResponse["systems"][number]): boolean {
    return this.rawCertificateEntries(system.rawCertificateCn).length > 1;
  }

  remainingRawCertificateCount(system: AdminUsageMetricsResponse["systems"][number]): number {
    return Math.max(this.rawCertificateEntries(system.rawCertificateCn).length - 1, 0);
  }

  toggleRawCertificate(system: AdminUsageMetricsResponse["systems"][number]): void {
    const key = `${system.system}|${system.tenant || ""}`;
    if (this.isRawCertificateExpanded(system)) {
      this.expandedRawCertificateSystems.delete(key);
      return;
    }

    this.expandedRawCertificateSystems.add(key);
  }

  private loadCurrentMode(): void {
    if (this.mode === "batch") {
      this.loadBatchMetrics();
      return;
    }

    if (this.mode === "combined") {
      this.loadCombinedMetrics();
      return;
    }

    this.loadUsageMetrics();
  }

  private loadUsageMetrics(): void {
    this.loading = true;
    this.loadError = "";

    this.commonApiService.getAdminUsageMetrics(this.selectedLookbackHours).subscribe({
      next: (response) => {
        this.usageMetrics = response;
        this.loading = false;
        this.loadError = response.message || "";
      },
      error: () => {
        this.loading = false;
        this.loadError = "Unable to load usage metrics.";
        this.usageMetrics = this.emptyResponse();
      }
    });
  }

  private loadBatchMetrics(): void {
    this.loading = true;
    this.loadError = "";

    this.commonApiService.getAdminBatchIntakeReliability(this.selectedLookbackDays()).subscribe({
      next: (response) => {
        this.batchMetrics = response;
        this.loading = false;
      },
      error: () => {
        this.loading = false;
        this.loadError = "Unable to load batch file usage metrics.";
        this.batchMetrics = this.emptyBatchResponse();
      }
    });
  }

  private loadCombinedMetrics(): void {
    this.loading = true;
    this.loadError = "";

    let pending = 2;
    let apiResponse: AdminUsageMetricsResponse | null = null;
    let batchResponse: BatchIntakeReliabilityReport | null = null;
    let failed = false;

    const finishIfReady = () => {
      if (failed || pending > 0 || !apiResponse || !batchResponse) {
        return;
      }

      this.usageMetrics = apiResponse;
      this.batchMetrics = batchResponse;
      this.combinedMetrics = buildCombinedMetrics(apiResponse, batchResponse);
      this.loading = false;
    };

    const fail = (message: string) => {
      if (failed) {
        return;
      }

      failed = true;
      this.loading = false;
      this.loadError = message;
      this.usageMetrics = this.emptyResponse();
      this.batchMetrics = this.emptyBatchResponse();
      this.combinedMetrics = [];
    };

    this.commonApiService.getAdminUsageMetrics(this.selectedLookbackHours).subscribe({
      next: (response) => {
        apiResponse = response;
        pending -= 1;
        finishIfReady();
      },
      error: () => fail("Unable to load combined API usage metrics.")
    });

    this.commonApiService.getAdminBatchIntakeReliability(this.selectedLookbackDays()).subscribe({
      next: (response) => {
        batchResponse = response;
        pending -= 1;
        finishIfReady();
      },
      error: () => fail("Unable to load combined batch usage metrics.")
    });
  }

  private emptyResponse(): AdminUsageMetricsResponse {
    return {
      lookbackHours: this.selectedLookbackHours,
      totalSystems: 0,
      totalCalls: 0,
      totalSuccessCount: 0,
      totalFailedCount: 0,
      totalDistinctSourceIps: 0,
      averageFailureRate: 0,
      lastUpdated: new Date().toISOString(),
      systems: [],
    };
  }

  private emptyBatchResponse(): BatchIntakeReliabilityReport {
    return {
      totalFiles: 0,
      failedFiles: 0,
      rejectedRecords: 0,
      averageProcessingMinutes: 0,
      files: [],
      sourceSummaries: [],
      topErrors: [],
    };
  }

  private compareSystems(left: AdminUsageMetricsResponse["systems"][number], right: AdminUsageMetricsResponse["systems"][number]): number {
    const direction = this.sortDirection === "asc" ? 1 : -1;

    switch (this.sortKey) {
      case "system":
        return left.system.localeCompare(right.system) * direction;
      case "callCount":
        return (left.callCount - right.callCount) * direction;
      case "successCount":
        return (left.successCount - right.successCount) * direction;
      case "failedCount":
        return (left.failedCount - right.failedCount) * direction;
      case "failureRate":
        return (left.failureRate - right.failureRate) * direction;
      case "sourceIpCount":
        return (left.sourceIpCount - right.sourceIpCount) * direction;
      case "firstSeen":
        return ((new Date(left.firstSeen || 0).getTime()) - (new Date(right.firstSeen || 0).getTime())) * direction;
      case "lastSeen":
        return ((new Date(left.lastSeen || 0).getTime()) - (new Date(right.lastSeen || 0).getTime())) * direction;
      default:
        return 0;
    }
  }

  private compareBatchSources(left: BatchSourceSummaryRow, right: BatchSourceSummaryRow): number {
    const direction = this.batchSortDirection === "asc" ? 1 : -1;

    switch (this.batchSortKey) {
      case "sourceSystemName":
        return left.sourceSystemName.localeCompare(right.sourceSystemName) * direction;
      case "totalFiles":
        return (left.totalFiles - right.totalFiles) * direction;
      case "failedFiles":
        return (left.failedFiles - right.failedFiles) * direction;
      case "rejectedRecords":
        return (left.rejectedRecords - right.rejectedRecords) * direction;
      case "averageProcessingMinutes":
        return (left.averageProcessingMinutes - right.averageProcessingMinutes) * direction;
      default:
        return 0;
    }
  }

  private compareBatchFiles(left: BatchFileRow, right: BatchFileRow): number {
    const direction = this.batchFileSortDirection === "asc" ? 1 : -1;

    switch (this.batchFileSortKey) {
      case "requestId":
        return left.requestId.localeCompare(right.requestId) * direction;
      case "fileName":
        return left.fileName.localeCompare(right.fileName) * direction;
      case "sourceSystemName":
        return left.sourceSystemName.localeCompare(right.sourceSystemName) * direction;
      case "recordsCount":
        return (left.recordsCount - right.recordsCount) * direction;
      case "status":
        return left.status.localeCompare(right.status) * direction;
      case "processingMinutes":
        return (left.processingMinutes - right.processingMinutes) * direction;
      case "rejectCount":
        return (left.rejectCount - right.rejectCount) * direction;
      case "requestDateTime":
        return ((new Date(left.requestDateTime || 0).getTime()) - (new Date(right.requestDateTime || 0).getTime())) * direction;
      default:
        return 0;
    }
  }

  private selectedLookbackDays(): number {
    return Math.max(1, Math.ceil(this.selectedLookbackHours / 24));
  }

  private chartTimestamp(index: number): string {
    return new Date(2026, 0, index + 1).toISOString();
  }
}
