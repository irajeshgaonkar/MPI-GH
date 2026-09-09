import { Component, HostListener, Input, OnChanges, SimpleChanges } from "@angular/core";

export interface TrafficBarChartPoint {
  label: string;
  value: number;
}

const MAX_BAR_WIDTH = 56;
const MIN_BAR_WIDTH = 12;
const ROTATE_LABEL_THRESHOLD = 8;
const IDEAL_SLOT_FOR_MAX_BAR = MAX_BAR_WIDTH / (1 - 0.32);
const TOOLTIP_LEFT_MIN_PERCENT = 8;
const TOOLTIP_LEFT_MAX_PERCENT = 92;
/** Lowest bar fill opacity (smallest values); max stays at 1. */
const MIN_VALUE_FILL_OPACITY = 0.35;
/** Non-hovered bars when one bar is active. */
const INACTIVE_HOVER_OPACITY = 0.35;

@Component({
  selector: "app-traffic-bar-chart",
  templateUrl: "./traffic-bar-chart.component.html",
  styleUrls: ["./traffic-bar-chart.component.scss"],
})
export class TrafficBarChartComponent implements OnChanges {
  @Input() title = "";
  @Input() subtitle = "";
  @Input() data: TrafficBarChartPoint[] = [];
  @Input() valueLabel = "API Calls";
  @Input() xAxisLabel = "";
  @Input() barColor = "#0b84f3";

  readonly chartWidth = 920;
  readonly chartHeight = 320;
  readonly paddingRight = 24;
  readonly paddingTop = 28;
  readonly barGapRatio = 0.32;

  /** Left padding leaves room for Y tick numbers + axis title. */
  readonly paddingLeft = 72;

  maxValue = 0;
  activeIndex: number | null = null;
  tooltipPinned = false;

  private dataFingerprint = "";

  ngOnChanges(changes: SimpleChanges): void {
    this.maxValue = this.resolveMaxValue();

    if (!changes["data"]) {
      return;
    }

    const nextFingerprint = this.fingerprintData(this.data);
    if (nextFingerprint === this.dataFingerprint) {
      return;
    }

    this.dataFingerprint = nextFingerprint;
    this.activeIndex = null;
    this.tooltipPinned = false;
  }

  @HostListener("document:click", ["$event"])
  onDocumentClick(event: MouseEvent): void {
    const target = event.target as HTMLElement | null;
    if (!target?.closest(".traffic-bar-chart")) {
      this.clearTooltip(true);
    }
  }

  hasData(): boolean {
    return !!this.data?.length && this.maxValue > 0;
  }

  shouldRotateLabels(): boolean {
    return (this.data?.length || 0) > ROTATE_LABEL_THRESHOLD;
  }

  paddingBottom(): number {
    return this.shouldRotateLabels() ? 78 : 56;
  }

  plotWidth(): number {
    return this.chartWidth - this.paddingLeft - this.paddingRight;
  }

  plotHeight(): number {
    return this.chartHeight - this.paddingTop - this.paddingBottom();
  }

  baselineY(): number {
    return this.chartHeight - this.paddingBottom();
  }

  private slotWidth(): number {
    if (!this.data.length) {
      return 0;
    }

    const fullSlot = this.plotWidth() / this.data.length;
    const compactTotal = this.data.length * IDEAL_SLOT_FOR_MAX_BAR;
    if (compactTotal < this.plotWidth()) {
      return IDEAL_SLOT_FOR_MAX_BAR;
    }

    return fullSlot;
  }

  private groupOffsetX(): number {
    if (!this.data.length) {
      return this.paddingLeft;
    }

    const usedWidth = this.data.length * this.slotWidth();
    if (usedWidth >= this.plotWidth()) {
      return this.paddingLeft;
    }

    return this.paddingLeft + (this.plotWidth() - usedWidth) / 2;
  }

  getBarWidth(): number {
    if (!this.data.length) {
      return 0;
    }

    const width = this.slotWidth() * (1 - this.barGapRatio);
    return Math.min(MAX_BAR_WIDTH, Math.max(MIN_BAR_WIDTH, width));
  }

  getBarX(index: number): number {
    const slot = this.slotWidth();
    return this.groupOffsetX() + slot * index + (slot - this.getBarWidth()) / 2;
  }

  /** Full column hit target (easier hover than bar-only width). */
  getHitboxX(index: number): number {
    return this.groupOffsetX() + this.slotWidth() * index;
  }

  getHitboxWidth(): number {
    return this.slotWidth();
  }

  getBarHeight(value: number): number {
    if (this.maxValue <= 0) {
      return 0;
    }

    return (value / this.maxValue) * this.plotHeight();
  }

  getBarY(value: number): number {
    return this.baselineY() - this.getBarHeight(value);
  }

  yTickValues(): number[] {
    if (this.maxValue <= 0) {
      return [0];
    }

    const mid = Math.round(this.maxValue / 2);
    return [this.maxValue, mid, 0];
  }

  getYTickY(value: number): number {
    return this.getBarY(value);
  }

  yAxisTitleX(): number {
    return 16;
  }

  yAxisTitleY(): number {
    return this.paddingTop + this.plotHeight() / 2;
  }

  xAxisTitleX(): number {
    return this.paddingLeft + this.plotWidth() / 2;
  }

  xAxisTitleY(): number {
    return this.chartHeight - 10;
  }

  onBarEnter(index: number): void {
    if (!this.tooltipPinned) {
      this.activeIndex = index;
    }
  }

  onBarLeave(): void {
    if (!this.tooltipPinned) {
      this.activeIndex = null;
    }
  }

  onBarClick(event: MouseEvent, index: number): void {
    event.preventDefault();
    event.stopPropagation();

    if (this.tooltipPinned && this.activeIndex === index) {
      this.tooltipPinned = false;
      this.activeIndex = null;
      return;
    }

    this.activeIndex = index;
    this.tooltipPinned = true;
  }

  onBarFocus(index: number): void {
    this.activeIndex = index;
  }

  onBarKeydown(event: KeyboardEvent, index: number): void {
    if (event.key === "Enter" || event.key === " ") {
      event.preventDefault();
      this.onBarClick(event as unknown as MouseEvent, index);
    }
  }

  clearTooltip(force = false): void {
    if (force || !this.tooltipPinned) {
      this.tooltipPinned = false;
      this.activeIndex = null;
    }
  }

  isActive(index: number): boolean {
    return this.activeIndex === index;
  }

  barFillOpacity(index: number, value: number): number {
    const intensity = this.valueIntensity(value);
    if (this.activeIndex === null) {
      return intensity;
    }

    return this.isActive(index) ? intensity : intensity * INACTIVE_HOVER_OPACITY;
  }

  activePoint(): TrafficBarChartPoint | null {
    if (this.activeIndex === null || !this.data[this.activeIndex]) {
      return null;
    }

    return this.data[this.activeIndex];
  }

  tooltipLeftPercent(): number {
    if (this.activeIndex === null) {
      return 50;
    }

    const centerX = this.getBarX(this.activeIndex) + this.getBarWidth() / 2;
    const percent = (centerX / this.chartWidth) * 100;
    return Math.min(TOOLTIP_LEFT_MAX_PERCENT, Math.max(TOOLTIP_LEFT_MIN_PERCENT, percent));
  }

  tooltipTopPercent(): number {
    if (this.activeIndex === null) {
      return 12;
    }

    const point = this.data[this.activeIndex];
    const barTop = this.getBarY(point.value);
    return Math.max(2, (barTop / this.chartHeight) * 100 - 2);
  }

  formatValue(value: number): string {
    return new Intl.NumberFormat().format(value);
  }

  xLabelTransform(index: number): string | null {
    if (!this.shouldRotateLabels()) {
      return null;
    }

    const x = this.getBarX(index) + this.getBarWidth() / 2;
    const y = this.baselineY() + 20;
    return `rotate(-28 ${x} ${y})`;
  }

  private resolveMaxValue(): number {
    const values = (this.data || []).map((point) => Number(point.value || 0));
    return Math.max(...values, 0);
  }

  private valueIntensity(value: number): number {
    if (this.maxValue <= 0) {
      return MIN_VALUE_FILL_OPACITY;
    }

    const ratio = Math.max(0, Math.min(1, Number(value || 0) / this.maxValue));
    return MIN_VALUE_FILL_OPACITY + (1 - MIN_VALUE_FILL_OPACITY) * ratio;
  }

  private fingerprintData(data: TrafficBarChartPoint[] | null | undefined): string {
    if (!data?.length) {
      return "";
    }

    return data.map((point) => `${point.label}:${point.value}`).join("|");
  }
}
