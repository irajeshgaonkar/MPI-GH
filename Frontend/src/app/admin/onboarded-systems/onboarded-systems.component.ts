import { Component, OnInit } from "@angular/core";
import { AbstractControl, FormArray, FormBuilder, FormGroup, ValidationErrors, ValidatorFn, Validators } from "@angular/forms";
import { switchMap } from "rxjs";
import {
  AdminOnboardedSystem,
  AdminOnboardedSystemIpWhitelist,
  AdminUpsertOnboardedSystemRequest,
  CommonApiService,
} from "src/app/services/common-api.service";
import { SharedService } from "src/app/services/sharedService";
import { AdminExportDialogService } from "src/app/services/admin-export-dialog.service";

@Component({
  selector: "app-onboarded-systems",
  templateUrl: "./onboarded-systems.component.html",
  styleUrls: ["./onboarded-systems.component.scss"],
})
export class OnboardedSystemsComponent implements OnInit {
  readonly tenantValues = ["HHS Coalition", "Non-Coalition"];

  loading = false;
  saving = false;
  editorOpen = false;
  search = "";
  statusFilter = "all";
  tenantFilter = "all";
  selectedSystemId: number | null = null;
  systems: AdminOnboardedSystem[] = [];
  expandedWhitelistSystems = new Set<number>();

  readonly systemForm = this.fb.group({
    sourceSystemName: ["", Validators.required],
    agencyName: ["", Validators.required],
    tenant: ["", Validators.required],
    connectivityMode: [""],
    onboardingDate: ["", Validators.required],
    isActive: [true],
    enableNotification: [false],
    ipWhitelists: this.fb.array([]),
  });

  constructor(
    private fb: FormBuilder,
    private commonApiService: CommonApiService,
    private sharedService: SharedService,
    private adminExportDialog: AdminExportDialogService,
  ) {}

  ngOnInit(): void {
    this.loadSystems();
  }

  get whitelistRows(): FormArray {
    return this.systemForm.get("ipWhitelists") as FormArray;
  }

  get totalSystems(): number {
    return this.systems.length;
  }

  get activeSystems(): number {
    return this.systems.filter((system) => system.isActive).length;
  }

  get inactiveSystems(): number {
    return this.systems.filter((system) => !system.isActive).length;
  }

  get tenantOptions(): string[] {
    return [...new Set([...this.tenantValues, ...this.systems.map((system) => system.tenant).filter((tenant) => !!tenant)])]
      .sort((left, right) => left.localeCompare(right));
  }

  filteredSystems(): AdminOnboardedSystem[] {
    const normalized = this.search.trim().toLowerCase();
    return this.systems.filter((system) => {
      const matchesSearch = !normalized || [
        system.sourceSystemName,
        system.agencyName,
        system.tenant,
        system.connectivityMode || "",
        system.enableNotification ? "notifications on" : "notifications off",
        ...system.ipWhitelists.flatMap((item) => [item.startIpAddress || "", item.endIpAddress || "", item.ipAddressCidr || ""]),
      ].join(" ").toLowerCase().includes(normalized);

      const matchesStatus = this.statusFilter === "all"
        || (this.statusFilter === "active" && system.isActive)
        || (this.statusFilter === "inactive" && !system.isActive);

      const matchesTenant = this.tenantFilter === "all" || system.tenant === this.tenantFilter;

      return matchesSearch && matchesStatus && matchesTenant;
    });
  }

  openCreate(): void {
    this.selectedSystemId = null;
    this.editorOpen = true;
    this.systemForm.reset({
      sourceSystemName: "",
      agencyName: "",
      tenant: "",
      connectivityMode: "",
      onboardingDate: this.today(),
      isActive: true,
      enableNotification: false,
    });
    this.whitelistRows.clear();
    this.addWhitelistRow();
  }

  editSystem(system: AdminOnboardedSystem): void {
    this.selectedSystemId = system.id;
    this.editorOpen = true;
    this.systemForm.reset({
      sourceSystemName: system.sourceSystemName,
      agencyName: system.agencyName,
      tenant: system.tenant,
      connectivityMode: system.connectivityMode || "",
      onboardingDate: this.formatDateForInput(system.onboardingDate) || this.today(),
      isActive: system.isActive,
      enableNotification: system.enableNotification,
    });
    this.whitelistRows.clear();
    const rows = system.ipWhitelists.length ? system.ipWhitelists : [];
    rows.forEach((item) => this.addWhitelistRow(item));
    if (!rows.length) {
      this.addWhitelistRow();
    }
  }

  closeEditor(): void {
    this.editorOpen = false;
    this.selectedSystemId = null;
    this.systemForm.reset();
    this.whitelistRows.clear();
  }

  addWhitelistRow(item?: AdminOnboardedSystemIpWhitelist): void {
    this.whitelistRows.push(this.buildWhitelistGroup(item));
  }

  removeWhitelistRow(index: number): void {
    this.whitelistRows.removeAt(index);
    if (!this.whitelistRows.length) {
      this.addWhitelistRow();
    }
  }

  saveSystem(): void {
    if (this.systemForm.invalid) {
      this.systemForm.markAllAsTouched();
      return;
    }

    const payload = this.buildPayload();
    const whitelistError = this.validateWhitelists(payload.ipWhitelists);
    if (whitelistError) {
      this.sharedService.showToast(whitelistError);
      return;
    }

    this.saving = true;
    const request = this.selectedSystemId
      ? this.commonApiService.updateAdminOnboardedSystem(this.selectedSystemId, payload)
      : this.commonApiService.createAdminOnboardedSystem(payload);
    const existingSystem = this.selectedSystemId
      ? this.systems.find((system) => system.id === this.selectedSystemId) || null
      : null;
    const currentSourceName = String(existingSystem?.sourceSystemName || "").trim().toLowerCase();
    const requestedSourceName = payload.sourceSystemName.trim().toLowerCase();
    const requiresVeratoSourceCreation = !this.selectedSystemId || currentSourceName !== requestedSourceName;
    const saveRequest = requiresVeratoSourceCreation
      ? this.commonApiService.createVeratoDataSource(payload.sourceSystemName).pipe(switchMap(() => request))
      : request;

    saveRequest.subscribe({
      next: () => {
        this.sharedService.showToast(`Onboarded system ${this.selectedSystemId ? "updated" : "created"} successfully`);
        this.closeEditor();
        this.loadSystems();
      },
      error: (error) => {
        this.saving = false;
        this.sharedService.showToast(error?.error?.message || "Unable to save onboarded system");
      }
    });
  }

  deleteSystem(system: AdminOnboardedSystem): void {
    if (!window.confirm(`Delete onboarded system "${system.sourceSystemName}"?`)) {
      return;
    }

    this.commonApiService.deleteAdminOnboardedSystem(system.id).subscribe({
      next: () => {
        this.sharedService.showToast("Onboarded system deleted successfully");
        if (this.selectedSystemId === system.id) {
          this.closeEditor();
        }
        this.loadSystems();
      },
      error: (error) => this.sharedService.showToast(error?.error?.message || "Unable to delete onboarded system")
    });
  }

  openExportDialog(): void {
    this.adminExportDialog.open({ preselected: ["onboarded-systems"] });
  }

  whitelistSummary(system: AdminOnboardedSystem): string {
    if (!system.ipWhitelists.length) {
      return "No whitelist entries";
    }

    return system.ipWhitelists
      .map((item) => item.ipAddressCidr || `${item.startIpAddress || ""} - ${item.endIpAddress || ""}`.trim())
      .join(", ");
  }

  whitelistEntries(system: AdminOnboardedSystem): string[] {
    if (!system.ipWhitelists.length) {
      return [];
    }

    return system.ipWhitelists.map((item) => item.ipAddressCidr || `${item.startIpAddress || ""} - ${item.endIpAddress || ""}`.trim());
  }

  visibleWhitelistEntries(system: AdminOnboardedSystem): string[] {
    const entries = this.whitelistEntries(system);
    if (this.isWhitelistExpanded(system) || entries.length <= 1) {
      return entries;
    }

    return entries.slice(0, 1);
  }

  isWhitelistExpanded(system: AdminOnboardedSystem): boolean {
    return this.expandedWhitelistSystems.has(system.id);
  }

  canToggleWhitelist(system: AdminOnboardedSystem): boolean {
    return this.whitelistEntries(system).length > 1;
  }

  remainingWhitelistCount(system: AdminOnboardedSystem): number {
    return Math.max(this.whitelistEntries(system).length - 1, 0);
  }

  toggleWhitelist(system: AdminOnboardedSystem): void {
    if (this.isWhitelistExpanded(system)) {
      this.expandedWhitelistSystems.delete(system.id);
      return;
    }

    this.expandedWhitelistSystems.add(system.id);
  }

  whitelistedIpCount(system: AdminOnboardedSystem): string {
    if (!system.ipWhitelists.length) {
      return "0";
    }

    let total = 0;
    let hasValidEntry = false;

    for (const whitelist of system.ipWhitelists) {
      const count = this.countWhitelistEntry(whitelist);
      if (count !== null) {
        total += count;
        hasValidEntry = true;
      }
    }

    if (!hasValidEntry) {
      return "0";
    }

    return total.toLocaleString();
  }

  trackBySystem(_: number, item: AdminOnboardedSystem): number {
    return item.id;
  }

  trackByIndex(index: number): number {
    return index;
  }

  private loadSystems(selectSystemId?: number): void {
    this.loading = true;
    this.commonApiService.getAdminOnboardedSystems().subscribe({
      next: (response) => {
        this.systems = response.systems;
        this.loading = false;
        this.saving = false;
        if (selectSystemId) {
          const selected = this.systems.find((item) => item.id === selectSystemId);
          if (selected) {
            this.editSystem(selected);
          }
        }
      },
      error: () => {
        this.loading = false;
        this.saving = false;
        this.sharedService.showToast("Unable to load onboarded systems");
      }
    });
  }

  private buildWhitelistGroup(item?: AdminOnboardedSystemIpWhitelist): FormGroup {
    return this.fb.group({
      id: [item?.id || null],
      startIpAddress: [item?.startIpAddress || "", this.ipv4Validator()],
      endIpAddress: [item?.endIpAddress || "", this.ipv4Validator()],
      ipAddressCidr: [item?.ipAddressCidr || "", this.cidrValidator()],
    }, { validators: this.whitelistRowValidator() });
  }

  private buildPayload(): AdminUpsertOnboardedSystemRequest {
    const formValue = this.systemForm.getRawValue();
    return {
      sourceSystemName: String(formValue.sourceSystemName || "").trim(),
      agencyName: String(formValue.agencyName || "").trim(),
      tenant: String(formValue.tenant || "").trim(),
      connectivityMode: String(formValue.connectivityMode || "").trim() || null,
      onboardingDate: String(formValue.onboardingDate || ""),
      isActive: !!formValue.isActive,
      enableNotification: !!formValue.enableNotification,
      ipWhitelists: this.whitelistRows.controls.map((group) => ({
        id: group.get("id")?.value || null,
        startIpAddress: String(group.get("startIpAddress")?.value || "").trim() || null,
        endIpAddress: String(group.get("endIpAddress")?.value || "").trim() || null,
        ipAddressCidr: String(group.get("ipAddressCidr")?.value || "").trim() || null,
      })),
    };
  }

  private validateWhitelists(whitelists: AdminUpsertOnboardedSystemRequest["ipWhitelists"]): string {
    for (const whitelist of whitelists) {
      const hasRange = !!whitelist.startIpAddress || !!whitelist.endIpAddress;
      const hasCidr = !!whitelist.ipAddressCidr;

      if (!hasRange && !hasCidr) {
        continue;
      }

      if ((!!whitelist.startIpAddress) !== (!!whitelist.endIpAddress)) {
        return "Each IP range entry requires both a start and end IP address.";
      }

      if (hasRange && hasCidr) {
        return "Use either CIDR or start/end range for a whitelist entry, not both.";
      }
    }

    return "";
  }

  private formatDateForInput(value?: string | null): string {
    if (!value) {
      return "";
    }

    return new Date(value).toISOString().slice(0, 10);
  }

  private today(): string {
    return new Date().toISOString().slice(0, 10);
  }

  private countWhitelistEntry(whitelist: AdminOnboardedSystemIpWhitelist): number | null {
    if (whitelist.ipAddressCidr) {
      return this.countCidrAddresses(whitelist.ipAddressCidr);
    }

    if (whitelist.startIpAddress && whitelist.endIpAddress) {
      const start = this.ipv4ToNumber(whitelist.startIpAddress);
      const end = this.ipv4ToNumber(whitelist.endIpAddress);

      if (start === null || end === null || end < start) {
        return null;
      }

      return (end - start) + 1;
    }

    return null;
  }

  private countCidrAddresses(cidr: string): number | null {
    const [ip, prefixText] = cidr.split("/");
    const prefix = Number(prefixText);
    const baseIp = this.ipv4ToNumber(ip);

    if (baseIp === null || !Number.isInteger(prefix) || prefix < 0 || prefix > 32) {
      return null;
    }

    return 2 ** (32 - prefix);
  }

  private ipv4ToNumber(ip: string): number | null {
    const octets = ip.split(".");
    if (octets.length !== 4) {
      return null;
    }

    let result = 0;
    for (const octetText of octets) {
      if (!/^\d+$/.test(octetText)) {
        return null;
      }

      const octet = Number(octetText);
      if (octet < 0 || octet > 255) {
        return null;
      }

      result = (result * 256) + octet;
    }

    return result;
  }

  private ipv4Validator(): ValidatorFn {
    return (control: AbstractControl): ValidationErrors | null => {
      const value = String(control.value || "").trim();
      if (!value) {
        return null;
      }

      return this.ipv4ToNumber(value) === null ? { invalidIp: true } : null;
    };
  }

  private cidrValidator(): ValidatorFn {
    return (control: AbstractControl): ValidationErrors | null => {
      const value = String(control.value || "").trim();
      if (!value) {
        return null;
      }

      return this.countCidrAddresses(value) === null ? { invalidCidr: true } : null;
    };
  }

  private whitelistRowValidator(): ValidatorFn {
    return (control: AbstractControl): ValidationErrors | null => {
      const group = control as FormGroup;
      const start = String(group.get("startIpAddress")?.value || "").trim();
      const end = String(group.get("endIpAddress")?.value || "").trim();
      const cidr = String(group.get("ipAddressCidr")?.value || "").trim();
      const hasStart = !!start;
      const hasEnd = !!end;
      const hasCidr = !!cidr;
      const hasRange = hasStart || hasEnd;

      if (!hasRange && !hasCidr) {
        return null;
      }

      if (hasRange && hasCidr) {
        return { mixedWhitelistMode: true };
      }

      if (hasStart !== hasEnd) {
        return { incompleteRange: true };
      }

      if (hasStart && hasEnd) {
        const startValue = this.ipv4ToNumber(start);
        const endValue = this.ipv4ToNumber(end);

        if (startValue !== null && endValue !== null && endValue < startValue) {
          return { invalidRangeOrder: true };
        }
      }

      return null;
    };
  }
}
