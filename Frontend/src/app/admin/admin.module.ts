import { NgModule } from "@angular/core";
import { CommonModule } from "@angular/common";
import { Routes, RouterModule } from "@angular/router";
import { HttpClientModule } from "@angular/common/http";
import { FormsModule, ReactiveFormsModule } from "@angular/forms";
import { MaterialModule } from "../shared/material/material.module";
import { MainGridComponent } from "./main-grid/main-grid.component";
import { ModifiedRecordsComponent } from "./modified-records/modified-records.component";
import { ReportsLandingComponent } from "./reports/landing/landing.component";
import { MatchingLinkIdsReportComponent } from "./reports/matching-link-ids/matching-link-ids.component";
import { ActualSystemsConnectedComponent } from "./reports/actual-systems-connected/actual-systems-connected.component";
import { LinkIdMatchingMultipleSourcesComponent } from "./reports/link-id-matching-multiple-source/link-id-matching-multiple-source.component";
import { LinkIdOperationsComponent } from "./reports/link-id-operations/link-id-operations.component";
import { UserAccessByAgencyComponent } from "./reports/user-access-by-agency/user-access-by-agency.component";
import { UserActionsReportComponent } from "./reports/user-actions/user-actions.component";
import { ReportsComponent } from "./reports/main/main.component";
import { UnderConstructionComponent } from "../under-construction/under-construction.component";
import { HelpLandingComponent } from "./help/landing/help-landing.component";
import { AdminConsoleComponent } from "./metrics/admin-console.component";
import { OnboardedSystemsComponent } from "./onboarded-systems/onboarded-systems.component";
import { UsageMetricsComponent } from "./usage-metrics/usage-metrics.component";
import { TrafficAnalysisComponent } from "./traffic-analysis/traffic-analysis.component";

const routes: Routes = [
  {
    path: "",
    redirectTo: "user-grid",
    pathMatch: "full",
  },
  {
    path: "user-grid",
    component: MainGridComponent,
    data: {
      breadcrumb: "MPI Dashboard",
    },
  },
  {
    path: "modify-records",
    component: ModifiedRecordsComponent,
    data: {
      breadcrumb: "Modify Records",
    },
  },
  {
    path: "reports",
    component: ReportsLandingComponent,
    data: {
      breadcrumb: "Reports",
    },
  },
  {
    path: "matching-link-ids-report",
    component: MatchingLinkIdsReportComponent,
    data: {
      breadcrumb: "Matching Link Ids Report",
    },
  },
  {
    path: "actual-systems-connected-report",
    component: ActualSystemsConnectedComponent,
    data: {
      breadcrumb: "Actual Systems Connected",
    },
  },
  {
    path: "link-id-matching-multiple-sources-report",
    component: LinkIdMatchingMultipleSourcesComponent,
    data: {
      breadcrumb: "LinkId Matching Multiple Sources",
    },
  },
  {
    path: "link-id-operations-report",
    component: LinkIdOperationsComponent,
    data: {
      breadcrumb: "LinkId Operations",
    },
  },
  {
    path: "user-actions-report",
    component: UserActionsReportComponent,
    data: {
      breadcrumb: "User Actions Report",
    },
  },
  {
    path: "user-access-report",
    component: UserAccessByAgencyComponent,
    data: {
      breadcrumb: "User Access Report",
    },
  },
  {
    path: "settings",
    component: UnderConstructionComponent,
    data: {
      breadcrumb: "User Intities",
    },
  },
  {
    path: "help",
    component: HelpLandingComponent,
    data: {
      breadcrumb: "Help",
    },
  },
  {
    path: "metrics",
    component: AdminConsoleComponent,
    data: {
      breadcrumb: "MPI Health",
    },
  },
  {
    path: "onboarded-systems",
    component: OnboardedSystemsComponent,
    data: {
      breadcrumb: "Onboarded Systems",
    },
  },
  {
    path: "usage-metrics",
    redirectTo: "combined-usage",
    pathMatch: "full",
  },
  {
    path: "combined-usage",
    component: UsageMetricsComponent,
    data: {
      breadcrumb: "Combined Usage",
      usageMode: "combined",
    },
  },
  {
    path: "api-usage",
    component: UsageMetricsComponent,
    data: {
      breadcrumb: "API Usage",
      usageMode: "api",
    },
  },
  {
    path: "batch-usage",
    component: UsageMetricsComponent,
    data: {
      breadcrumb: "Batch Usage",
      usageMode: "batch",
    },
  },
  {
    path: "traffic-analysis",
    component: TrafficAnalysisComponent,
    data: {
      breadcrumb: "Traffic Analysis",
    },
  },
];

@NgModule({
  declarations: [],
  imports: [
    CommonModule,
    HttpClientModule,
    FormsModule,
    ReactiveFormsModule,
    RouterModule.forChild(routes),
    MaterialModule,
  ],
  exports: [RouterModule],
  entryComponents: [],
})

export class AdminModule {}
