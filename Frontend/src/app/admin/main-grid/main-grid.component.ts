import { Component, OnInit } from "@angular/core";
import { MODIFY_DATA } from "../../shared/mocks";
import { ApiCallService } from "../../services/api-call.service";
import { CommonApiService } from "../../services/common-api.service";
import { BODY_DATA } from "../../shared/constants";
import { SharedService } from "src/app/services/sharedService";
import { NotificationService } from "src/app/services/notification.service";
import { Subject } from "rxjs";
import { debounceTime } from "rxjs/operators";
import { PageEvent } from "@angular/material/paginator";
import { AuthService } from "src/app/services/auth.service";

interface ISearchQuery {
  firstName: string;
  lastName: string;
  email: string;
  contact: string;
  dateOfBirth: string;
  addressLine1: string;
  addressLine2: string;
  city: string;
  state: string;
  zip: string;
  ssn: string;
  linkId: string;
  sourceSystemId: string;
  sourceSystemName: string;
}

interface IGroupedResultRow {
  groupKey: string;
  mpiLinkId: string;
  sourceName: string;
  sourceSystemId: string;
  isChecked: boolean;
  sourceRecords: any[];
  hide?: boolean;
}

interface ILinkGraphNode {
  id: string;
  label: string;
  type: "mpi" | "source";
  meta?: string;
}

interface ILinkGraphEdge {
  from: string;
  to: string;
  relationship: string;
}

interface ILinkGraphPayload {
  mpiLinkId: string;
  nodes: ILinkGraphNode[];
  edges: ILinkGraphEdge[];
  sourceRecords: any[];
}

interface IRenderedLinkGraphNode extends ILinkGraphNode {
  x: number;
  y: number;
}

@Component({
  selector: "app-main-grid",
  templateUrl: "./main-grid.component.html",
  styleUrls: ["./main-grid.component.scss"],
})
export class MainGridComponent implements OnInit {
  readonly compareFields: Array<{ key: string; label: string; sensitive?: boolean }> = [
    { key: "mpiLinkId", label: "MPI Link ID" },
    { key: "sourceSystemId", label: "Source System ID" },
    { key: "sourceName", label: "Source System Name" },
    { key: "tenant", label: "Tenant" },
    { key: "sourceSystemLastUpdate", label: "Source System Last Updated" },
    { key: "firstName", label: "First Name" },
    { key: "middleName", label: "Middle Name" },
    { key: "lastName", label: "Last Name" },
    { key: "suffix", label: "Suffix" },
    { key: "birthDate", label: "Date of Birth", sensitive: true },
    { key: "gender", label: "Gender" },
    { key: "ssn", label: "SSN", sensitive: true },
    { key: "emailAddress", label: "Email" },
    { key: "addressType", label: "Address Type" },
    { key: "addressLine1", label: "Address Line 1" },
  ];

  selectedItems: Record<string, string | undefined> = {};
  selectedGroupedRecords: Record<string, any[]> = {};
  data: any[] = [];
  pageData: any;
  sortedData: IGroupedResultRow[] = [];
  modifySortedData: any[] = [];
  modifyQueue: any[] = MODIFY_DATA;
  modifySelectedItems = new Set<number>();
  modifySelectedRecords: any[] = [];
  totalRecords = 0;
  pageEvent!: PageEvent;
  public loading = false;
  searchQuery: ISearchQuery = this.createEmptySearchQuery();
  filterValues: ISearchQuery = this.createEmptySearchQuery();
  filterTextChanged: Subject<ISearchQuery> = new Subject<ISearchQuery>();
  filtered: string[] = [];
  orderBy = "";
  sortField = "mpiLinkId";
  sortDirection: "asc" | "desc" = "desc";
  detailRow: any = null;
  showGroupedRecords = false;
  activeResultsModal: "compare" | "graph" | null = null;
  activeModifyModal: "compare" | "delete" | "merge" | "link" | "unlink" | "unmerge" | null = null;
  activeWorkspace: "results" | "modify" = "results";
  graphLoading = false;
  linkGraphPayload: ILinkGraphPayload | null = null;
  graphViewMode: "visual" | "json" = "visual";
  graphHoveredNodeId: string | null = null;
  graphSelectedNodeId: string | null = null;

  constructor(
    public apiCallService: ApiCallService,
    public commonApiService: CommonApiService,
    public sharedService: SharedService,
    private notifyService: NotificationService,
    public authService: AuthService
  ) {}

  ngOnInit(): void {
    this.resetFilters();
    this.pageData = this.apiCallService.dashboardPagefilter.getValue();
    this.filterTextChanged.pipe(debounceTime(400)).subscribe((searchQuery) => {
      this.searchQuery = searchQuery;
      this.callSearchApi();
    });
    this.applySort();
  }

  onPaginationChange(event: any): void {
    this.apiCallService.dashboardPagefilter.next(event);
    this.pageData = this.apiCallService.dashboardPagefilter.getValue();
    this.getIdentities();
  }

  getIdentities(): void {
    this.loading = true;
    const { pageIndex, pageSize }: any = this.apiCallService.dashboardPagefilter.getValue();

    this.commonApiService.getIdeitities(pageIndex, pageSize, this.searchQuery, this.orderBy).subscribe((res) => {
      const { data, recordsCount }: any = res;
      this.totalRecords = recordsCount;
      this.data = data || [];
      this.sortedData = this.groupResults(this.data);
      this.pageData = this.apiCallService.dashboardPagefilter.getValue();
      this.loading = false;
    });
  }

  onInlineFilterChange(): void {
    this.filterTextChanged.next({ ...this.filterValues });
  }

  callSearchApi(): void {
    this.resetPagination();
    this.getIdentities();
  }

  resetFilters(): void {
    this.searchQuery = this.createEmptySearchQuery();
    this.filterValues = this.createEmptySearchQuery();
  }

  resetPagination(): void {
    this.apiCallService.dashboardPagefilter.next({
      pageIndex: 0,
      pageSize: 20,
    });
  }

  selectItem(item: IGroupedResultRow): void {
    const checked = !item.isChecked;

    this.sortedData = this.sortedData.map((row) =>
      row.groupKey === item.groupKey ? { ...row, isChecked: checked } : row
    );

    if (checked) {
      this.selectedGroupedRecords[item.groupKey] = [...item.sourceRecords];
    } else {
      delete this.selectedGroupedRecords[item.groupKey];
    }

    item.sourceRecords.forEach((record: any) => {
      this.selectedItems[record.id] = checked ? item.groupKey : undefined;
    });
  }

  shouldShowButtons(): boolean {
    const role = this.authService.getRole();
    return role === "Admin";
  }

  canSelectRows(): boolean {
    const role = this.authService.getRole();
    return role === "Admin" || role === "Source" || role === "Viewer" || role === "SourceUser";
  }

  shouldEnableButton(): boolean {
    return this.selectedSystemCount() > 0;
  }

  selectToggleAll(): void {
    const shouldCheckAll = !this.isAllChecked();
    this.sortedData = this.sortedData.map((item) => {
      if (shouldCheckAll) {
        this.selectedGroupedRecords[item.groupKey] = [...item.sourceRecords];
      } else {
        delete this.selectedGroupedRecords[item.groupKey];
      }

      item.sourceRecords.forEach((record: any) => {
        this.selectedItems[record.id] = shouldCheckAll ? item.groupKey : undefined;
      });
      return { ...item, isChecked: shouldCheckAll };
    });
  }

  isAllChecked(): boolean {
    return !!this.sortedData.length && this.sortedData.every((el: any) => el.isChecked);
  }

  clearInlineFilters(): void {
    this.resetFilters();
    this.callSearchApi();
  }

  hasActiveFilters(): boolean {
    return Object.values(this.filterValues).some((value) => !!`${value || ""}`.trim());
  }

  toggleSort(field: string): void {
    if (this.sortField === field) {
      this.sortDirection = this.sortDirection === "asc" ? "desc" : "asc";
    } else {
      this.sortField = field;
      this.sortDirection = "desc";
    }

    this.applySort();
  }

  isSortedBy(field: string): boolean {
    return this.sortField === field;
  }

  sortIndicator(field: string): string {
    if (!this.isSortedBy(field)) {
      return "Sort";
    }

    return this.sortDirection === "asc" ? "Ascending" : "Descending";
  }

  applySort(): void {
    this.resetPagination();
    this.orderBy = this.sortField ? `${this.sortField} ${this.sortDirection}` : "";
    this.getIdentities();
  }

  showSensitiveData(): boolean {
    return this.authService.getRole() == "Admin";
  }

  isViewerRole(): boolean {
    return this.authService.getRole() === "Viewer";
  }

  get hasTwoResultSelections(): boolean {
    return this.selectedResultRows().length === 2;
  }

  get hasSingleModifySelection(): boolean {
    return this.modifySelectedItems.size === 1;
  }

  get hasTwoModifySelections(): boolean {
    return this.modifySelectedItems.size === 2;
  }

  openDetails(item: IGroupedResultRow): void {
    this.detailRow = item;
    this.showGroupedRecords = false;
  }

  openModifyDetails(item: any): void {
    this.detailRow = {
      groupKey: this.buildGroupKey(item),
      mpiLinkId: item.mpiLinkId || "--",
      sourceName: item.sourceName || "--",
      sourceSystemId: item.sourceSystemId || "--",
      isChecked: false,
      sourceRecords: [item],
    };
    this.showGroupedRecords = false;
  }

  closeDetails(): void {
    this.detailRow = null;
    this.showGroupedRecords = false;
  }

  toggleGroupedRecords(): void {
    this.showGroupedRecords = !this.showGroupedRecords;
  }

  openResultsModal(type: "compare"): void {
    this.activeResultsModal = type;
  }

  openLinkGraph(item: IGroupedResultRow): void {
    if (!item.mpiLinkId || item.mpiLinkId === "--") {
      this.sharedService.showToast("MPI Link ID is not available for this row");
      return;
    }

    this.activeResultsModal = "graph";
    this.graphLoading = true;
    this.linkGraphPayload = null;
    this.graphViewMode = "visual";
    this.graphHoveredNodeId = null;
    this.graphSelectedNodeId = null;

    this.commonApiService.getMatchingLinkIdsReport(0, 1000, "", item.mpiLinkId).subscribe({
      next: (response: any) => {
        const data = response?.data || {};
        const linkKey = Object.keys(data)[0] || item.mpiLinkId;
        const records = (data[linkKey] || []).map((record: any) => ({
          ...record,
          sourceName: record.sourceName || record.SourceName || "--",
          sourceSystemId: record.sourceSystemId || record.SourceSystemId || "--",
          mpiLinkId: record.mpiLinkId || record.MPILinkId || linkKey,
          firstName: record.firstName || record.FirstName || "--",
          middleName: record.middleName || record.MiddleName || "--",
          lastName: record.lastName || record.LastName || "--",
          gender: record.gender || record.Gender || "--",
        }));

        this.linkGraphPayload = this.buildLinkGraph(linkKey, records);
        this.graphLoading = false;
      },
      error: () => {
        this.graphLoading = false;
        this.linkGraphPayload = null;
        this.sharedService.showToast("Unable to load MPI link graph");
      }
    });
  }

  closeResultsModal(): void {
    this.activeResultsModal = null;
    this.graphLoading = false;
    this.linkGraphPayload = null;
    this.graphViewMode = "visual";
    this.graphHoveredNodeId = null;
    this.graphSelectedNodeId = null;
  }

  openModifyModal(type: "compare" | "delete" | "merge" | "link" | "unlink" | "unmerge"): void {
    if (this.loading) {
      return;
    }

    this.modifySelectedRecords = this.getSelectedModifyData();
    this.activeModifyModal = type;
  }

  closeModifyModal(): void {
    if (this.loading) {
      return;
    }

    this.activeModifyModal = null;
  }

  operationLoadingLabel(): string {
    switch (this.activeModifyModal) {
      case "delete": return "Deleting record...";
      case "merge": return "Merging records...";
      case "unmerge": return "Unmerging records...";
      case "link": return "Linking records...";
      case "unlink": return "Unlinking records...";
      default: return "Operation in progress...";
    }
  }

  selectModifyItem(event: any, item: any): void {
    const value = Number(event.target.value);

    if (this.modifySelectedItems.has(value)) {
      this.modifySelectedItems.delete(value);
      this.modifySelectedRecords = this.modifySelectedRecords.filter((record: any) => record.id !== value);
      return;
    }

    if (this.modifySelectedItems.size >= 2) {
      event.target.checked = false;
      this.sharedService.showToast("Reached maximum limit of select count");
      event.preventDefault();
      event.stopPropagation();
      return;
    }

    this.modifySelectedItems.add(value);
    this.modifySelectedRecords.push(item);
  }

  detailRows(): Array<{ label: string; value: string }> {
    if (!this.detailRow) {
      return [];
    }

    const baseRecord = this.detailRow.sourceRecords?.[0] || {};

    return [
      { label: "MPI Link ID", value: this.detailRow.mpiLinkId || "--" },
      { label: "Source System Name", value: this.detailRow.sourceName || "--" },
      { label: "Tenant", value: baseRecord.tenant || "--" },
      { label: "Source System ID", value: this.detailRow.sourceSystemId || "--" },
      { label: "Representative MPI Link ID", value: baseRecord.mpiLinkId || "--" },
      { label: "First Name", value: baseRecord.firstName || "--" },
      { label: "Middle Name", value: baseRecord.middleName || "--" },
      { label: "Last Name", value: baseRecord.lastName || "--" },
      { label: "Suffix", value: baseRecord.suffix || "--" },
      { label: "Date of Birth", value: baseRecord.birthDate || "--" },
      { label: "Gender", value: baseRecord.gender || "--" },
      { label: "SSN", value: this.showSensitiveData() ? baseRecord.ssn || "--" : "Sensitive fields masked" },
      { label: "Email", value: baseRecord.emailAddress || "--" },
      { label: "Address Type", value: baseRecord.addressType || "--" },
      { label: "Address Line 1", value: baseRecord.addressLine1 || "--" },
      { label: "Address Line 2", value: baseRecord.addressLine2 || "--" },
      { label: "City", value: baseRecord.city || "--" },
      { label: "State", value: baseRecord.state || "--" },
      { label: "ZIP", value: baseRecord.zip || "--" },
      { label: "Source System Last Updated", value: baseRecord.sourceSystemLastUpdate || "--" },
      { label: "Grouped Records", value: `${this.detailRow.sourceRecords?.length || 0}` },
    ];
  }

  detailDifferences(): Array<{ label: string; values: string[] }> {
    if (!this.detailRow?.sourceRecords?.length) {
      return [];
    }

    return this.compareFields
      .map((field) => {
        const values = Array.from(
          new Set(
            this.detailRow.sourceRecords
              .map((record: any) => this.getCompareValue(record, field.key))
              .filter((value: string) => value && value !== "--")
          )
        ) as string[];

        return {
          label: field.label,
          values,
        };
      })
      .filter((field) => field.values.length > 1);
  }

  groupedRecordRows(): Array<{ label: string; value: string }[]> {
    if (!this.detailRow?.sourceRecords?.length) {
      return [];
    }

    return this.detailRow.sourceRecords.map((record: any, index: number) => [
      { label: "Record", value: `${index + 1}` },
      { label: "MPI Link ID", value: record.mpiLinkId || "--" },
      { label: "Tenant", value: record.tenant || "--" },
      { label: "First Name", value: record.firstName || "--" },
      { label: "Middle Name", value: record.middleName || "--" },
      { label: "Last Name", value: record.lastName || "--" },
      { label: "Suffix", value: record.suffix || "--" },
      { label: "Date of Birth", value: record.birthDate || "--" },
      { label: "Gender", value: record.gender || "--" },
      { label: "SSN", value: this.showSensitiveData() ? record.ssn || "--" : "Sensitive fields masked" },
      { label: "Email", value: record.emailAddress || "--" },
      { label: "Address Type", value: record.addressType || "--" },
      { label: "Address Line 1", value: record.addressLine1 || "--" },
      { label: "Address Line 2", value: record.addressLine2 || "--" },
      { label: "City", value: record.city || "--" },
      { label: "State", value: record.state || "--" },
      { label: "ZIP", value: record.zip || "--" },
      { label: "Last Updated", value: record.sourceSystemLastUpdate || "--" },
    ]);
  }

  getCompareValue(item: any, key: string): string {
    if (!item) {
      return "--";
    }

    const value = item[key];
    if (!value) {
      return "--";
    }

    if (key === "ssn" && !this.showSensitiveData()) {
      return "Sensitive fields masked";
    }

    return value;
  }

  isCompareFieldDifferent(key: string): boolean {
    if (this.modifySelectedRecords.length < 2) {
      return false;
    }

    const first = this.modifySelectedRecords[0]?.[key] ?? "";
    const second = this.modifySelectedRecords[1]?.[key] ?? "";
    return first !== second;
  }

  compareDifferenceCount(): number {
    return this.compareFields.filter((field) => this.isCompareFieldDifferent(field.key)).length;
  }

  resultCompareDifferenceCount(): number {
    return this.compareFields.filter((field) => this.isResultCompareFieldDifferent(field.key)).length;
  }

  linkGraphJson(): string {
    return JSON.stringify(this.linkGraphPayload, null, 2);
  }

  setGraphViewMode(mode: "visual" | "json"): void {
    this.graphViewMode = mode;
  }

  renderedGraphNodes(): IRenderedLinkGraphNode[] {
    if (!this.linkGraphPayload?.nodes?.length) {
      return [];
    }

    const width = 760;
    const height = 420;
    const centerX = width / 2;
    const centerY = height / 2;
    const radius = 150;
    const sourceNodes = this.linkGraphPayload.nodes.slice(1);

    const nodes: IRenderedLinkGraphNode[] = [
      {
        ...this.linkGraphPayload.nodes[0],
        x: centerX,
        y: centerY,
      },
    ];

    sourceNodes.forEach((node, index) => {
      const angle = (-Math.PI / 2) + ((Math.PI * 2) * index / Math.max(sourceNodes.length, 1));
      nodes.push({
        ...node,
        x: centerX + (Math.cos(angle) * radius),
        y: centerY + (Math.sin(angle) * radius),
      });
    });

    return nodes;
  }

  graphNodeById(nodeId: string | null): IRenderedLinkGraphNode | null {
    if (!nodeId) {
      return null;
    }

    return this.renderedGraphNodes().find((node) => node.id === nodeId) || null;
  }

  graphCenterNode(): IRenderedLinkGraphNode | null {
    return this.renderedGraphNodes()[0] || null;
  }

  graphSourceNodes(): IRenderedLinkGraphNode[] {
    return this.renderedGraphNodes().slice(1);
  }

  activeGraphNode(): IRenderedLinkGraphNode | null {
    return this.graphNodeById(this.graphSelectedNodeId || this.graphHoveredNodeId) || this.graphCenterNode();
  }

  hoverGraphNode(nodeId: string | null): void {
    this.graphHoveredNodeId = nodeId;
  }

  selectGraphNode(nodeId: string): void {
    this.graphSelectedNodeId = this.graphSelectedNodeId === nodeId ? null : nodeId;
  }

  graphEdgeActive(nodeId: string): boolean {
    const activeNodeId = this.graphSelectedNodeId || this.graphHoveredNodeId;
    if (!activeNodeId) {
      return false;
    }

    const centerNodeId = this.graphCenterNode()?.id;
    return activeNodeId === nodeId || activeNodeId === centerNodeId;
  }

  graphNodeRecords(node: IRenderedLinkGraphNode | null): any[] {
    if (!node || !this.linkGraphPayload) {
      return [];
    }

    if (node.type === "mpi") {
      return this.linkGraphPayload.sourceRecords;
    }

    return this.linkGraphPayload.sourceRecords.filter((record: any) =>
      `source:${record.sourceName || "--"}::${record.sourceSystemId || "--"}` === node.id
    );
  }

  graphNodeLabelLines(node: IRenderedLinkGraphNode): string[] {
    if (node.type === "mpi") {
      return ["MPI Link", this.truncateMiddle(node.label, 18)];
    }

    const sourceLabel = node.label || "--";
    const segments = sourceLabel
      .replace(/([.\-_])/g, "$1 ")
      .split(/\s+/)
      .filter(Boolean);

    if (!segments.length) {
      return ["--"];
    }

    const lines: string[] = [];
    let currentLine = "";

    for (const segment of segments) {
      const candidate = currentLine ? `${currentLine}${segment}` : segment;
      if (candidate.length <= 13) {
        currentLine = candidate;
        continue;
      }

      if (currentLine) {
        lines.push(currentLine);
      }
      currentLine = segment;

      if (lines.length === 1) {
        break;
      }
    }

    if (currentLine && lines.length < 2) {
      lines.push(currentLine);
    }

    return lines.slice(0, 2).map((line, index, all) => {
      if (index === all.length - 1 && segments.join("").length > all.join("").length) {
        return this.truncateWithEllipsis(line, 13);
      }

      return this.truncateWithEllipsis(line, 13);
    });
  }

  graphNodeMetaLine(node: IRenderedLinkGraphNode): string {
    if (node.type === "mpi") {
      return `${this.graphNodeRecords(node).length} records`;
    }

    return this.truncateMiddle(node.meta || "--", 14);
  }

  graphNodeLabelY(node: IRenderedLinkGraphNode): number {
    return node.type === "mpi" ? node.y - 12 : node.y - 10;
  }

  graphNodeMetaY(node: IRenderedLinkGraphNode): number {
    return node.type === "mpi" ? node.y + 18 : node.y + 16;
  }

  private truncateWithEllipsis(value: string, maxLength: number): string {
    if (!value || value.length <= maxLength) {
      return value;
    }

    return `${value.slice(0, Math.max(0, maxLength - 1))}…`;
  }

  private truncateMiddle(value: string, maxLength: number): string {
    if (!value || value.length <= maxLength) {
      return value;
    }

    const sideLength = Math.max(3, Math.floor((maxLength - 1) / 2));
    return `${value.slice(0, sideLength)}…${value.slice(-sideLength)}`;
  }

  isResultCompareFieldDifferent(key: string): boolean {
    const rows = this.selectedResultRows();
    if (rows.length < 2) {
      return false;
    }

    const first = rows[0]?.sourceRecords?.[0]?.[key] ?? "";
    const second = rows[1]?.sourceRecords?.[0]?.[key] ?? "";
    return first !== second;
  }

  moveToModify(): void {
    if (!this.shouldEnableButton()) return;

    this.loading = true;
    const selectedGroupKeys = Object.keys(this.selectedGroupedRecords);
    this.filtered = selectedGroupKeys;
    const selectedRecords = selectedGroupKeys.flatMap((groupKey) => this.selectedGroupedRecords[groupKey] || []);

    for (const record of selectedRecords) {
      const isModifiedNotExists = MODIFY_DATA.every((value) => value.id !== record.id);
      if (isModifiedNotExists) {
        MODIFY_DATA.push(record);
      }
    }

    const ID_LIST: any = MODIFY_DATA.map((el: any) => el.id);
    sessionStorage.setItem("selectedRecords", ID_LIST);
    this.sharedService.showToast("Moved records to modify");

    this.modifySortedData = this.data.filter((el: any) => !this.filtered.includes(this.buildGroupKey(el)));
    this.sortedData = this.groupResults(this.modifySortedData);
    this.modifyQueue = MODIFY_DATA;
    this.activeWorkspace = "modify";
    this.loading = false;
  }

  deleteSelected(): void {
    if (this.loading || !this.hasSingleModifySelection) {
      return;
    }

    this.loading = true;
    const selectedRecord = this.getSelectedModifyData()[0];
    const bodyData = {
      content: {
        name: selectedRecord.sourceName,
        id: selectedRecord.sourceSystemId,
      },
    };

    this.commonApiService.delete(bodyData).subscribe(
      () => {
        this.notifyService.showSuccess("Deleted record successfully.", "");
        this.removeSelectedModifyRecords();
      },
      (err) => {
        this.notifyService.showError(this.extractErrorMessage(err, "Unable to delete record"), "");
        this.loading = false;
      }
    );
  }

  linkSelected(): void {
    if (this.loading || !this.hasTwoModifySelections) {
      return;
    }

    this.loading = true;
    const data = this.getSelectedModifyData();
    const bodyData = {
      linkToSource: {
        name: data[0].sourceName,
        id: data[0].sourceSystemId,
      },
      source: {
        name: data[1].sourceName,
        id: data[1].sourceSystemId,
      },
    };

    this.commonApiService.link(bodyData).subscribe(
      () => {
        this.notifyService.showSuccess("Linked records successfully.", "");
        this.removeSelectedModifyRecords();
      },
      (err) => {
        this.notifyService.showError(this.extractErrorMessage(err, "Unable to link records"), "");
        this.loading = false;
      }
    );
  }

  unlinkSelected(): void {
    if (this.loading || !this.hasTwoModifySelections) {
      return;
    }

    this.loading = true;
    const data = this.getSelectedModifyData();
    const bodyData = {
      unlinkFromSource: {
        name: data[0].sourceName,
        id: data[0].sourceSystemId,
      },
      source: {
        name: data[1].sourceName,
        id: data[1].sourceSystemId,
      },
    };

    this.commonApiService.unlink(bodyData).subscribe(
      () => {
        this.notifyService.showSuccess("Unlinked records successfully.", "");
        this.removeSelectedModifyRecords();
      },
      (err) => {
        this.notifyService.showError(this.extractErrorMessage(err, "Unable to unlink records"), "");
        this.loading = false;
      }
    );
  }

  mergeSelected(): void {
    if (this.loading || !this.hasTwoModifySelections) {
      return;
    }

    this.loading = true;
    const data = this.getSelectedModifyData();
    const bodyData = {
      toSurviveSource: {
        name: data[0].sourceName,
        id: data[0].sourceSystemId,
      },
      toRetireSource: {
        name: data[1].sourceName,
        id: data[1].sourceSystemId,
      },
    };

    this.commonApiService.merge(bodyData).subscribe(
      () => {
        this.notifyService.showSuccess("Merged records successfully.", "");
        this.removeSelectedModifyRecords();
      },
      (err) => {
        this.notifyService.showError(this.extractErrorMessage(err, "Unable to merge records"), "");
        this.loading = false;
      }
    );
  }

  unmergeSelected(): void {
    if (this.loading || !this.hasTwoModifySelections) {
      return;
    }

    this.loading = true;
    const data = this.getSelectedModifyData();
    const bodyData = {
      unmergeFromSource: {
        name: data[0].sourceName,
        id: data[0].sourceSystemId,
      },
      unmergeSource: {
        name: data[1].sourceName,
        id: data[1].sourceSystemId,
      },
    };

    this.commonApiService.unmerge(bodyData).subscribe(
      () => {
        this.notifyService.showSuccess("Unmerged records successfully.", "");
        this.removeSelectedModifyRecords();
      },
      (err) => {
        this.notifyService.showError(this.extractErrorMessage(err, "Unable to unmerge records"), "");
        this.loading = false;
      }
    );
  }

  resultSummaryCards(): Array<{ label: string; value: string; detail: string }> {
    return [
      {
        label: "Total Results",
        value: `${this.totalRecords}`,
        detail: "Matches from the current filter set",
      },
      {
        label: "Rows On Page",
        value: `${this.sortedData.length}`,
        detail: `Page ${Number(this.pageData?.pageIndex || 0) + 1} of results`,
      },
      {
        label: "Selected Systems",
        value: `${this.selectedSystemCount()}`,
        detail: "Unique source systems selected for action",
      },
    ];
  }

  modifySummaryCards(): Array<{ label: string; value: string; detail: string }> {
    return [
      {
        label: "Queued Records",
        value: `${this.modifyQueue.length}`,
        detail: "Records staged for modify actions",
      },
      {
        label: "Selected",
        value: `${this.modifySelectedItems.size}`,
        detail: "Records currently selected in the queue",
      },
    ];
  }

  showResultsWorkspace(): void {
    this.activeWorkspace = "results";
  }

  showModifyWorkspace(): void {
    this.activeWorkspace = "modify";
  }

  clearModifyQueue(): void {
    this.modifyQueue = [];
    MODIFY_DATA.splice(0);
    this.modifySelectedItems = new Set<number>();
    this.modifySelectedRecords = [];
    this.selectedItems = {};
    this.selectedGroupedRecords = {};
    this.sortedData = this.sortedData.map((item: any) => ({
      ...item,
      isChecked: false,
    }));
    this.activeModifyModal = null;
    sessionStorage.removeItem("selectedRecords");
    this.sharedService.showToast("Cleared modify queue");
  }

  trackByRow(_: number, item: any): string {
    return item.groupKey || `${item.id}-${item.sourceSystemId}`;
  }

  selectedResultRows(): IGroupedResultRow[] {
    return Object.entries(this.selectedGroupedRecords).map(([groupKey, records]) => {
      const firstRecord = records?.[0] || {};
      return {
        groupKey,
        mpiLinkId: firstRecord.mpiLinkId || "--",
        sourceName: firstRecord.sourceName || "--",
        sourceSystemId: firstRecord.sourceSystemId || "--",
        isChecked: true,
        sourceRecords: records || [],
      };
    });
  }

  private getSelectedModifyData(): any[] {
    return this.modifyQueue.filter((item: any) => this.modifySelectedItems.has(item.id));
  }

  private createEmptySearchQuery(): ISearchQuery {
    return {
      firstName: BODY_DATA.firstName || "",
      lastName: BODY_DATA.lastName || "",
      email: BODY_DATA.email || "",
      contact: BODY_DATA.contact || "",
      dateOfBirth: BODY_DATA.dateOfBirth || "",
      addressLine1: BODY_DATA.addressLine1 || "",
      addressLine2: BODY_DATA.addressLine2 || "",
      city: BODY_DATA.city || "",
      state: BODY_DATA.state || "",
      zip: BODY_DATA.zip || "",
      ssn: BODY_DATA.ssn || "",
      linkId: BODY_DATA.linkId || "",
      sourceSystemId: BODY_DATA.sourceSystemId || "",
      sourceSystemName: BODY_DATA.sourceSystemName || "",
    };
  }

  private isSourceSystemSelected(groupKey: string): boolean {
    return !!this.selectedGroupedRecords[groupKey];
  }

  private selectedSystemCount(): number {
    return Object.keys(this.selectedGroupedRecords).length;
  }

  private buildGroupKey(item: any): string {
    return `${(item.sourceName || "").trim().toLowerCase()}::${item.sourceSystemId || ""}`;
  }

  private groupResults(items: any[]): IGroupedResultRow[] {
    const grouped = new Map<string, IGroupedResultRow>();

    items.forEach((item: any) => {
      const groupKey = this.buildGroupKey(item);
      const existing = grouped.get(groupKey);

      if (existing) {
        existing.sourceRecords.push(item);
        return;
      }

      grouped.set(groupKey, {
        groupKey,
        mpiLinkId: item.mpiLinkId || "--",
        sourceName: item.sourceName || "--",
        sourceSystemId: item.sourceSystemId || "--",
        isChecked: this.isSourceSystemSelected(groupKey),
        sourceRecords: [item],
      });
    });

    return Array.from(grouped.values());
  }

  private buildLinkGraph(mpiLinkId: string, records: any[]): ILinkGraphPayload {
    const mpiNodeId = `mpi:${mpiLinkId}`;
    const sourceNodes = new Map<string, ILinkGraphNode>();
    const edges = new Map<string, ILinkGraphEdge>();

    records.forEach((record: any) => {
      const sourceName = record.sourceName || "--";
      const sourceSystemId = record.sourceSystemId || "--";
      const nodeId = `source:${sourceName}::${sourceSystemId}`;

      if (!sourceNodes.has(nodeId)) {
        sourceNodes.set(nodeId, {
          id: nodeId,
          label: sourceName,
          type: "source",
          meta: sourceSystemId,
        });
      }

      if (!edges.has(nodeId)) {
        edges.set(nodeId, {
          from: mpiNodeId,
          to: nodeId,
          relationship: "connected-source",
        });
      }
    });

    return {
      mpiLinkId,
      nodes: [
        {
          id: mpiNodeId,
          label: mpiLinkId,
          type: "mpi",
          meta: `${records.length} connected records`,
        },
        ...Array.from(sourceNodes.values()),
      ],
      edges: Array.from(edges.values()),
      sourceRecords: records,
    };
  }

  private removeSelectedModifyRecords(): void {
    const selectedIds = new Set(this.modifySelectedItems);
    const selectedGroupKeys = new Set(
      this.modifyQueue
        .filter((item: any) => selectedIds.has(item.id))
        .map((item: any) => this.buildGroupKey(item))
    );

    this.modifyQueue = this.modifyQueue.filter((item: any) => !selectedIds.has(item.id));
    MODIFY_DATA.splice(0);
    MODIFY_DATA.push(...this.modifyQueue);
    this.data = this.data.filter((item: any) => !selectedGroupKeys.has(this.buildGroupKey(item)));
    this.sortedData = this.groupResults(this.data);
    this.modifySelectedItems = new Set<number>();
    this.modifySelectedRecords = [];
    this.selectedItems = Object.fromEntries(
      Object.entries(this.selectedItems).filter(([id, groupKey]) => !selectedIds.has(Number(id)) && !selectedGroupKeys.has(`${groupKey || ""}`))
    );
    Object.keys(this.selectedGroupedRecords).forEach((groupKey) => {
      if (selectedGroupKeys.has(groupKey)) {
        delete this.selectedGroupedRecords[groupKey];
      }
    });
    this.loading = false;
    this.closeModifyModal();
  }

  private extractErrorMessage(error: any, fallback: string): string {
    const errorPayload = error?.error;
    const messageParts: string[] = [];

    const primaryMessage =
      errorPayload?.message ||
      errorPayload?.error ||
      errorPayload?.title ||
      errorPayload?.detail ||
      error?.message ||
      error?.statusText;

    if (typeof primaryMessage === "string" && primaryMessage.trim()) {
      messageParts.push(primaryMessage.trim());
    }

    if (Array.isArray(errorPayload?.errors)) {
      const validationMessages = errorPayload.errors
        .filter((entry: unknown) => typeof entry === "string" && entry.trim())
        .map((entry: string) => entry.trim());

      if (validationMessages.length) {
        messageParts.push(validationMessages.join(", "));
      }
    }

    if (messageParts.length) {
      return messageParts.join(" ");
    }

    return fallback;
  }
}
