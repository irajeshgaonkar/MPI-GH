import { NgModule } from "@angular/core";
import { BrowserModule } from "@angular/platform-browser";
import { HttpClientModule, HttpClient, HTTP_INTERCEPTORS } from "@angular/common/http";
import { AppRoutingModule } from "./app-routing.module";
import { AppComponent } from "./app.component";
import { BrowserAnimationsModule } from "@angular/platform-browser/animations";
import { HeaderComponent } from "./shared/header/header.component";
import { LeftNavComponent } from "./shared/left-nav/left-nav.component";
import { FilterHeaderComponent } from "./shared/filter-header/filter-header.component";
import { MainGridComponent } from "./admin/main-grid/main-grid.component";
import { LoginComponent } from "./login/login.component";
import { MatSort, MatSortModule } from "@angular/material/sort";
import { MaterialModule } from "./shared/material/material.module";
import { NgxPermissionsModule } from "ngx-permissions";
import { FormsModule, ReactiveFormsModule } from "@angular/forms";
import { ModifiedRecordsComponent } from "./admin/modified-records/modified-records.component";
("@angular/platform-browser/animations");
import { MatButtonModule } from "@angular/material/button";
import { ToastrModule } from 'ngx-toastr';
import { AuthInterceptor } from "./services/auth_interecptor";
import { ReportsLandingComponent } from "./admin/reports/landing/landing.component";
import { MatchingLinkIdsReportComponent } from "./admin/reports/matching-link-ids/matching-link-ids.component";
import { ActualSystemsConnectedComponent } from "./admin/reports/actual-systems-connected/actual-systems-connected.component";
import { LinkIdMatchingMultipleSourcesComponent } from "./admin/reports/link-id-matching-multiple-source/link-id-matching-multiple-source.component";
import { LinkIdOperationsComponent } from "./admin/reports/link-id-operations/link-id-operations.component";
import {UserAccessByAgencyComponent} from "./admin/reports/user-access-by-agency/user-access-by-agency.component";
import { UserActionsReportComponent } from "./admin/reports/user-actions/user-actions.component";
import { ReportsComponent } from "./admin/reports/main/main.component";
import { UnderConstructionComponent } from "./under-construction/under-construction.component";
import { HelpLandingComponent } from "./admin/help/landing/help-landing.component"
import { AdminConsoleComponent } from "./admin/metrics/admin-console.component";
import { MetricChartComponent } from "./shared/metric-chart/metric-chart.component";
import { TrafficBarChartComponent } from "./shared/traffic-bar-chart/traffic-bar-chart.component";
import { OnboardedSystemsComponent } from "./admin/onboarded-systems/onboarded-systems.component";
import { AdministrationNavComponent } from "./admin/administration-nav/administration-nav.component";
import { AdminExportModalComponent } from "./admin/admin-export-modal/admin-export-modal.component";
import { UsageMetricsComponent } from "./admin/usage-metrics/usage-metrics.component";
import { TrafficAnalysisComponent } from "./admin/traffic-analysis/traffic-analysis.component";

@NgModule({
  declarations: [
    AppComponent,
    HeaderComponent,
    LeftNavComponent,
    FilterHeaderComponent,
    MainGridComponent,
    LoginComponent,
    ReportsComponent,
    ModifiedRecordsComponent,
    ReportsLandingComponent,
    MatchingLinkIdsReportComponent,
    ActualSystemsConnectedComponent,
    LinkIdMatchingMultipleSourcesComponent,
    LinkIdOperationsComponent,
    UserAccessByAgencyComponent,
    UserActionsReportComponent,
    UnderConstructionComponent,
    HelpLandingComponent,
    AdminConsoleComponent,
    MetricChartComponent,
    TrafficBarChartComponent,
    OnboardedSystemsComponent,
    AdministrationNavComponent,
    AdminExportModalComponent,
    UsageMetricsComponent,
    TrafficAnalysisComponent
  ],
  imports: [
    BrowserModule.withServerTransition({ appId: "ng-cli-universal" }),
    HttpClientModule,
    FormsModule,
    ReactiveFormsModule,
    NgxPermissionsModule.forRoot(),
    AppRoutingModule,
    BrowserAnimationsModule,
    MaterialModule,
    MatButtonModule,
    MatSortModule,
    ToastrModule.forRoot()
  ],
  providers: [HttpClient, {provide: HTTP_INTERCEPTORS, useClass: AuthInterceptor, multi: true }],
  bootstrap: [AppComponent],
})
export class AppModule {}
