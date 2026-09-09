import { Component, OnDestroy, OnInit } from "@angular/core";
import { Observable, Subject } from "rxjs";
import { finalize, takeUntil } from "rxjs/operators";
import {
  AdminApiTrafficDayCount,
  AdminApiTrafficHourCount,
  AdminApiTrafficMetricResponse,
  AdminApiTrafficMonthCount,
  CommonApiService,
} from "src/app/services/common-api.service";
import { AdminExportDialogService } from "src/app/services/admin-export-dialog.service";
import { TrafficBarChartPoint } from "src/app/shared/traffic-bar-chart/traffic-bar-chart.component";

const TABLE_PAGE_SIZE = 10;

interface MetricSectionState<T> {
  items: T[];
  loading: boolean;
  error: string;
  lastUpdated: string | null;
}

@Component({
  selector: "app-traffic-analysis",
  templateUrl: "./traffic-analysis.component.html",
  styleUrls: ["./traffic-analysis.component.scss"],
})
export class TrafficAnalysisComponent implements OnInit, OnDestroy {
  selectedLookbackHours = 24;

  callsPerDay: MetricSectionState<AdminApiTrafficDayCount> = this.emptySection();
  busiestHours: MetricSectionState<AdminApiTrafficHourCount> = this.emptySection();
  busiestDays: MetricSectionState<AdminApiTrafficDayCount> = this.emptySection();
  busiestMonths: MetricSectionState<AdminApiTrafficMonthCount> = this.emptySection();

  /** Stable chart inputs — must not be recomputed in the template (breaks hover). */
  hoursChartData: TrafficBarChartPoint[] = [];
  daysChartData: TrafficBarChartPoint[] = [];
  monthsChartData: TrafficBarChartPoint[] = [];

  callsPage = 1;

  readonly pageSize = TABLE_PAGE_SIZE;
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

  private readonly destroy$ = new Subject<void>();
  private readonly lookbackLoadCancel$ = new Subject<void>();
  private readonly fixedLoadCancel$ = new Subject<void>();

  constructor(
    private commonApiService: CommonApiService,
    private adminExportDialog: AdminExportDialogService,
  ) {}

  ngOnInit(): void {
    this.loadAll(false);
  }

  openExportDialog(): void {
    this.adminExportDialog.open({ preselected: ["traffic-analysis"] });
  }

  ngOnDestroy(): void {
    this.lookbackLoadCancel$.next();
    this.lookbackLoadCancel$.complete();
    this.fixedLoadCancel$.next();
    this.fixedLoadCancel$.complete();
    this.destroy$.next();
    this.destroy$.complete();
  }

  onLookbackChange(value: string | number): void {
    this.selectedLookbackHours = Number(value);
    this.callsPage = 1;
    this.loadLookbackDependent(false);
  }

  refresh(): void {
    this.loadAll(true);
  }

  get anyLoading(): boolean {
    return (
      this.callsPerDay.loading ||
      this.busiestHours.loading ||
      this.busiestDays.loading ||
      this.busiestMonths.loading
    );
  }

  get showInitialLoader(): boolean {
    return this.anyLoading && !this.hasAnySectionData();
  }

  lastUpdatedLabel(): string {
    const timestamps = [
      this.callsPerDay.lastUpdated,
      this.busiestHours.lastUpdated,
      this.busiestDays.lastUpdated,
      this.busiestMonths.lastUpdated,
    ]
      .filter((value): value is string => !!value)
      .map((value) => new Date(value).getTime())
      .filter((value) => !Number.isNaN(value));

    if (!timestamps.length) {
      return "N/A";
    }

    return new Date(Math.max(...timestamps)).toLocaleString();
  }

  sortedCallsPerDay(): AdminApiTrafficDayCount[] {
    return [...(this.callsPerDay.items || [])].sort(
      (left, right) => new Date(right.day).getTime() - new Date(left.day).getTime()
    );
  }

  pagedCallsPerDay(): AdminApiTrafficDayCount[] {
    return this.pageSlice(this.sortedCallsPerDay(), this.callsPage);
  }

  callsTotalPages(): number {
    return this.totalPages(this.sortedCallsPerDay().length);
  }

  showCallsPager(): boolean {
    return this.sortedCallsPerDay().length > this.pageSize;
  }

  prevCallsPage(): void {
    this.callsPage = Math.max(1, this.callsPage - 1);
  }

  nextCallsPage(): void {
    this.callsPage = Math.min(this.callsTotalPages(), this.callsPage + 1);
  }

  formatDayLabel(value: string): string {
    return this.parseUtcDate(value).toLocaleDateString([], {
      month: "short",
      day: "numeric",
    });
  }

  private loadAll(refresh: boolean): void {
    this.callsPage = 1;
    this.loadLookbackDependentSections(refresh);
    this.loadFixedWindowSections(refresh);
  }

  private loadLookbackDependent(refresh: boolean): void {
    this.callsPage = 1;
    this.loadLookbackDependentSections(refresh);
  }

  private loadLookbackDependentSections(refresh: boolean): void {
    this.lookbackLoadCancel$.next();
    this.loadSection(
      this.callsPerDay,
      this.commonApiService.getAdminApiTrafficCallsPerDay(this.selectedLookbackHours, refresh),
      "Unable to load calls per day.",
      this.lookbackLoadCancel$
    );
  }

  private loadFixedWindowSections(refresh: boolean): void {
    this.fixedLoadCancel$.next();

    this.loadSection(
      this.busiestHours,
      this.commonApiService.getAdminApiTrafficBusiestHours(refresh),
      "Unable to load busiest hours.",
      this.fixedLoadCancel$,
      (items) => {
        this.hoursChartData = items.map((row) => ({
          label: this.formatHourLabel(row.hour),
          value: row.apiCalls,
        }));
      }
    );
    this.loadSection(
      this.busiestDays,
      this.commonApiService.getAdminApiTrafficBusiestDays(refresh),
      "Unable to load busiest days.",
      this.fixedLoadCancel$,
      (items) => {
        this.daysChartData = items.map((row) => ({
          label: this.formatDayLabel(row.day),
          value: row.apiCalls,
        }));
      }
    );
    this.loadSection(
      this.busiestMonths,
      this.commonApiService.getAdminApiTrafficBusiestMonths(refresh),
      "Unable to load busiest months.",
      this.fixedLoadCancel$,
      (items) => {
        this.monthsChartData = items.map((row) => ({
          label: this.formatMonthLabel(row.month),
          value: row.apiCalls,
        }));
      }
    );
  }

  private loadSection<T>(
    section: MetricSectionState<T>,
    request$: Observable<AdminApiTrafficMetricResponse<T>>,
    fallbackError: string,
    cancel$: Subject<void>,
    onItems?: (items: T[]) => void
  ): void {
    section.loading = true;
    section.error = "";

    request$.pipe(
      takeUntil(cancel$),
      takeUntil(this.destroy$),
      finalize(() => {
        section.loading = false;
      })
    ).subscribe({
      next: (response) => {
        section.items = response.items || [];
        section.lastUpdated = response.lastUpdated || null;
        section.error = response.message || "";
        onItems?.(section.items);
      },
      error: () => {
        section.items = [];
        section.lastUpdated = null;
        section.error = fallbackError;
        onItems?.([]);
      },
    });
  }

  private hasAnySectionData(): boolean {
    return (
      this.callsPerDay.items.length > 0 ||
      this.busiestHours.items.length > 0 ||
      this.busiestDays.items.length > 0 ||
      this.busiestMonths.items.length > 0
    );
  }

  private emptySection<T>(): MetricSectionState<T> {
    return {
      items: [],
      loading: false,
      error: "",
      lastUpdated: null,
    };
  }

  private pageSlice<T>(rows: T[], page: number): T[] {
    const start = (page - 1) * this.pageSize;
    return rows.slice(start, start + this.pageSize);
  }

  private totalPages(rowCount: number): number {
    return Math.max(1, Math.ceil(rowCount / this.pageSize));
  }

  private formatHourLabel(value: string): string {
    return this.parseUtcDate(value).toLocaleString([], {
      month: "short",
      day: "numeric",
      hour: "numeric",
    });
  }

  private formatMonthLabel(value: string): string {
    return this.parseUtcDate(value).toLocaleDateString([], {
      month: "short",
      year: "numeric",
    });
  }

  private parseUtcDate(value: string): Date {
    const normalized = /z$|[+-]\d{2}:\d{2}$/i.test(value) ? value : `${value}Z`;
    return new Date(normalized);
  }
}
