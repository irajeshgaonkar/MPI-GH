import { Injectable } from "@angular/core";
import { BehaviorSubject } from "rxjs";

export type AdminExportSectionId =
  | "onboarded-systems"
  | "combined-usage"
  | "api-usage"
  | "batch-usage"
  | "traffic-analysis";

export interface AdminExportSectionOption {
  id: AdminExportSectionId;
  label: string;
}

export interface AdminExportDialogState {
  open: boolean;
  preselected: AdminExportSectionId[];
}

export const ADMIN_EXPORT_SECTIONS: AdminExportSectionOption[] = [
  { id: "onboarded-systems", label: "Onboarded Systems" },
  { id: "combined-usage", label: "Combined Usage" },
  { id: "api-usage", label: "API Usage" },
  { id: "batch-usage", label: "Batch Usage" },
  { id: "traffic-analysis", label: "Traffic Analysis" },
];

export const ADMIN_EXPORT_LOOKBACK_OPTIONS = [
  { label: "1 hour", value: 1 },
  { label: "6 hours", value: 6 },
  { label: "24 hours", value: 24 },
  { label: "Weekly", value: 24 * 7 },
  { label: "Monthly", value: 24 * 30 },
  { label: "3 Months", value: 24 * 90 },
  { label: "6 Months", value: 24 * 180 },
  { label: "Yearly", value: 24 * 365 },
];

@Injectable({
  providedIn: "root",
})
export class AdminExportDialogService {
  private readonly stateSubject = new BehaviorSubject<AdminExportDialogState>({
    open: false,
    preselected: [],
  });

  readonly state$ = this.stateSubject.asObservable();

  open(options?: { preselected?: AdminExportSectionId[] }): void {
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
}
