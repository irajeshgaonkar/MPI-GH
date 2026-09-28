import { Component, ElementRef, OnDestroy, OnInit, ViewChild } from "@angular/core";
import { Subject, interval, of } from "rxjs";
import { catchError, map, startWith, switchMap, takeUntil } from "rxjs/operators";
import { AdminAlert, AdminConfiguredThresholds, AdminCustomErrorEntry, AdminDashboardResponse, AdminVeratoErrorEntry, CommonApiService } from "../../services/common-api.service";
import { PdfExportService } from "../../services/pdf-export.service";
import { MetricChartSeries } from "../../shared/metric-chart/metric-chart.component";

type AdminStatus = "Healthy" | "Warning" | "Critical";

interface SummaryCard {
  label: string;
  value: string;
  status: AdminStatus;
  detail: string;
  statusReason: string;
}

interface HealthIndicator {
  label: string;
  value: string;
  status: AdminStatus;
  detail: string;
  statusReason: string;
}

interface TooltipCard extends SummaryCard {
  tooltip: string;
}

interface TooltipIndicator extends HealthIndicator {
  tooltip: string;
}

@Component({
  selector: "app-admin-console",
  templateUrl: "./admin-console.component.html",
  styleUrls: ["./admin-console.component.scss"],
})
export class AdminConsoleComponent implements OnInit, OnDestroy {
  @ViewChild("exportContent") exportContent?: ElementRef<HTMLElement>;

  readonly refreshIntervalMs = 30000;
  readonly destroy$ = new Subject<void>();
  readonly lookbackOptions = [
    { label: "15 minutes", value: 15 },
    { label: "30 minutes", value: 30 },
    { label: "1 hour", value: 60 },
    { label: "3 hours", value: 180 },
    { label: "6 hours", value: 360 },
    { label: "24 hours", value: 1440 },
  ];

  loading = false;
  manualRefreshing = false;
  exporting = false;
  usingFallbackData = false;
  loadError = "";
  deepDiveOpen = false;
  veratoDeepDiveOpen = false;
  selectedLookbackMinutes = 60;

  dashboard: AdminDashboardResponse = this.buildFallbackDashboard();
  latencySeries: MetricChartSeries[] = [
    { key: "avg", label: "Avg Latency", color: "#0b84f3" },
    { key: "p95", label: "P95 Latency", color: "#ff7f50" },
  ];
  throughputSeries: MetricChartSeries[] = [
    { key: "rpm", label: "Requests / Min", color: "#00a389" },
  ];
  errorSeries: MetricChartSeries[] = [
    { key: "4xx", label: "4xx", color: "#f3b43f" },
    { key: "5xx", label: "5xx", color: "#d14343" },
  ];
  timeoutSeries: MetricChartSeries[] = [
    { key: "count", label: "Timeouts", color: "#a75cf5" },
  ];
  concurrencySeries: MetricChartSeries[] = [
    { key: "used", label: "Concurrency", color: "#155a8a" },
  ];
  veratoErrorSeries: MetricChartSeries[] = [
    { key: "count", label: "Failed Requests", color: "#c94b2d" },
  ];
  readonly sectionTooltips: Record<string, string> = {
    systemHealth: "A leadership snapshot of overall MPI availability, responsiveness, error pressure, traffic volume, and dependency behavior in the selected time window.",
    apiPerformance: "These charts show whether response time or traffic demand is rising, stable, or degrading across the selected lookback window.",
    errorTimeouts: "Use this section to spot reliability deterioration. 4xx reflects client-side issues, 5xx reflects server-side failures, and timeout spikes indicate requests not finishing in time.",
    infrastructure: "This section summarizes platform condition across API Gateway, Lambda execution pressure, downstream dependency response time, and timeout posture.",
    customErrors: "MPI custom error logs show application-level failures captured in CloudWatch Logs with function name, error code, message, and tracking reference for faster triage.",
    veratoErrors: "Verato errors come from failed Verato-related entries in user request processing. Use this section to see how often downstream Verato operations are failing, which operations are affected, and which request traces need investigation.",
    alerts: "Operational signals surfaced by the backend to call attention to unusual degradation, spikes, or threshold breaches."
  };
  readonly chartTooltips: Record<string, string> = {
    latency: "Average latency shows the typical response time. P95 latency shows the slower edge of experience, where 95% of requests complete at or below this value.",
    throughput: "Requests per minute shows current traffic load and helps separate demand spikes from actual service degradation.",
    errors: "4xx errors usually indicate caller or request issues. 5xx errors indicate failures inside MPI or its dependencies.",
    timeouts: "Timeout count shows requests that took too long to complete and were terminated or considered failed.",
    healthMix: "This donut groups the current infrastructure indicators into healthy, warning, and critical states for a fast overall posture check.",
    concurrency: "Concurrency shows how many Lambda executions are active at the same time, which helps indicate scaling pressure or saturation risk.",
    veratoErrors: "This chart shows the volume of failed Verato operations over time so leadership can see whether downstream problems are isolated or sustained."
  };

  constructor(
    private commonApiService: CommonApiService,
    private pdfExportService: PdfExportService,
  ) {}

  ngOnInit(): void {
    this.loading = true;
    interval(this.refreshIntervalMs)
      .pipe(
        startWith(0),
        switchMap(() => this.fetchDashboard().pipe(
          map((response) => ({ response, isFallback: false })),
          catchError(() => of({
            response: this.buildFallbackDashboard(),
            isFallback: true,
          }))
        )),
        takeUntil(this.destroy$)
      )
      .subscribe(({ response, isFallback }) => {
        this.dashboard = response;
        this.usingFallbackData = isFallback;
        this.loadError = isFallback ? "Live admin metrics are unavailable. Showing sample operational data." : "";
        this.loading = false;
        this.manualRefreshing = false;
      });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  refreshNow(): void {
    this.manualRefreshing = true;
    this.fetchDashboard()
      .pipe(
        map((response) => ({ response, isFallback: false })),
        catchError(() => of({
          response: this.buildFallbackDashboard(),
          isFallback: true,
        })),
        takeUntil(this.destroy$)
      )
      .subscribe(({ response, isFallback }) => {
        this.dashboard = response;
        this.usingFallbackData = isFallback;
        this.loadError = isFallback ? "Live admin metrics are unavailable. Showing sample operational data." : "";
        this.manualRefreshing = false;
      });
  }

  async exportHealthReport(): Promise<void> {
    if (!this.exportContent?.nativeElement || this.exporting) {
      return;
    }

    this.exporting = true;
    try {
      const safeDate = new Date().toISOString().slice(0, 10);
      await this.pdfExportService.exportElement(this.exportContent.nativeElement, {
        fileName: `mpi-health-${safeDate}.pdf`,
        orientation: "landscape",
        title: "MPI Health Report",
        metadata: this.exportMetadata(),
      });
    } finally {
      this.exporting = false;
    }
  }

  onLookbackChange(value: number): void {
    this.selectedLookbackMinutes = Number(value);
    this.manualRefreshing = true;
    this.refreshNow();
  }

  selectedLookbackLabel(): string {
    return this.lookbackOptions.find((item) => item.value === this.selectedLookbackMinutes)?.label || `${this.selectedLookbackMinutes} minutes`;
  }

  summaryCards(): TooltipCard[] {
    const summary = this.dashboard.summary;
    return [
      {
        label: "API Availability",
        value: summary.healthStatus,
        status: this.healthStatus(summary.healthStatus),
        detail: "Overall MPI API health",
        statusReason: this.healthStatusReason(summary.healthStatus),
        tooltip: "An overall service health signal derived from availability, errors, timeouts, and latency indicators."
      },
      {
        label: "Average Latency",
        value: `${summary.avgLatencyMs} ms`,
        status: this.latencyStatus(summary.avgLatencyMs),
        detail: "Average end-to-end response time",
        statusReason: this.latencyStatusReason(summary.avgLatencyMs),
        tooltip: "The typical time it takes MPI to complete a request from start to finish during the selected window."
      },
      {
        label: "P95 Latency",
        value: `${summary.p95LatencyMs} ms`,
        status: this.latencyStatus(summary.p95LatencyMs),
        detail: "Tail latency across requests",
        statusReason: this.latencyStatusReason(summary.p95LatencyMs),
        tooltip: "The response-time level that 95% of requests are at or below. This is useful for spotting slower outlier experience."
      },
      {
        label: "Error Rate",
        value: `${summary.errorRate}%`,
        status: this.errorStatus(summary.errorRate),
        detail: "4xx/5xx rate over recent traffic",
        statusReason: this.errorStatusReason(summary.errorRate),
        tooltip: "The percentage of requests ending in 4xx or 5xx responses. Higher values indicate reliability or caller quality issues."
      },
      {
        label: "Throughput",
        value: `${summary.requestsPerMinute} rpm`,
        status: this.throughputStatus(summary.requestsPerMinute),
        detail: "Recent requests per minute",
        statusReason: this.throughputStatusReason(summary.requestsPerMinute),
        tooltip: "The recent rate of incoming requests. This helps show demand volume and whether spikes align with performance changes."
      },
      {
        label: "Lambda Concurrency",
        value: `${summary.lambdaConcurrency}`,
        status: this.concurrencyStatus(summary.lambdaConcurrency),
        detail: "Current concurrency utilization",
        statusReason: this.concurrencyStatusReason(summary.lambdaConcurrency),
        tooltip: "The number of Lambda executions running at the same time. Elevated levels can indicate scaling pressure."
      },
      {
        label: "Timeout Count",
        value: `${summary.timeoutCount}`,
        status: this.timeoutStatus(summary.timeoutCount),
        detail: "Recent timeout volume",
        statusReason: this.timeoutStatusReason(summary.timeoutCount),
        tooltip: "The number of requests that did not complete in time during the selected window."
      },
      {
        label: "Dependency Latency",
        value: `${summary.dependencyLatencyMs} ms`,
        status: this.latencyStatus(summary.dependencyLatencyMs),
        detail: "Verato / downstream latency",
        statusReason: this.latencyStatusReason(summary.dependencyLatencyMs),
        tooltip: "The response time of the key downstream service MPI depends on, such as Verato or another matching dependency."
      },
    ];
  }

  infrastructureIndicators(): TooltipIndicator[] {
    const summary = this.dashboard.summary;
    return [
      {
        label: "API Gateway",
        value: summary.healthStatus,
        status: this.healthStatus(summary.healthStatus),
        detail: "External API availability",
        statusReason: this.healthStatusReason(summary.healthStatus),
        tooltip: "Shows whether the public-facing API entry point appears healthy for callers."
      },
      {
        label: "MPI Lambda",
        value: `${summary.lambdaConcurrency} in use`,
        status: this.concurrencyStatus(summary.lambdaConcurrency),
        detail: "Execution concurrency pressure",
        statusReason: this.concurrencyStatusReason(summary.lambdaConcurrency),
        tooltip: "Shows current Lambda execution load and whether the service is approaching a concurrency pressure zone."
      },
      {
        label: "Downstream Dependency",
        value: `${summary.dependencyLatencyMs} ms`,
        status: this.latencyStatus(summary.dependencyLatencyMs),
        detail: "Dependency response health",
        statusReason: this.latencyStatusReason(summary.dependencyLatencyMs),
        tooltip: "Shows how quickly the main downstream dependency is responding, which directly affects MPI performance."
      },
      {
        label: "Timeouts",
        value: `${summary.timeoutCount}`,
        status: this.timeoutStatus(summary.timeoutCount),
        detail: "Timeout signal across service calls",
        statusReason: this.timeoutStatusReason(summary.timeoutCount),
        tooltip: "Shows whether timeout failures are currently present and whether they are isolated or elevated."
      },
    ];
  }

  statusBreakdown(): { healthy: number; warning: number; critical: number } {
    const indicators = this.infrastructureIndicators();
    return {
      healthy: indicators.filter((item) => item.status === "Healthy").length,
      warning: indicators.filter((item) => item.status === "Warning").length,
      critical: indicators.filter((item) => item.status === "Critical").length,
    };
  }

  donutStyle(): string {
    const breakdown = this.statusBreakdown();
    const total = breakdown.healthy + breakdown.warning + breakdown.critical || 1;
    const healthyPct = (breakdown.healthy / total) * 100;
    const warningPct = (breakdown.warning / total) * 100;
    const criticalPct = (breakdown.critical / total) * 100;
    const warningEnd = healthyPct + warningPct;

    return `conic-gradient(#00a389 0 ${healthyPct}%, #f3b43f ${healthyPct}% ${warningEnd}%, #d14343 ${warningEnd}% ${warningEnd + criticalPct}%)`;
  }

  statusClass(status: AdminStatus | string): string {
    const normalized = String(status).toLowerCase();
    return `status-pill status-pill--${normalized}`;
  }

  trackByAlert(_: number, item: AdminAlert): string {
    return `${item.severity}-${item.message}`;
  }

  trackByCustomError(_: number, item: AdminCustomErrorEntry): string {
    return `${item.timestamp || "unknown"}-${item.trackingId || "none"}-${item.errorCode || "uncoded"}-${item.functionName || "unknown"}-${item.name || "unnamed"}`;
  }

  trackByVeratoError(_: number, item: AdminVeratoErrorEntry): string {
    return `${item.timestamp}-${item.trackingId}-${item.apiCallType}`;
  }

  lastUpdatedLabel(): string {
    return new Date(this.dashboard.lastUpdated).toLocaleString();
  }

  customErrorEntries(): AdminCustomErrorEntry[] {
    return this.dashboard.customErrors.entries || [];
  }

  customErrorMetricCards(): Array<{ label: string; value: string; detail: string }> {
    const summary = this.dashboard.customErrors.summary;
    return [
      {
        label: "Custom Errors",
        value: `${summary.totalErrors}`,
        detail: "Matched log events in the selected time window",
      },
      {
        label: "Error Codes",
        value: `${summary.distinctErrorCodes}`,
        detail: "Distinct MPI custom error codes observed",
      },
      {
        label: "Code Functions",
        value: `${summary.distinctFunctions}`,
        detail: "Distinct application functions emitting custom errors",
      },
      {
        label: "Tracking IDs",
        value: `${summary.distinctTrackingIds}`,
        detail: "Unique request traces tied to the error events",
      },
    ];
  }

  customErrorStatusLabel(): AdminStatus {
    return this.customErrorStatus(this.dashboard.customErrors.summary.totalErrors);
  }

  formatCustomErrorTimestamp(value?: string | null): string {
    if (!value) {
      return "--";
    }

    return this.parseApiTimestamp(value).toLocaleString();
  }

  openCustomErrorDeepDive(): void {
    this.deepDiveOpen = true;
  }

  closeCustomErrorDeepDive(): void {
    this.deepDiveOpen = false;
  }

  openVeratoErrorDeepDive(): void {
    this.veratoDeepDiveOpen = true;
  }

  closeVeratoErrorDeepDive(): void {
    this.veratoDeepDiveOpen = false;
  }

  veratoErrorMetricCards(): Array<{ label: string; value: string; detail: string }> {
    const summary = this.dashboard.veratoErrors.summary;
    return [
      {
        label: "Failed Verato Requests",
        value: `${summary.totalErrors}`,
        detail: "Failed Verato operations in the selected time window",
      },
      {
        label: "Operation Types",
        value: `${summary.distinctOperations}`,
        detail: "Distinct Verato operation types that failed",
      },
      {
        label: "Tracking IDs",
        value: `${summary.distinctTrackingIds}`,
        detail: "Unique failed request traces tied to Verato operations",
      },
      {
        label: "Retry Attempts",
        value: `${summary.totalRetries}`,
        detail: "Retries already attempted across failed Verato requests",
      },
    ];
  }

  veratoErrorEntries(): AdminVeratoErrorEntry[] {
    return this.dashboard.veratoErrors.entries || [];
  }

  veratoErrorStatusLabel(): AdminStatus {
    return this.customErrorStatus(this.dashboard.veratoErrors.summary.totalErrors);
  }

  private healthStatus(value: string): AdminStatus {
    const normalized = value.toLowerCase();
    if (normalized === "critical") return "Critical";
    if (normalized === "warning") return "Warning";
    return "Healthy";
  }

  private healthStatusReason(value: string): string {
    const normalized = value.toLowerCase();
    if (normalized === "critical") return "Critical because backend health checks detected severe degradation.";
    if (normalized === "warning") return "Warning because backend health checks detected elevated risk signals.";
    return "Healthy because backend health checks did not detect active risk signals.";
  }

  private latencyStatus(value: number): AdminStatus {
    const thresholds = this.dashboard.thresholds;
    if (value >= thresholds.criticalLatencyMs) return "Critical";
    if (value >= thresholds.warningLatencyMs) return "Warning";
    return "Healthy";
  }

  private latencyStatusReason(value: number): string {
    const thresholds = this.dashboard.thresholds;
    if (value >= thresholds.criticalLatencyMs) return `Critical because latency is ${value} ms, above the ${thresholds.criticalLatencyMs} ms threshold.`;
    if (value >= thresholds.warningLatencyMs) return `Warning because latency is ${value} ms, above the ${thresholds.warningLatencyMs} ms threshold.`;
    return `Healthy because latency is ${value} ms, below the ${thresholds.warningLatencyMs} ms threshold.`;
  }

  private errorStatus(value: number): AdminStatus {
    const thresholds = this.dashboard.thresholds;
    if (value >= thresholds.criticalErrorRatePercent) return "Critical";
    if (value >= thresholds.warningErrorRatePercent) return "Warning";
    return "Healthy";
  }

  private errorStatusReason(value: number): string {
    const thresholds = this.dashboard.thresholds;
    if (value >= thresholds.criticalErrorRatePercent) return `Critical because error rate is ${value}%, above the ${thresholds.criticalErrorRatePercent}% threshold.`;
    if (value >= thresholds.warningErrorRatePercent) return `Warning because error rate is ${value}%, above the ${thresholds.warningErrorRatePercent}% threshold.`;
    return `Healthy because error rate is ${value}%, below the ${thresholds.warningErrorRatePercent}% threshold.`;
  }

  private throughputStatus(value: number): AdminStatus {
    if (value < 50) return "Warning";
    return "Healthy";
  }

  private throughputStatusReason(value: number): string {
    if (value < 50) return `Warning because throughput is ${value} rpm, below the 50 rpm baseline.`;
    return `Healthy because throughput is ${value} rpm, at or above the 50 rpm baseline.`;
  }

  private concurrencyStatus(value: number): AdminStatus {
    const thresholds = this.dashboard.thresholds;
    if (value >= thresholds.criticalConcurrency) return "Critical";
    if (value >= thresholds.warningConcurrency) return "Warning";
    return "Healthy";
  }

  private concurrencyStatusReason(value: number): string {
    const thresholds = this.dashboard.thresholds;
    if (value >= thresholds.criticalConcurrency) return `Critical because concurrency is ${value}, above the ${thresholds.criticalConcurrency} threshold.`;
    if (value >= thresholds.warningConcurrency) return `Warning because concurrency is ${value}, above the ${thresholds.warningConcurrency} threshold.`;
    return `Healthy because concurrency is ${value}, below the ${thresholds.warningConcurrency} threshold.`;
  }

  private customErrorStatus(value: number): AdminStatus {
    if (value >= 10) return "Critical";
    if (value > 0) return "Warning";
    return "Healthy";
  }

  private timeoutStatusReason(value: number): string {
    const thresholds = this.dashboard.thresholds;
    if (value >= thresholds.criticalTimeoutCount) return `Critical because timeout count is ${value}, above the threshold of ${thresholds.criticalTimeoutCount}.`;
    if (value >= thresholds.warningTimeoutCount) return `Warning because timeout count is ${value}, above the threshold of ${thresholds.warningTimeoutCount}.`;
    return "Healthy because no timeouts were observed in the selected window.";
  }

  private timeoutStatus(value: number): AdminStatus {
    const thresholds = this.dashboard.thresholds;
    if (value >= thresholds.criticalTimeoutCount) return "Critical";
    if (value >= thresholds.warningTimeoutCount) return "Warning";
    return "Healthy";
  }

  private buildFallbackDashboard(): AdminDashboardResponse {
    return {
      summary: {
        healthStatus: "Healthy",
        avgLatencyMs: 320,
        p95LatencyMs: 780,
        errorRate: 0.8,
        requestsPerMinute: 145,
        lambdaConcurrency: 42,
        timeoutCount: 3,
        dependencyLatencyMs: 410,
      },
      thresholds: this.defaultThresholds(),
      trends: {
        latency: [
          { timestamp: "2026-04-17T09:00:00Z", avg: 300, p95: 700 },
          { timestamp: "2026-04-17T09:05:00Z", avg: 340, p95: 790 },
          { timestamp: "2026-04-17T09:10:00Z", avg: 315, p95: 760 },
          { timestamp: "2026-04-17T09:15:00Z", avg: 360, p95: 840 },
        ],
        throughput: [
          { timestamp: "2026-04-17T09:00:00Z", rpm: 120 },
          { timestamp: "2026-04-17T09:05:00Z", rpm: 145 },
          { timestamp: "2026-04-17T09:10:00Z", rpm: 151 },
          { timestamp: "2026-04-17T09:15:00Z", rpm: 139 },
        ],
        errors: [
          { timestamp: "2026-04-17T09:00:00Z", "4xx": 1, "5xx": 0 },
          { timestamp: "2026-04-17T09:05:00Z", "4xx": 0, "5xx": 2 },
          { timestamp: "2026-04-17T09:10:00Z", "4xx": 1, "5xx": 1 },
          { timestamp: "2026-04-17T09:15:00Z", "4xx": 0, "5xx": 0 },
        ],
        timeouts: [
          { timestamp: "2026-04-17T09:00:00Z", count: 0 },
          { timestamp: "2026-04-17T09:05:00Z", count: 2 },
          { timestamp: "2026-04-17T09:10:00Z", count: 1 },
          { timestamp: "2026-04-17T09:15:00Z", count: 0 },
        ],
        concurrency: [
          { timestamp: "2026-04-17T09:00:00Z", used: 35 },
          { timestamp: "2026-04-17T09:05:00Z", used: 42 },
          { timestamp: "2026-04-17T09:10:00Z", used: 48 },
          { timestamp: "2026-04-17T09:15:00Z", used: 40 },
        ],
      },
      alerts: [
        {
          severity: "warning",
          message: "P95 latency increased by 18% in the last 15 minutes",
        },
        {
          severity: "healthy",
          message: "Request volume remains within expected operating range",
        },
      ],
      customErrors: {
        summary: {
          totalErrors: 3,
          distinctErrorCodes: 2,
          distinctFunctions: 2,
          distinctTrackingIds: 3,
        },
        entries: [
          {
            timestamp: "2026-04-17T09:14:12Z",
            name: "Identity Search Timeout",
            layer: "Application",
            functionName: "SearchIdentity",
            errorCode: "HCA-MPI-1001",
            message: "Unable to complete identity search because downstream match service timed out.",
            trackingId: "trk-12451",
            logStream: "2026/04/17/[$LATEST]example01",
            logGroup: "/aws/lambda/mpi-frontend-api-lambda",
          },
          {
            timestamp: "2026-04-17T09:08:47Z",
            name: "Invalid Link Payload",
            layer: "Application",
            functionName: "LinkIdentity",
            errorCode: "HCA-MPI-2204",
            message: "Link operation rejected due to inconsistent source system payload.",
            trackingId: "trk-12407",
            logStream: "2026/04/17/[$LATEST]example02",
            logGroup: "/aws/lambda/mpi-frontend-api-lambda",
          },
          {
            timestamp: "2026-04-17T09:03:05Z",
            name: "Identity Search Timeout",
            layer: "Integration",
            functionName: "SearchIdentity",
            errorCode: "HCA-MPI-1001",
            message: "Unable to complete identity search because downstream match service timed out.",
            trackingId: "trk-12398",
            logStream: "2026/04/17/[$LATEST]example03",
            logGroup: "/aws/lambda/mpi-frontend-api-lambda",
          },
        ],
      },
      veratoErrors: {
        summary: {
          totalErrors: 4,
          distinctOperations: 2,
          distinctTrackingIds: 4,
          totalRetries: 3,
        },
        trend: [
          { timestamp: "2026-04-17T09:00:00Z", count: 1, label: "9:00 AM" },
          { timestamp: "2026-04-17T09:15:00Z", count: 0, label: "9:15 AM" },
          { timestamp: "2026-04-17T09:30:00Z", count: 2, label: "9:30 AM" },
          { timestamp: "2026-04-17T09:45:00Z", count: 1, label: "9:45 AM" },
        ],
        entries: [
          {
            timestamp: "2026-04-17T09:44:49Z",
            trackingId: "MyWABenefits Post 2026-04-17T09:44:49.976 bca3b0b9-8720-4101-afae-f7b7d4980d6f",
            apiCallType: "VE VEPost",
            userName: "system",
            status: "Failed",
            message: "PostIdentityService failure: PostIdentityRequest failed due to stale object update during ingest.",
            retryCount: 0,
            responseJson: "{\"Success\":false,\"RetryableError\":false}",
          },
          {
            timestamp: "2026-04-17T09:31:11Z",
            trackingId: "trk-ve-1002",
            apiCallType: "VE Link",
            userName: "system",
            status: "Failed",
            message: "LinkIdentitiesRequest failed because Verato returned invalid input.",
            retryCount: 1,
            responseJson: "{\"Success\":false,\"Message\":\"Invalid input\"}",
          },
          {
            timestamp: "2026-04-17T09:29:03Z",
            trackingId: "trk-ve-1001",
            apiCallType: "VE VEPost",
            userName: "system",
            status: "Failed",
            message: "PostIdentityService failure caused by downstream ingest error.",
            retryCount: 1,
            responseJson: "{\"Success\":false,\"Message\":\"Error ingesting entity\"}",
          },
          {
            timestamp: "2026-04-17T09:02:15Z",
            trackingId: "trk-ve-0998",
            apiCallType: "VE Merge",
            userName: "system",
            status: "Failed",
            message: "MergeIdentitiesRequest failed after retry exhaustion.",
            retryCount: 1,
            responseJson: "{\"Success\":false,\"RetryableError\":true}",
          },
        ],
      },
      lastUpdated: "2026-04-17T09:15:00Z",
    };
  }

  private defaultThresholds(): AdminConfiguredThresholds {
    return {
      warningLatencyMs: 500,
      criticalLatencyMs: 1000,
      warningErrorRatePercent: 1,
      criticalErrorRatePercent: 3,
      warningTimeoutCount: 1,
      criticalTimeoutCount: 5,
      warningConcurrency: 50,
      criticalConcurrency: 80,
    };
  }

  private fetchDashboard() {
    return this.commonApiService.getAdminDashboard(this.selectedLookbackMinutes);
  }

  private exportMetadata(): Array<{ label: string; value: string }> {
    return [
      { label: "Generated", value: new Date().toLocaleString() },
      { label: "Exported By", value: localStorage.getItem("LoggedInUser") || "Unknown User" },
      { label: "Mode", value: "MPI Health" },
      { label: "Lookback", value: this.selectedLookbackLabel() },
      { label: "Data Source", value: this.usingFallbackData ? "Fallback sample data" : "Live operational metrics" },
      { label: "Last Updated", value: this.lastUpdatedLabel() },
      { label: "Health Status", value: this.dashboard.summary.healthStatus },
    ];
  }

  private parseApiTimestamp(value: string): Date {
    const normalized = /z$|[+-]\d{2}:\d{2}$/i.test(value) ? value : `${value}Z`;
    return new Date(normalized);
  }
}
