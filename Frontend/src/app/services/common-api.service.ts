import { Injectable, OnInit } from "@angular/core";
import { HttpClient, HttpHeaders, HttpParams } from "@angular/common/http";
import { Observable, of, BehaviorSubject } from "rxjs";
import { AppSettingsService } from "./app.settings.service";
import { ThisReceiver } from "@angular/compiler";
import { environment } from "src/environments/environment";

export interface UserInfo {
  "custom:groups": string;
  given_name: string;
  family_name: string;
  picture?: string;
}

export interface AdminSummary {
  healthStatus: string;
  avgLatencyMs: number;
  p95LatencyMs: number;
  errorRate: number;
  requestsPerMinute: number;
  lambdaConcurrency: number;
  timeoutCount: number;
  dependencyLatencyMs: number;
}

export interface AdminConfiguredThresholds {
  warningLatencyMs: number;
  criticalLatencyMs: number;
  warningErrorRatePercent: number;
  criticalErrorRatePercent: number;
  warningTimeoutCount: number;
  criticalTimeoutCount: number;
  warningConcurrency: number;
  criticalConcurrency: number;
}

export interface AdminTrendPoint {
  timestamp: string;
  [key: string]: string | number;
}

export interface AdminAlert {
  severity: string;
  message: string;
}

export interface AdminCustomErrorSummary {
  totalErrors: number;
  distinctErrorCodes: number;
  distinctFunctions: number;
  distinctTrackingIds: number;
}

export interface AdminCustomErrorEntry {
  timestamp?: string | null;
  name?: string | null;
  layer?: string | null;
  functionName?: string | null;
  errorCode?: string | null;
  message?: string | null;
  trackingId?: string | null;
  logStream?: string | null;
  logGroup?: string | null;
}

export interface AdminCustomErrorCollection {
  summary: AdminCustomErrorSummary;
  entries: AdminCustomErrorEntry[];
}

export interface AdminVeratoErrorSummary {
  totalErrors: number;
  distinctOperations: number;
  distinctTrackingIds: number;
  totalRetries: number;
}

export interface AdminVeratoErrorTrendPoint {
  timestamp: string;
  count: number;
  label?: string | null;
}

export interface AdminVeratoErrorEntry {
  timestamp: string;
  trackingId: string;
  apiCallType: string;
  userName: string;
  status: string;
  message: string;
  retryCount: number;
  responseJson?: string | null;
}

export interface AdminVeratoErrorCollection {
  summary: AdminVeratoErrorSummary;
  trend: AdminVeratoErrorTrendPoint[];
  entries: AdminVeratoErrorEntry[];
}

export interface AdminDashboardResponse {
  summary: AdminSummary;
  thresholds: AdminConfiguredThresholds;
  trends: {
    latency: AdminTrendPoint[];
    throughput: AdminTrendPoint[];
    errors: AdminTrendPoint[];
    timeouts: AdminTrendPoint[];
    concurrency: AdminTrendPoint[];
  };
  alerts: AdminAlert[];
  customErrors: AdminCustomErrorCollection;
  veratoErrors: AdminVeratoErrorCollection;
  lastUpdated: string;
}

export interface AdminUsageMetricsRow {
  system: string;
  tenant: string;
  rawCertificateCn: string;
  callCount: number;
  failedCount: number;
  successCount: number;
  failureRate: number;
  firstSeen?: string | null;
  lastSeen?: string | null;
  sourceIpCount: number;
}

export interface AdminUsageMetricsResponse {
  lookbackHours: number;
  totalSystems: number;
  totalCalls: number;
  totalSuccessCount: number;
  totalFailedCount: number;
  totalDistinctSourceIps: number;
  averageFailureRate: number;
  lastUpdated: string;
  message?: string | null;
  systems: AdminUsageMetricsRow[];
}

export interface AdminApiTrafficDayCount {
  day: string;
  apiCalls: number;
}

export interface AdminApiTrafficHourCount {
  hour: string;
  apiCalls: number;
}

export interface AdminApiTrafficEndpointCount {
  path: string;
  calls: number;
}

export interface AdminApiTrafficMonthCount {
  month: string;
  apiCalls: number;
}

export interface AdminApiTrafficMetricResponse<T> {
  lookbackHours: number;
  lastUpdated: string;
  message?: string | null;
  items: T[];
}

export interface ProtectedPopulationBreakdown {
  type: string;
  count: number;
}

export interface SourceSystemQualityRow {
  sourceSystemName: string;
  agency: string;
  totalIdentities: number;
  activeIdentities: number;
  deletedIdentities: number;
  deleteRate: number;
  staleIdentities: number;
  staleRate: number;
  completenessScore: number;
  minimumDataSetQualifiedCount: number;
  minimumDataSetScore: number;
  missingDobCount: number;
  missingSsnCount: number;
  missingGenderCount: number;
  missingAddressCount: number;
  missingCommunicationCount: number;
  protectedPopulationCount: number;
  protectedPopulationBreakdown: ProtectedPopulationBreakdown[];
}

export interface SourceSystemDataQualityReport {
  totalSources: number;
  totalActiveIdentities: number;
  totalDeletedIdentities: number;
  sources: SourceSystemQualityRow[];
}

export interface LinkClusterRow {
  mpiLinkId: string;
  sourceIdentityCount: number;
  distinctSourceSystems: number;
  sourceSystems: string[];
}

export interface OperationTrendPoint {
  date: string;
  merge: number;
  unmerge: number;
}

export interface LinkIdIngestTrendRow {
  date: string;
  sourceSystemName: string;
  incomingRecords: number;
  newPersonRecords: number;
  alreadyInMpiRecords: number;
}

export interface SourceFragmentationRow {
  sourceSystemName: string;
  totalActiveIdentities: number;
  identitiesInSharedLinkIds: number;
  fragmentationRate: number;
}

export interface MpiLinkageEffectivenessReport {
  uniqueLinkIds: number;
  averageSourceIdentitiesPerLinkId: number;
  multiSourceLinkIds: number;
  multiSourceLinkDetails: LinkClusterRow[];
  recentActivity: OperationTrendPoint[];
  highestFragmentationSources: SourceFragmentationRow[];
  linkIdIngestTrend: LinkIdIngestTrendRow[];
}

export interface BatchFileRow {
  requestId: string;
  fileName: string;
  sourceSystemName: string;
  tenant: string;
  recordsCount: number;
  status: string;
  processingMinutes: number;
  rejectCount: number;
  topErrorMessage: string;
  requestDateTime: string;
}

export interface BatchSourceSummaryRow {
  sourceSystemName: string;
  tenant: string;
  totalFiles: number;
  failedFiles: number;
  rejectedRecords: number;
  averageProcessingMinutes: number;
}

export interface BatchErrorRow {
  sourceSystemName: string;
  message: string;
  count: number;
}

export interface BatchIntakeReliabilityReport {
  totalFiles: number;
  failedFiles: number;
  rejectedRecords: number;
  averageProcessingMinutes: number;
  files: BatchFileRow[];
  sourceSummaries: BatchSourceSummaryRow[];
  topErrors: BatchErrorRow[];
}

export interface UserModifySummaryRow {
  userName: string;
  modifyCount: number;
}

export interface TouchedIdentityRow {
  clientIdentityId: number;
  mpiLinkId: string;
  sourceSystemName: string;
  sourceSystemId: string;
  touchCount: number;
}

export interface StewardshipOutcomeRow {
  operationType: string;
  status: string;
  count: number;
}

export interface LinkIdOperationRow {
  requestDateTime: string;
  operationType: string;
  status: string;
  trackingId: string;
  primaryMpiLinkId: string;
  secondaryMpiLinkId: string;
  affectedLinkIds: string;
  message: string;
}

export interface ManualStewardshipAnalyticsReport {
  totalQueuedRecords: number;
  distinctUsers: number;
  distinctTouchedIdentities: number;
  topUsers: UserModifySummaryRow[];
  mostTouchedIdentities: TouchedIdentityRow[];
  downstreamOutcomes: StewardshipOutcomeRow[];
  linkIdOperations: LinkIdOperationRow[];
}

export interface SystemReferenceRow {
  id: number;
  sourceSystemName: string;
  agencyName: string;
  isActive: boolean;
}

export interface DataSharingMatrixRow {
  sourceSystemId: number;
  sourceSystemName: string;
  allowedSystemId: number;
  allowedSystemName: string;
  status: string;
  dataSharingLevel: string;
}

export interface DataSharingCoverageReport {
  totalSystems: number;
  activeSystems: number;
  activeMappings: number;
  inactiveMappings: number;
  gapCount: number;
  systems: SystemReferenceRow[];
  matrix: DataSharingMatrixRow[];
}

export interface AdminReportsDashboardResponse {
  lookbackDays: number;
  staleThresholdDays: number;
  lastUpdated: string;
  dataQuality: SourceSystemDataQualityReport;
  linkage: MpiLinkageEffectivenessReport;
  batchIntake: BatchIntakeReliabilityReport;
  stewardship: ManualStewardshipAnalyticsReport;
  dataSharing: DataSharingCoverageReport;
}

export interface AdminOnboardedSystemIpWhitelist {
  id: number;
  startIpAddress?: string;
  endIpAddress?: string;
  ipAddressCidr?: string;
}

export interface AdminOnboardedSystem {
  id: number;
  sourceSystemName: string;
  agencyName: string;
  tenant: string;
  connectivityMode?: string;
  onboardingDate?: string | null;
  isActive: boolean;
  enableNotification: boolean;
  startIpAddress?: string;
  endIpAddress?: string;
  ipAddressCidr?: string;
  createdBy?: string | null;
  createdDate?: string | null;
  modifiedBy?: string;
  modifiedDate?: string;
  ipWhitelists: AdminOnboardedSystemIpWhitelist[];
}

export interface AdminOnboardedSystemListResponse {
  systems: AdminOnboardedSystem[];
}

export interface AdminUpsertOnboardedSystemIpWhitelistRequest {
  id?: number | null;
  startIpAddress?: string | null;
  endIpAddress?: string | null;
  ipAddressCidr?: string | null;
}

export interface AdminUpsertOnboardedSystemRequest {
  sourceSystemName: string;
  agencyName: string;
  tenant: string;
  connectivityMode?: string | null;
  onboardingDate: string;
  isActive: boolean;
  enableNotification: boolean;
  ipWhitelists: AdminUpsertOnboardedSystemIpWhitelistRequest[];
}

@Injectable({
  providedIn: "root",
})
export class CommonApiService implements OnInit {
 

  public loginURL;
  public IdentitiesURL;
  public createDataSourceURL;
  public linkURL;
  public unlinkURL;
  public mergeURL;
  public unmergeURL;
  public deleteURL;
  public userActionsReportURL;
  public matchingLinkIdsReportURL;
  public adminDashboardURL;
  public adminUsageMetricsURL;
  public adminApiTrafficURL;
  public adminBatchIntakeReliabilityURL;
  public adminReportsDashboardURL;
  public adminOnboardedSystemsURL;

  constructor(public http: HttpClient, public appSettings: AppSettingsService) {
    //********************** LOGIN STARTS *************************
    this.loginURL = environment.auth.userInfoUrl;
    //********************** LOGIN STARTS *************************

    //********************** IDENTITY STARTS *************************
    this.IdentitiesURL =
      appSettings.consultantApiBaseUrl +
      appSettings.ext.CoreData.Identities.Identities;
    this.createDataSourceURL =
      appSettings.consultantApiBaseUrl +
      appSettings.ext.CoreData.Identities.createDataSource;
    this.linkURL =
      appSettings.consultantApiBaseUrl +
      appSettings.ext.CoreData.Identities.link;
    this.unlinkURL =
      appSettings.consultantApiBaseUrl +
      appSettings.ext.CoreData.Identities.unlink;
    this.mergeURL =
      appSettings.consultantApiBaseUrl +
      appSettings.ext.CoreData.Identities.merge;
    this.unmergeURL =
      appSettings.consultantApiBaseUrl +
      appSettings.ext.CoreData.Identities.unmerge;
    this.deleteURL =
      appSettings.consultantApiBaseUrl +
      appSettings.ext.CoreData.Identities.delete;
    this.userActionsReportURL =
      appSettings.consultantApiBaseUrl + appSettings.ext.CoreData.Reports.UserActions
    this.matchingLinkIdsReportURL =
      appSettings.consultantApiBaseUrl + appSettings.ext.CoreData.Reports.MatchingLinkIds
    this.adminDashboardURL =
      (environment.adminConsoleApiBaseUrl || appSettings.consultantApiBaseUrl) + appSettings.ext.CoreData.Admin.Dashboard
    this.adminUsageMetricsURL =
      (environment.adminConsoleApiBaseUrl || appSettings.consultantApiBaseUrl) + appSettings.ext.CoreData.Admin.UsageMetrics
    this.adminApiTrafficURL =
      (environment.adminConsoleApiBaseUrl || appSettings.consultantApiBaseUrl) + appSettings.ext.CoreData.Admin.ApiTraffic
    this.adminBatchIntakeReliabilityURL =
      (environment.adminConsoleApiBaseUrl || appSettings.consultantApiBaseUrl) + appSettings.ext.CoreData.Admin.BatchIntakeReliability
    this.adminReportsDashboardURL =
      (environment.adminConsoleApiBaseUrl || appSettings.consultantApiBaseUrl) + appSettings.ext.CoreData.Admin.ReportsDashboard
    this.adminOnboardedSystemsURL =
      (environment.adminConsoleApiBaseUrl || appSettings.consultantApiBaseUrl) + appSettings.ext.CoreData.Admin.OnboardedSystems
    // ********************** IDENTITY ENDS ***************************
  }

  ngOnInit() {}
  static HttpOptions() {
    return {
      headers: new HttpHeaders({
        "Content-Type": "application/json",
        accept: "application/json",
      }),
    };
  }

  // *************** POST API *********************//
  login(accessToken: string, idToken: string) : Observable<UserInfo> {
    const headerDict = {
      'Content-Type': 'application/json',
      'Accept': 'application/json',
      'Authorization': `bearer ${accessToken}`,
    }
    
    const requestOptions = {                                                                                                                                                                                 
      headers: new HttpHeaders(headerDict), 
    };

    const cUrl = this.loginURL;
    return this.http.get<UserInfo>(cUrl, requestOptions);
  }


  getRequestHeader(){
    const TOKEN: string = localStorage.getItem("token") || "";
    const reqHeader = new HttpHeaders().set(
      "Authorization",
      "Bearer " + TOKEN
    );
    return reqHeader;
  }

  getIdeitities(pagNumber: number, recordsPerPage: number, data: any, orderBy: string) {
    const cUrl = `${this.IdentitiesURL}?pagNumber=${pagNumber}&recordsPerPage=${recordsPerPage}&orderBy=${orderBy}`;
    return this.http.post(cUrl, data, {
      headers: this.getRequestHeader()
    });
  }

  getUserOperationsReport(pagNumber: number, recordsPerPage: number, orderBy: string, userFilter: string) {
    const cUrl = `${this.userActionsReportURL}?userNameFilter=${userFilter}&pagNumber=${pagNumber}&recordsPerPage=${recordsPerPage}&orderBy=${orderBy}`;
    return this.http.get(cUrl, {
      headers: this.getRequestHeader()
    });
  }

  getMatchingLinkIdsReport(pagNumber: number, recordsPerPage: number, orderBy: string, linkId: string) {
    const cUrl = `${this.matchingLinkIdsReportURL}?linkId=${linkId}&pagNumber=${pagNumber}&recordsPerPage=${recordsPerPage}&orderBy=${orderBy}`;
    return this.http.get(cUrl, {
      headers: this.getRequestHeader()
    });
  }

  getAdminDashboard(lookbackMinutes?: number) {
    let params = new HttpParams();
    if (lookbackMinutes && lookbackMinutes > 0) {
      params = params.set("lookbackMinutes", String(lookbackMinutes));
    }

    return this.http.get<AdminDashboardResponse>(this.adminDashboardURL, {
      params,
      headers: this.getRequestHeader()
    });
  }

  getAdminUsageMetrics(lookbackHours?: number) {
    let params = new HttpParams();
    if (lookbackHours && lookbackHours > 0) {
      params = params.set("lookbackHours", String(lookbackHours));
    }

    return this.http.get<AdminUsageMetricsResponse>(this.adminUsageMetricsURL, {
      params,
      headers: this.getRequestHeader()
    });
  }

  getAdminApiTrafficCallsPerDay(lookbackHours?: number, refresh = false) {
    return this.getAdminApiTrafficMetric<AdminApiTrafficDayCount>(
      `${this.adminApiTrafficURL}/calls-per-day`,
      lookbackHours,
      refresh
    );
  }

  getAdminApiTrafficTopEndpoints(lookbackHours?: number, refresh = false) {
    return this.getAdminApiTrafficMetric<AdminApiTrafficEndpointCount>(
      `${this.adminApiTrafficURL}/top-endpoints`,
      lookbackHours,
      refresh
    );
  }

  getAdminApiTrafficBusiestHours(refresh = false) {
    return this.getAdminApiTrafficMetric<AdminApiTrafficHourCount>(
      `${this.adminApiTrafficURL}/busiest-hours`,
      undefined,
      refresh
    );
  }

  getAdminApiTrafficBusiestDays(refresh = false) {
    return this.getAdminApiTrafficMetric<AdminApiTrafficDayCount>(
      `${this.adminApiTrafficURL}/busiest-days`,
      undefined,
      refresh
    );
  }

  getAdminApiTrafficBusiestMonths(refresh = false) {
    let params = new HttpParams();
    if (refresh) {
      params = params.set("refresh", "true");
    }

    return this.http.get<AdminApiTrafficMetricResponse<AdminApiTrafficMonthCount>>(
      `${this.adminApiTrafficURL}/busiest-months`,
      { params, headers: this.getRequestHeader() }
    );
  }

  private getAdminApiTrafficMetric<T>(url: string, lookbackHours?: number, refresh = false) {
    let params = new HttpParams();
    if (lookbackHours && lookbackHours > 0) {
      params = params.set("lookbackHours", String(lookbackHours));
    }
    if (refresh) {
      params = params.set("refresh", "true");
    }

    return this.http.get<AdminApiTrafficMetricResponse<T>>(url, {
      params,
      headers: this.getRequestHeader()
    });
  }

  getAdminBatchIntakeReliability(lookbackDays?: number) {
    let params = new HttpParams();
    if (lookbackDays && lookbackDays > 0) {
      params = params.set("lookbackDays", String(lookbackDays));
    }

    return this.http.get<BatchIntakeReliabilityReport>(this.adminBatchIntakeReliabilityURL, {
      params,
      headers: this.getRequestHeader()
    });
  }

  getAdminReportsDashboard(lookbackDays?: number) {
    let params = new HttpParams();
    if (lookbackDays && lookbackDays > 0) {
      params = params.set("lookbackDays", String(lookbackDays));
    }

    return this.http.get<AdminReportsDashboardResponse>(this.adminReportsDashboardURL, {
      params,
      headers: this.getRequestHeader()
    });
  }

  getAdminOnboardedSystems() {
    return this.http.get<AdminOnboardedSystemListResponse>(this.adminOnboardedSystemsURL, {
      headers: this.getRequestHeader()
    });
  }

  getAdminOnboardedSystem(id: number) {
    return this.http.get<AdminOnboardedSystem>(`${this.adminOnboardedSystemsURL}/${id}`, {
      headers: this.getRequestHeader()
    });
  }

  createAdminOnboardedSystem(data: AdminUpsertOnboardedSystemRequest) {
    return this.http.post<AdminOnboardedSystem>(this.adminOnboardedSystemsURL, data, {
      headers: this.getRequestHeader()
    });
  }

  createVeratoDataSource(sourceSystemName: string) {
    const payload = {
      content: {
        sources: [sourceSystemName],
      },
    };

    return this.http.post(this.createDataSourceURL, payload, {
      headers: this.getRequestHeader()
    });
  }

  updateAdminOnboardedSystem(id: number, data: AdminUpsertOnboardedSystemRequest) {
    return this.http.put<AdminOnboardedSystem>(`${this.adminOnboardedSystemsURL}/${id}`, data, {
      headers: this.getRequestHeader()
    });
  }

  deleteAdminOnboardedSystem(id: number) {
    return this.http.delete(`${this.adminOnboardedSystemsURL}/${id}`, {
      headers: this.getRequestHeader()
    });
  }

  updateAdminOnboardedSystemNotification(id: number, enableNotification: boolean) {
    return this.http.put<AdminOnboardedSystem>(`${this.adminOnboardedSystemsURL}/${id}/notification-settings`, { enableNotification }, {
      headers: this.getRequestHeader()
    });
  }

  addAdminOnboardedSystemWhitelist(id: number, data: AdminUpsertOnboardedSystemIpWhitelistRequest) {
    return this.http.post<AdminOnboardedSystem>(`${this.adminOnboardedSystemsURL}/${id}/ip-whitelists`, data, {
      headers: this.getRequestHeader()
    });
  }

  updateAdminOnboardedSystemWhitelist(id: number, whitelistId: number, data: AdminUpsertOnboardedSystemIpWhitelistRequest) {
    return this.http.put<AdminOnboardedSystem>(`${this.adminOnboardedSystemsURL}/${id}/ip-whitelists/${whitelistId}`, data, {
      headers: this.getRequestHeader()
    });
  }

  deleteAdminOnboardedSystemWhitelist(id: number, whitelistId: number) {
    return this.http.delete<AdminOnboardedSystem>(`${this.adminOnboardedSystemsURL}/${id}/ip-whitelists/${whitelistId}`, {
      headers: this.getRequestHeader()
    });
  }

  // *************** GET API *********************//

  // *************** UPDATE API *********************//
  link(data: any) {
    const cUrl = `${this.linkURL}`;
    return this.http.put(cUrl, data, {
      headers: this.getRequestHeader(),
    });
  }
  unlink(data: any) {
    const cUrl = `${this.unlinkURL}`;
    return this.http.put(cUrl, data, {
      headers: this.getRequestHeader(),
    });
  }
  merge(data: any) {
    const cUrl = `${this.mergeURL}`;
    return this.http.put(cUrl, data, {
      headers: this.getRequestHeader(),
    });
  }
  unmerge(data: any) {
    const cUrl = `${this.unmergeURL}`;
    return this.http.put(cUrl, data, {
      headers: this.getRequestHeader(),
    });
  }

  // *************** DELETE API *********************//
  delete(data: any) {
    const cUrl = `${this.deleteURL}`;
    return this.http.delete(cUrl, {
      headers: this.getRequestHeader(),
      body: data,
    });
  }
}
