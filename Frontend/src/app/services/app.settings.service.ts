import { Injectable } from "@angular/core";
import { environment } from "src/environments/environment";

@Injectable({
  providedIn: "root",
})
export class AppSettingsService {
  public consultantApiBaseUrl = environment.consultantApiBaseUrl;

  //EXT
  public ext = {
    CoreData: {
      auth: {
        Login: "login",
        LogOff: "",
      },
      Identities: {
        Identities: "api/Identities/dashboardData",
        createDataSource: "api/Identities/createDataSource",
        link: "api/Identities/link",
        unlink: "api/Identities/unlink",
        merge: "api/Identities/merge",
        unmerge: "api/Identities/unmerge",
        delete: "api/Identities/delete",
      },
      Reports: {
        UserActions: "api/Reports/userActionsReports",
        MatchingLinkIds: "api/Reports/matchingLinkIds"
      },
      Admin: {
        Dashboard: "api/admin/dashboard",
        UsageMetrics: "api/admin/usage-metrics",
        ApiTraffic: "api/admin/api-traffic",
        BatchIntakeReliability: "api/admin/reports/batch-intake-reliability",
        ReportsDashboard: "api/admin/reports/dashboard",
        OnboardedSystems: "api/admin/onboarded-systems"
      }
    },
  };

  constructor() {}
}
