import { Injectable } from "@angular/core";
import { firstValueFrom } from "rxjs";
import jsPDF from "jspdf";
import autoTable from "jspdf-autotable";
import {
  ADMIN_EXPORT_LOOKBACK_OPTIONS,
  ADMIN_EXPORT_SECTIONS,
  AdminExportSectionId,
} from "src/app/services/admin-export-dialog.service";
import {
  AdminApiTrafficDayCount,
  AdminApiTrafficHourCount,
  AdminApiTrafficMonthCount,
  AdminOnboardedSystem,
  BatchIntakeReliabilityReport,
  CommonApiService,
} from "src/app/services/common-api.service";
import { buildCombinedMetrics } from "src/app/admin/usage-metrics/admin-usage-combined.util";

interface SectionResult {
  id: AdminExportSectionId;
  error?: string;
}

interface BarChartPoint {
  label: string;
  value: number;
}

@Injectable({
  providedIn: "root",
})
export class AdminReportPdfService {
  constructor(private readonly commonApiService: CommonApiService) {}

  async export(sections: AdminExportSectionId[], lookbackHours: number): Promise<void> {
    const ordered = ADMIN_EXPORT_SECTIONS
      .map((section) => section.id)
      .filter((id) => sections.includes(id));

    if (!ordered.length) {
      throw new Error("Select at least one section to export.");
    }

    const doc = new jsPDF({
      orientation: "landscape",
      unit: "pt",
      format: "letter",
    });

    const margin = 28;
    const pageWidth = doc.internal.pageSize.getWidth();
    const contentWidth = pageWidth - margin * 2;
    let cursorY = this.renderCover(doc, ordered, lookbackHours, margin, contentWidth);

    for (const sectionId of ordered) {
      cursorY = this.ensureSpace(doc, cursorY, margin, 48);
      const result = await this.appendSection(doc, sectionId, lookbackHours, margin, contentWidth, cursorY);
      cursorY = result.nextY;
    }

    const safeDate = new Date().toISOString().slice(0, 10);
    doc.save(`mpi-administration-report-${safeDate}.pdf`);
  }

  private async appendSection(
    doc: jsPDF,
    sectionId: AdminExportSectionId,
    lookbackHours: number,
    margin: number,
    contentWidth: number,
    startY: number,
  ): Promise<{ nextY: number; result: SectionResult }> {
    const title = ADMIN_EXPORT_SECTIONS.find((section) => section.id === sectionId)?.label || sectionId;
    let y = this.writeSectionTitle(doc, title, margin, startY);

    try {
      switch (sectionId) {
        case "onboarded-systems":
          y = await this.appendOnboardedSystems(doc, margin, contentWidth, y);
          break;
        case "api-usage":
          y = await this.appendApiUsage(doc, lookbackHours, margin, contentWidth, y);
          break;
        case "batch-usage":
          y = await this.appendBatchUsage(doc, lookbackHours, margin, contentWidth, y);
          break;
        case "combined-usage":
          y = await this.appendCombinedUsage(doc, lookbackHours, margin, contentWidth, y);
          break;
        case "traffic-analysis":
          y = await this.appendTrafficAnalysis(doc, lookbackHours, margin, contentWidth, y);
          break;
      }

      return { nextY: y + 12, result: { id: sectionId } };
    } catch (error) {
      const message = error instanceof Error ? error.message : "Unable to load section data.";
      doc.setFontSize(10);
      doc.setTextColor(120, 60, 0);
      doc.text(`Error: ${message}`, margin, y);
      doc.setTextColor(0, 0, 0);
      return { nextY: y + 28, result: { id: sectionId, error: message } };
    }
  }

  private async appendOnboardedSystems(
    doc: jsPDF,
    margin: number,
    contentWidth: number,
    startY: number,
  ): Promise<number> {
    const response = await firstValueFrom(this.commonApiService.getAdminOnboardedSystems());
    const systems = response.systems || [];
    const active = systems.filter((system) => system.isActive).length;
    let y = this.writeSummaryLine(
      doc,
      `Total: ${systems.length}  |  Active: ${active}  |  Inactive: ${systems.length - active}`,
      margin,
      startY,
    );

    return this.addTable(doc, {
      startY: y,
      margin,
      head: [["System", "Agency", "Tenant", "Mode", "Active", "Notify", "Whitelist"]],
      body: systems.map((system) => [
        system.sourceSystemName || "",
        system.agencyName || "",
        system.tenant || "",
        system.connectivityMode || "",
        system.isActive ? "Yes" : "No",
        system.enableNotification ? "Yes" : "No",
        this.whitelistSummary(system),
      ]),
      columnStyles: {
        0: { cellWidth: 110 },
        1: { cellWidth: 100 },
        2: { cellWidth: 90 },
        3: { cellWidth: 70 },
        4: { cellWidth: 45 },
        5: { cellWidth: 50 },
        6: { cellWidth: contentWidth - 465 },
      },
    });
  }

  private async appendApiUsage(
    doc: jsPDF,
    lookbackHours: number,
    margin: number,
    contentWidth: number,
    startY: number,
  ): Promise<number> {
    const metrics = await firstValueFrom(this.commonApiService.getAdminUsageMetrics(lookbackHours));
    let y = this.writeSummaryLine(
      doc,
      [
        `Lookback: ${this.lookbackLabel(lookbackHours)}`,
        `Systems: ${metrics.totalSystems}`,
        `Calls: ${this.formatNumber(metrics.totalCalls)}`,
        `Success: ${this.formatNumber(metrics.totalSuccessCount)}`,
        `Failed: ${this.formatNumber(metrics.totalFailedCount)}`,
        `Avg Failure: ${metrics.averageFailureRate}%`,
        `Source IPs: ${this.formatNumber(metrics.totalDistinctSourceIps)}`,
      ].join("  |  "),
      margin,
      startY,
    );

    return this.addTable(doc, {
      startY: y,
      margin,
      head: [["System", "Tenant", "Calls", "Success", "Failed", "Failure %", "IPs", "First Seen", "Last Seen"]],
      body: (metrics.systems || []).map((row) => [
        row.system || "",
        row.tenant || "",
        this.formatNumber(row.callCount),
        this.formatNumber(row.successCount),
        this.formatNumber(row.failedCount),
        `${row.failureRate}`,
        this.formatNumber(row.sourceIpCount),
        this.formatDate(row.firstSeen),
        this.formatDate(row.lastSeen),
      ]),
      columnStyles: {
        0: { cellWidth: 100 },
        1: { cellWidth: 80 },
        2: { cellWidth: 55, halign: "right" },
        3: { cellWidth: 55, halign: "right" },
        4: { cellWidth: 50, halign: "right" },
        5: { cellWidth: 55, halign: "right" },
        6: { cellWidth: 40, halign: "right" },
        7: { cellWidth: 90 },
        8: { cellWidth: contentWidth - 525 },
      },
    });
  }

  private async appendBatchUsage(
    doc: jsPDF,
    lookbackHours: number,
    margin: number,
    contentWidth: number,
    startY: number,
  ): Promise<number> {
    const lookbackDays = this.lookbackDays(lookbackHours);
    const report = await firstValueFrom(this.commonApiService.getAdminBatchIntakeReliability(lookbackDays));
    let y = this.writeSummaryLine(
      doc,
      [
        `Lookback: ${this.lookbackLabel(lookbackHours)} (${lookbackDays} day${lookbackDays === 1 ? "" : "s"})`,
        `Files: ${this.formatNumber(report.totalFiles)}`,
        `Failed Files: ${this.formatNumber(report.failedFiles)}`,
        `Rejected Records: ${this.formatNumber(report.rejectedRecords)}`,
        `Avg Processing (min): ${report.averageProcessingMinutes}`,
      ].join("  |  "),
      margin,
      startY,
    );

    y = this.writeSubheading(doc, "Source Summary", margin, y);
    y = this.addTable(doc, {
      startY: y,
      margin,
      head: [["Source System", "Tenant", "Files", "Failed", "Rejected", "Avg Processing (min)"]],
      body: (report.sourceSummaries || []).map((row) => [
        row.sourceSystemName || "",
        row.tenant || "",
        this.formatNumber(row.totalFiles),
        this.formatNumber(row.failedFiles),
        this.formatNumber(row.rejectedRecords),
        `${row.averageProcessingMinutes}`,
      ]),
      columnStyles: {
        0: { cellWidth: 140 },
        1: { cellWidth: 100 },
        2: { cellWidth: 60, halign: "right" },
        3: { cellWidth: 60, halign: "right" },
        4: { cellWidth: 70, halign: "right" },
        5: { cellWidth: contentWidth - 430, halign: "right" },
      },
    });

    y = this.ensureSpace(doc, y, margin, 40);
    y = this.writeSubheading(doc, "Recent Files", margin, y);
    y = this.addTable(doc, {
      startY: y,
      margin,
      head: [["Request ID", "File", "Source", "Records", "Status", "Rejects", "Requested"]],
      body: (report.files || []).slice(0, 100).map((row) => [
        row.requestId || "",
        row.fileName || "",
        row.sourceSystemName || "",
        this.formatNumber(row.recordsCount),
        row.status || "",
        this.formatNumber(row.rejectCount),
        this.formatDate(row.requestDateTime),
      ]),
      columnStyles: {
        0: { cellWidth: 90 },
        1: { cellWidth: 130 },
        2: { cellWidth: 100 },
        3: { cellWidth: 55, halign: "right" },
        4: { cellWidth: 70 },
        5: { cellWidth: 55, halign: "right" },
        6: { cellWidth: contentWidth - 500 },
      },
    });

    if ((report.topErrors || []).length) {
      y = this.ensureSpace(doc, y, margin, 40);
      y = this.writeSubheading(doc, "Top Errors", margin, y);
      y = this.addTable(doc, {
        startY: y,
        margin,
        head: [["Source System", "Message", "Count"]],
        body: report.topErrors.map((row) => [
          row.sourceSystemName || "",
          row.message || "",
          this.formatNumber(row.count),
        ]),
        columnStyles: {
          0: { cellWidth: 120 },
          1: { cellWidth: contentWidth - 180 },
          2: { cellWidth: 60, halign: "right" },
        },
      });
    }

    return y;
  }

  private async appendCombinedUsage(
    doc: jsPDF,
    lookbackHours: number,
    margin: number,
    contentWidth: number,
    startY: number,
  ): Promise<number> {
    const lookbackDays = this.lookbackDays(lookbackHours);
    const [apiMetrics, batchMetrics] = await Promise.all([
      firstValueFrom(this.commonApiService.getAdminUsageMetrics(lookbackHours)),
      firstValueFrom(this.commonApiService.getAdminBatchIntakeReliability(lookbackDays)),
    ]);
    const rows = buildCombinedMetrics(apiMetrics, batchMetrics);
    const totalCalls = rows.reduce((sum, row) => sum + row.callCount, 0);
    const totalFailed = rows.reduce((sum, row) => sum + row.failedCount, 0);
    let y = this.writeSummaryLine(
      doc,
      [
        `Lookback: ${this.lookbackLabel(lookbackHours)}`,
        `Systems: ${rows.length}`,
        `Total Calls: ${this.formatNumber(totalCalls)}`,
        `Failed: ${this.formatNumber(totalFailed)}`,
        `API Calls: ${this.formatNumber(apiMetrics.totalCalls)}`,
        `Batch Records: ${this.formatNumber(this.batchRecordTotal(batchMetrics))}`,
      ].join("  |  "),
      margin,
      startY,
    );

    return this.addTable(doc, {
      startY: y,
      margin,
      head: [["System", "Tenant", "Total", "API", "Batch", "Success", "Failed", "Failure %", "IPs"]],
      body: rows.map((row) => [
        row.system || "",
        row.tenant || "",
        this.formatNumber(row.callCount),
        this.formatNumber(row.apiCallCount),
        this.formatNumber(row.batchRecordCount),
        this.formatNumber(row.successCount),
        this.formatNumber(row.failedCount),
        `${row.failureRate}`,
        this.formatNumber(row.sourceIpCount),
      ]),
      columnStyles: {
        0: { cellWidth: 110 },
        1: { cellWidth: 90 },
        2: { cellWidth: 55, halign: "right" },
        3: { cellWidth: 50, halign: "right" },
        4: { cellWidth: 55, halign: "right" },
        5: { cellWidth: 55, halign: "right" },
        6: { cellWidth: 50, halign: "right" },
        7: { cellWidth: 55, halign: "right" },
        8: { cellWidth: contentWidth - 520, halign: "right" },
      },
    });
  }

  private async appendTrafficAnalysis(
    doc: jsPDF,
    lookbackHours: number,
    margin: number,
    contentWidth: number,
    startY: number,
  ): Promise<number> {
    const [callsPerDay, busiestHours, busiestDays, busiestMonths] = await Promise.all([
      firstValueFrom(this.commonApiService.getAdminApiTrafficCallsPerDay(lookbackHours)),
      firstValueFrom(this.commonApiService.getAdminApiTrafficBusiestHours()),
      firstValueFrom(this.commonApiService.getAdminApiTrafficBusiestDays()),
      firstValueFrom(this.commonApiService.getAdminApiTrafficBusiestMonths()),
    ]);

    let y = this.writeSummaryLine(
      doc,
      `Lookback (tables): ${this.lookbackLabel(lookbackHours)}  |  Peak windows: hours 24h, days 30d, months 180d`,
      margin,
      startY,
    );

    y = this.writeSubheading(doc, "Calls Per Day", margin, y);
    y = this.addTable(doc, {
      startY: y,
      margin,
      head: [["Day", "API Calls"]],
      body: this.sortDaysDesc(callsPerDay.items || []).map((row) => [
        this.formatDayLabel(row.day),
        this.formatNumber(row.apiCalls),
      ]),
      columnStyles: {
        0: { cellWidth: contentWidth - 120 },
        1: { cellWidth: 120, halign: "right" },
      },
    });

    y = this.ensureSpace(doc, y, margin, 40);
    y = this.writeSubheading(doc, "Busiest Hours in a Day", margin, y);
    y = this.addTable(doc, {
      startY: y,
      margin,
      head: [["Hour", "API Calls"]],
      body: (busiestHours.items || []).map((row: AdminApiTrafficHourCount) => [
        this.formatHourLabel(row.hour),
        this.formatNumber(row.apiCalls),
      ]),
      columnStyles: {
        0: { cellWidth: contentWidth - 120 },
        1: { cellWidth: 120, halign: "right" },
      },
    });
    y = this.drawBarChart(
      doc,
      (busiestHours.items || []).map((row) => ({
        label: this.formatHourLabel(row.hour),
        value: row.apiCalls,
      })),
      margin,
      contentWidth,
      y,
    );

    y = this.ensureSpace(doc, y, margin, 40);
    y = this.writeSubheading(doc, "Busiest Days in a Month", margin, y);
    y = this.addTable(doc, {
      startY: y,
      margin,
      head: [["Day", "API Calls"]],
      body: (busiestDays.items || []).map((row: AdminApiTrafficDayCount) => [
        this.formatDayLabel(row.day),
        this.formatNumber(row.apiCalls),
      ]),
      columnStyles: {
        0: { cellWidth: contentWidth - 120 },
        1: { cellWidth: 120, halign: "right" },
      },
    });
    y = this.drawBarChart(
      doc,
      (busiestDays.items || []).map((row) => ({
        label: this.formatDayLabel(row.day),
        value: row.apiCalls,
      })),
      margin,
      contentWidth,
      y,
    );

    y = this.ensureSpace(doc, y, margin, 40);
    y = this.writeSubheading(doc, "Busiest Months", margin, y);
    y = this.addTable(doc, {
      startY: y,
      margin,
      head: [["Month", "API Calls"]],
      body: this.sortMonthsAsc(busiestMonths.items || []).map((row: AdminApiTrafficMonthCount) => [
        this.formatMonthLabel(row.month),
        this.formatNumber(row.apiCalls),
      ]),
      columnStyles: {
        0: { cellWidth: contentWidth - 120 },
        1: { cellWidth: 120, halign: "right" },
      },
    });
    y = this.drawBarChart(
      doc,
      this.sortMonthsAsc(busiestMonths.items || []).map((row) => ({
        label: this.formatMonthLabel(row.month),
        value: row.apiCalls,
      })),
      margin,
      contentWidth,
      y,
    );

    return y;
  }

  private renderCover(
    doc: jsPDF,
    sections: AdminExportSectionId[],
    lookbackHours: number,
    margin: number,
    contentWidth: number,
  ): number {
    const exportedBy = localStorage.getItem("LoggedInUser") || "Unknown User";
    const sectionLabels = sections
      .map((id) => ADMIN_EXPORT_SECTIONS.find((section) => section.id === id)?.label || id)
      .join(", ");

    doc.setFontSize(16);
    doc.setFont("helvetica", "bold");
    doc.text("MPI Administration Report", margin, margin + 8);

    doc.setFontSize(10);
    doc.setFont("helvetica", "normal");
    const lines = [
      `Exported By: ${exportedBy}`,
      `Exported At: ${new Date().toLocaleString()}`,
      `Lookback: ${this.lookbackLabel(lookbackHours)}`,
      `Sections: ${sectionLabels}`,
    ];

    let y = margin + 28;
    for (const line of lines) {
      doc.text(line, margin, y);
      y += 14;
    }

    doc.setDrawColor(180, 190, 200);
    doc.line(margin, y + 4, margin + contentWidth, y + 4);
    return y + 20;
  }

  private writeSectionTitle(doc: jsPDF, title: string, margin: number, y: number): number {
    doc.setFontSize(13);
    doc.setFont("helvetica", "bold");
    doc.setTextColor(25, 50, 77);
    doc.text(title, margin, y);
    doc.setTextColor(0, 0, 0);
    doc.setFont("helvetica", "normal");
    return y + 16;
  }

  private writeSubheading(doc: jsPDF, title: string, margin: number, y: number): number {
    doc.setFontSize(11);
    doc.setFont("helvetica", "bold");
    doc.text(title, margin, y);
    doc.setFont("helvetica", "normal");
    return y + 12;
  }

  private writeSummaryLine(doc: jsPDF, text: string, margin: number, y: number): number {
    doc.setFontSize(9);
    doc.setTextColor(70, 90, 110);
    const wrapped = doc.splitTextToSize(text, doc.internal.pageSize.getWidth() - margin * 2);
    doc.text(wrapped, margin, y);
    doc.setTextColor(0, 0, 0);
    return y + wrapped.length * 12 + 6;
  }

  private addTable(
    doc: jsPDF,
    options: {
      startY: number;
      margin: number;
      head: string[][];
      body: (string | number)[][];
      columnStyles?: Record<number, { cellWidth?: number; halign?: "left" | "center" | "right" }>;
    },
  ): number {
    autoTable(doc, {
      startY: options.startY,
      margin: { left: options.margin, right: options.margin },
      head: options.head,
      body: options.body.length ? options.body : [["No data available"]],
      styles: {
        fontSize: 8,
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
      columnStyles: options.columnStyles,
    });

    const finalY = (doc as jsPDF & { lastAutoTable?: { finalY: number } }).lastAutoTable?.finalY;
    return (finalY ?? options.startY) + 14;
  }

  private drawBarChart(
    doc: jsPDF,
    points: BarChartPoint[],
    margin: number,
    contentWidth: number,
    startY: number,
  ): number {
    if (!points.length) {
      return startY;
    }

    const chartHeight = 110;
    const labelHeight = 28;
    const chartTop = this.ensureSpace(doc, startY, margin, chartHeight + labelHeight + 10);
    const chartBottom = chartTop + chartHeight;
    const maxValue = Math.max(...points.map((point) => point.value), 1);
    const slotWidth = contentWidth / points.length;
    const barWidth = Math.min(36, slotWidth * 0.55);

    doc.setDrawColor(200, 210, 220);
    doc.setLineWidth(0.5);
    doc.line(margin, chartBottom, margin + contentWidth, chartBottom);

    points.forEach((point, index) => {
      const barHeight = (point.value / maxValue) * (chartHeight - 8);
      const x = margin + index * slotWidth + (slotWidth - barWidth) / 2;
      const y = chartBottom - barHeight;
      const shade = Math.round(90 + (point.value / maxValue) * 120);
      doc.setFillColor(11, shade, 243);
      doc.rect(x, y, barWidth, barHeight, "F");

      doc.setFontSize(7);
      doc.setTextColor(60, 80, 100);
      const label = doc.splitTextToSize(point.label, slotWidth - 4);
      doc.text(label, x + barWidth / 2, chartBottom + 10, { align: "center" });
      doc.text(this.formatNumber(point.value), x + barWidth / 2, y - 4, { align: "center" });
    });

    doc.setTextColor(0, 0, 0);
    return chartBottom + labelHeight + 8;
  }

  private ensureSpace(doc: jsPDF, y: number, margin: number, needed: number): number {
    const pageHeight = doc.internal.pageSize.getHeight();
    if (y + needed <= pageHeight - margin) {
      return y;
    }

    doc.addPage();
    return margin;
  }

  private whitelistSummary(system: AdminOnboardedSystem): string {
    if (!system.ipWhitelists?.length) {
      return "None";
    }

    return system.ipWhitelists
      .map((item) => item.ipAddressCidr || `${item.startIpAddress || ""}-${item.endIpAddress || ""}`.trim())
      .join("; ");
  }

  private lookbackDays(lookbackHours: number): number {
    return Math.max(1, Math.ceil(lookbackHours / 24));
  }

  private lookbackLabel(lookbackHours: number): string {
    return ADMIN_EXPORT_LOOKBACK_OPTIONS.find((option) => option.value === lookbackHours)?.label
      || `${lookbackHours} hours`;
  }

  private batchRecordTotal(report: BatchIntakeReliabilityReport): number {
    return (report.files || []).reduce((sum, file) => sum + (file.recordsCount || 0), 0);
  }

  private formatNumber(value: number | null | undefined): string {
    return Number(value || 0).toLocaleString();
  }

  private formatDate(value?: string | null): string {
    if (!value) {
      return "";
    }

    const date = new Date(value);
    return Number.isNaN(date.getTime()) ? value : date.toLocaleString();
  }

  private formatDayLabel(value: string): string {
    const date = new Date(value);
    return Number.isNaN(date.getTime())
      ? value
      : date.toLocaleDateString(undefined, { weekday: "short", month: "short", day: "numeric" });
  }

  private formatHourLabel(value: string): string {
    const date = new Date(value);
    if (Number.isNaN(date.getTime())) {
      return value;
    }

    return date.toLocaleString(undefined, { month: "short", day: "numeric", hour: "numeric" });
  }

  private formatMonthLabel(value: string): string {
    const date = new Date(value);
    return Number.isNaN(date.getTime())
      ? value
      : date.toLocaleDateString(undefined, { month: "short", year: "numeric" });
  }

  private sortDaysDesc(items: AdminApiTrafficDayCount[]): AdminApiTrafficDayCount[] {
    return [...items].sort((left, right) => new Date(right.day).getTime() - new Date(left.day).getTime());
  }

  private sortMonthsAsc(items: AdminApiTrafficMonthCount[]): AdminApiTrafficMonthCount[] {
    return [...items].sort((left, right) => new Date(left.month).getTime() - new Date(right.month).getTime());
  }
}
