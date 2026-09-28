import { Component, Input, OnChanges } from "@angular/core";

export interface MetricChartSeries {
  key: string;
  label: string;
  color: string;
}

@Component({
  selector: "app-metric-chart",
  templateUrl: "./metric-chart.component.html",
  styleUrls: ["./metric-chart.component.scss"],
})
export class MetricChartComponent implements OnChanges {
  @Input() title: string = "";
  @Input() subtitle: string = "";
  @Input() data: any[] = [];
  @Input() series: MetricChartSeries[] = [];
  @Input() variant: "line" | "area" | "bar" = "line";
  @Input() stackedBars = false;
  @Input() yMax?: number;

  readonly chartWidth = 640;
  readonly chartHeight = 220;
  readonly chartPadding = 18;
  readonly barGapRatio = 0.28;

  maxValue: number = 0;
  activeIndex: number | null = null;

  ngOnChanges(): void {
    this.maxValue = this.resolveMaxValue();

    if (!this.hasData()) {
      this.activeIndex = null;
      return;
    }

    if (this.activeIndex !== null && this.activeIndex >= this.data.length) {
      this.activeIndex = null;
    }
  }

  hasData(): boolean {
    return !!this.data.length && this.maxValue > 0;
  }

  isBarVariant(): boolean {
    return this.variant === "bar";
  }

  getPoints(key: string): string {
    if (!this.hasData()) {
      return "";
    }

    return this.data.map((point, index) => {
      const x = this.getXPosition(index);
      const y = this.getYPosition(Number(point[key] || 0));
      return `${x},${y}`;
    }).join(" ");
  }

  getAreaPoints(key: string): string {
    if (!this.hasData()) {
      return "";
    }

    const linePoints = this.getPoints(key);
    const firstX = this.chartPadding;
    const lastX = this.chartWidth - this.chartPadding;
    const baseY = this.chartHeight - this.chartPadding;

    return `${firstX},${baseY} ${linePoints} ${lastX},${baseY}`;
  }

  getBarX(index: number): number {
    return this.getXPosition(index) - this.getBarWidth() / 2;
  }

  getBarWidth(): number {
    const innerWidth = this.chartWidth - this.chartPadding * 2;
    const slotWidth = this.data.length === 0 ? innerWidth : innerWidth / this.data.length;
    return Math.max(4, slotWidth * (1 - this.barGapRatio));
  }

  getBarHeight(key: string, index: number): number {
    const baseY = this.chartHeight - this.chartPadding;
    if (this.isBarVariant() && this.stackedBars) {
      const base = this.getStackedBaseValue(key, index);
      const value = Number(this.data[index]?.[key] || 0);
      const top = this.getYPosition(base + value);
      const bottom = base > 0 ? this.getYPosition(base) : baseY;
      return Math.max(0, bottom - top);
    }

    const value = Number(this.data[index]?.[key] || 0);
    const top = this.getYPosition(value);
    return Math.max(0, baseY - top);
  }

  getBarY(key: string, index: number): number {
    if (this.isBarVariant() && this.stackedBars) {
      const base = this.getStackedBaseValue(key, index);
      const value = Number(this.data[index]?.[key] || 0);
      return this.getYPosition(base + value);
    }

    return this.getYPosition(Number(this.data[index]?.[key] || 0));
  }

  getTickLabel(index: number): string {
    if (this.data[index]?.label) {
      return this.data[index].label;
    }

    if (!this.data[index]?.timestamp) {
      return "";
    }

    const date = this.parseTimestamp(this.data[index].timestamp);
    return date.toLocaleTimeString([], { hour: "numeric", minute: "2-digit" });
  }

  getMidTickIndex(): number {
    return this.data.length > 2 ? Math.floor(this.data.length / 2) : 0;
  }

  getXPosition(index: number): number {
    const innerWidth = this.chartWidth - this.chartPadding * 2;
    if (this.isBarVariant()) {
      const slotWidth = this.data.length === 0 ? innerWidth : innerWidth / this.data.length;
      return this.chartPadding + slotWidth * index + slotWidth / 2;
    }

    return this.chartPadding + (this.data.length === 1 ? innerWidth / 2 : (innerWidth / (this.data.length - 1)) * index);
  }

  getYPosition(value: number): number {
    const innerHeight = this.chartHeight - this.chartPadding * 2;
    return this.chartHeight - this.chartPadding - (value / this.maxValue) * innerHeight;
  }

  setActiveIndex(index: number): void {
    this.activeIndex = index;
  }

  clearActiveIndex(): void {
    this.activeIndex = null;
  }

  getTooltipStyle(): Record<string, string> {
    if (this.activeIndex === null || !this.hasData()) {
      return { display: "none" };
    }

    const xPercent = (this.getXPosition(this.activeIndex) / this.chartWidth) * 100;
    const topPercent = (this.getActiveTooltipY() / this.chartHeight) * 100;
    const placeOnLeft = xPercent > 58;

    return {
      left: `${xPercent}%`,
      top: `${Math.max(4, Math.min(topPercent, 72))}%`,
      transform: placeOnLeft ? "translate(-100%, -12px)" : "translate(10px, -12px)",
    };
  }

  activeTimestampLabel(): string {
    if (this.activeIndex !== null && this.data[this.activeIndex]?.label) {
      return String(this.data[this.activeIndex].label);
    }

    if (this.activeIndex === null || !this.data[this.activeIndex]?.timestamp) {
      return "";
    }

    return this.parseTimestamp(this.data[this.activeIndex].timestamp).toLocaleString([], {
      month: "short",
      day: "numeric",
      hour: "numeric",
      minute: "2-digit",
    });
  }

  activeValue(key: string): string {
    if (this.activeIndex === null) {
      return "--";
    }

    return `${Number(this.data[this.activeIndex]?.[key] || 0)}`;
  }

  activeSourceBreakdown(): Array<{
    source: string;
    incoming: number;
    newPerson: number;
    alreadyInMpi: number;
    alreadyInMpiPercent: number;
  }> {
    if (this.activeIndex === null) {
      return [];
    }

    const breakdown = this.data[this.activeIndex]?.sourceBreakdown;
    return Array.isArray(breakdown) ? breakdown : [];
  }

  formatSourceBreakdown(source: {
    source: string;
    incoming: number;
    newPerson: number;
    alreadyInMpi: number;
    alreadyInMpiPercent: number;
  }): string {
    const isMatchRateSeries = this.series.some((item) => item.key === "alreadyInMpiPercent");
    if (isMatchRateSeries) {
      return `${source.alreadyInMpiPercent}% (${source.alreadyInMpi} / ${source.incoming})`;
    }

    return `${source.incoming} (${source.newPerson} new Link ID / ${source.alreadyInMpi} already)`;
  }

  activePointY(key: string): number {
    if (this.activeIndex === null) {
      return this.chartHeight - this.chartPadding;
    }

    return this.getYPosition(Number(this.data[this.activeIndex]?.[key] || 0));
  }

  trackBySeries(_: number, item: MetricChartSeries): string {
    return item.key;
  }

  private getActiveTooltipY(): number {
    if (this.activeIndex === null) {
      return this.chartPadding;
    }

    if (this.isBarVariant() && this.stackedBars) {
      const total = this.series.reduce(
        (sum, item) => sum + Number(this.data[this.activeIndex!]?.[item.key] || 0),
        0,
      );
      return this.getYPosition(total);
    }

    if (!this.series.length) {
      return this.chartPadding;
    }

    return Math.min(...this.series.map((item) => this.activePointY(item.key)));
  }

  private resolveMaxValue(): number {
    if (this.yMax !== undefined && this.yMax > 0) {
      return this.yMax;
    }

    if (this.isBarVariant() && this.stackedBars) {
      const totals = this.data.map((point) =>
        this.series.reduce((sum, item) => sum + Number(point[item.key] || 0), 0),
      );
      const max = Math.max(...totals, 0);
      return max > 0 ? max : 0;
    }

    const values = this.data.flatMap((point) => this.series.map((item) => Number(point[item.key] || 0)));
    const max = Math.max(...values, 0);
    return max > 0 ? max : 0;
  }

  private getStackedBaseValue(key: string, index: number): number {
    let base = 0;

    for (const item of this.series) {
      if (item.key === key) {
        break;
      }

      base += Number(this.data[index]?.[item.key] || 0);
    }

    return base;
  }

  private parseTimestamp(value: string): Date {
    const normalized = /z$|[+-]\d{2}:\d{2}$/i.test(value) ? value : `${value}Z`;
    return new Date(normalized);
  }
}
