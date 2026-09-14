import { Component, OnDestroy, OnInit } from "@angular/core";
import { Subject } from "rxjs";
import { takeUntil } from "rxjs/operators";
import {
  REPORTS_EXPORT_SECTIONS,
  ReportsExportDialogService,
  ReportsExportSectionId,
} from "src/app/services/reports-export-dialog.service";
import { SharedService } from "src/app/services/sharedService";

@Component({
  selector: "app-reports-export-modal",
  templateUrl: "./reports-export-modal.component.html",
  styleUrls: ["./reports-export-modal.component.scss"],
})
export class ReportsExportModalComponent implements OnInit, OnDestroy {
  readonly sections = REPORTS_EXPORT_SECTIONS;

  dialogOpen = false;
  exporting = false;
  selectAll = false;
  selectedSections = new Set<ReportsExportSectionId>();

  private readonly destroy$ = new Subject<void>();

  constructor(
    private readonly dialogService: ReportsExportDialogService,
    private readonly sharedService: SharedService,
  ) {}

  ngOnInit(): void {
    this.dialogService.state$
      .pipe(takeUntil(this.destroy$))
      .subscribe((state) => {
        this.dialogOpen = state.open;
        if (state.open) {
          this.exporting = false;
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

  isSectionSelected(id: ReportsExportSectionId): boolean {
    return this.selectedSections.has(id);
  }

  toggleSelectAll(checked: boolean): void {
    this.selectAll = checked;
    this.selectedSections = checked
      ? new Set(this.sections.map((section) => section.id))
      : new Set();
  }

  toggleSection(id: ReportsExportSectionId, checked: boolean): void {
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
      await this.dialogService.runExport(Array.from(this.selectedSections));
      this.dialogService.close();
    } catch (error) {
      const message = error instanceof Error ? error.message : "Unable to export reports.";
      this.sharedService.showToast(message);
    } finally {
      this.exporting = false;
    }
  }

  private syncSelectAll(): void {
    this.selectAll = this.sections.every((section) => this.selectedSections.has(section.id));
  }
}
