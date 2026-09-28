import { Component, OnDestroy, OnInit } from "@angular/core";
import { Subject } from "rxjs";
import { takeUntil } from "rxjs/operators";
import {
  ADMIN_EXPORT_LOOKBACK_OPTIONS,
  ADMIN_EXPORT_SECTIONS,
  AdminExportDialogService,
  AdminExportSectionId,
} from "src/app/services/admin-export-dialog.service";
import { AdminReportPdfService } from "src/app/services/admin-report-pdf.service";
import { SharedService } from "src/app/services/sharedService";

@Component({
  selector: "app-admin-export-modal",
  templateUrl: "./admin-export-modal.component.html",
  styleUrls: ["./admin-export-modal.component.scss"],
})
export class AdminExportModalComponent implements OnInit, OnDestroy {
  readonly sections = ADMIN_EXPORT_SECTIONS;
  readonly lookbackOptions = ADMIN_EXPORT_LOOKBACK_OPTIONS;

  dialogOpen = false;
  exporting = false;
  selectAll = false;
  selectedLookbackHours = 24;
  selectedSections = new Set<AdminExportSectionId>();

  private readonly destroy$ = new Subject<void>();

  constructor(
    private readonly dialogService: AdminExportDialogService,
    private readonly reportPdfService: AdminReportPdfService,
    private readonly sharedService: SharedService,
  ) {}

  ngOnInit(): void {
    this.dialogService.state$
      .pipe(takeUntil(this.destroy$))
      .subscribe((state) => {
        this.dialogOpen = state.open;
        if (state.open) {
          this.exporting = false;
          this.selectedLookbackHours = 24;
          this.selectedSections = new Set(state.preselected);
          this.syncSelectAll();
        }
      });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  closeExportDialog(): void {
    if (this.exporting) {
      return;
    }

    this.dialogService.close();
  }

  isSectionSelected(id: AdminExportSectionId): boolean {
    return this.selectedSections.has(id);
  }

  toggleSelectAll(checked: boolean): void {
    this.selectAll = checked;
    this.selectedSections = checked
      ? new Set(this.sections.map((section) => section.id))
      : new Set();
  }

  toggleSection(id: AdminExportSectionId, checked: boolean): void {
    const next = new Set(this.selectedSections);
    if (checked) {
      next.add(id);
    } else {
      next.delete(id);
    }

    this.selectedSections = next;
    this.syncSelectAll();
  }

  canExport(): boolean {
    return this.selectedSections.size > 0 && !this.exporting;
  }

  async runExport(): Promise<void> {
    if (!this.canExport()) {
      return;
    }

    this.exporting = true;
    try {
      await this.reportPdfService.export(
        Array.from(this.selectedSections),
        this.selectedLookbackHours,
      );
      this.dialogService.close();
    } catch (error) {
      const message = error instanceof Error ? error.message : "Unable to export administration report.";
      this.sharedService.showToast(message);
    } finally {
      this.exporting = false;
    }
  }

  private syncSelectAll(): void {
    this.selectAll = this.sections.every((section) => this.selectedSections.has(section.id));
  }
}
