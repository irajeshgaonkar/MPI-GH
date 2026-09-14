import { Injectable } from "@angular/core";
import { BehaviorSubject } from "rxjs";

export type ReportsExportSectionId =
  | "overview"
  | "incoming-match-trend"
  | "data-quality"
  | "mpi-linkage"
  | "batch-intake"
  | "stewardship"
  | "data-sharing";

export interface ReportsExportSectionOption {
  id: ReportsExportSectionId;
  label: string;
}

export interface ReportsExportDialogState {
  open: boolean;
  preselected: ReportsExportSectionId[];
}

export const REPORTS_EXPORT_SECTIONS: ReportsExportSectionOption[] = [
  { id: "overview", label: "Overview" },
  { id: "incoming-match-trend", label: "Incoming Match Trend" },
  { id: "data-quality", label: "Source System Data Quality" },
  { id: "mpi-linkage", label: "MPI Linkage" },
  { id: "batch-intake", label: "Batch Intake Reliability" },
  { id: "stewardship", label: "Manual Stewardship" },
  { id: "data-sharing", label: "Data Sharing Coverage" },
];

@Injectable({
  providedIn: "root",
})
export class ReportsExportDialogService {
  private readonly stateSubject = new BehaviorSubject<ReportsExportDialogState>({
    open: false,
    preselected: [],
  });
  private exportHandler: ((sections: ReportsExportSectionId[]) => Promise<void>) | null = null;

  readonly state$ = this.stateSubject.asObservable();

  registerExportHandler(handler: (sections: ReportsExportSectionId[]) => Promise<void>): void {
    this.exportHandler = handler;
  }

  unregisterExportHandler(): void {
    this.exportHandler = null;
  }

  open(options?: { preselected?: ReportsExportSectionId[] }): void {
    this.stateSubject.next({
      open: true,
      preselected: options?.preselected ? [...options.preselected] : [],
    });
  }

  close(): void {
    this.stateSubject.next({
      open: false,
      preselected: [],
    });
  }

  async runExport(sections: ReportsExportSectionId[]): Promise<void> {
    if (!this.exportHandler) {
      throw new Error("Unable to export reports.");
    }

    await this.exportHandler(sections);
  }
}
