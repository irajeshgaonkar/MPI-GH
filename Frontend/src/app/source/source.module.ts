import { NgModule } from "@angular/core";
import { CommonModule } from "@angular/common";
import { Routes, RouterModule } from "@angular/router";
import { HttpClientModule } from "@angular/common/http";
import { FormsModule, ReactiveFormsModule } from "@angular/forms";
import { MaterialModule } from "../shared/material/material.module";
import { MainGridComponent } from "../admin/main-grid/main-grid.component";
import { ModifiedRecordsComponent } from "../admin/modified-records/modified-records.component";
import { ReportsLandingComponent } from "../admin/reports/landing/landing.component";
import { MatchingLinkIdsReportComponent } from "../admin/reports/matching-link-ids/matching-link-ids.component";
import { UserActionsReportComponent } from "../admin/reports/user-actions/user-actions.component";
import { ActualSystemsConnectedComponent } from "../admin/reports/actual-systems-connected/actual-systems-connected.component";
import { LinkIdMatchingMultipleSourcesComponent } from "../admin/reports/link-id-matching-multiple-source/link-id-matching-multiple-source.component";
import { LinkIdOperationsComponent } from "../admin/reports/link-id-operations/link-id-operations.component";
import { UserAccessByAgencyComponent } from "../admin/reports/user-access-by-agency/user-access-by-agency.component";

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
      breadcrumb: "Actual Systems Connected Report",
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
export class SourceModule {}
