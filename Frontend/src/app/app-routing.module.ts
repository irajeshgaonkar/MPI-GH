import { NgModule } from "@angular/core";
import { RouterModule, Routes } from "@angular/router";
import { LoginComponent } from "./login/login.component";
import { MainGridComponent } from "./admin/main-grid/main-grid.component";
import { AuthGuard } from "./services/auth.guard";
import { NgxPermissionsGuard } from "ngx-permissions";
import { AppComponent } from "./app.component";

const routes: Routes = [
  {
    path: "",
    redirectTo: "login",
    pathMatch: "full",
  },
  {
    path: "login",
    component: LoginComponent,
    data: {
      breadcrumb: "Login Page",
    },
  },
  { path: "login", component: LoginComponent },
  {
    path: "",
    component: AppComponent,
    canActivate: [AuthGuard],
    data: {
      breadcrumb: "Dashboard",
    },
    children: [
      {
        path: "admin",
        canActivateChild: [NgxPermissionsGuard],
        data: {
          permissions: {
            only: ["Admin"],
            redirectTo: "/login",
          },
        },
        loadChildren: () =>
          import("./admin/admin.module").then((m) => m.AdminModule),
      },
      {
        path: "viewer",
        canActivateChild: [NgxPermissionsGuard],
        data: {
          permissions: {
            only: ["Viewer"],
            redirectTo: "/login",
          },
        },
        loadChildren: () =>
          import("./viewer/viewer.module").then((m) => m.ViewerModule),
      },
      {
        path: "source",
        canActivateChild: [NgxPermissionsGuard],
        data: {
          permissions: {
            only: ["SourceUser"],
            redirectTo: "/login",
          },
        },
        loadChildren: () =>
          import("./source/source.module").then((m) => m.SourceModule),
      },
    ],
  },
  { path: "**", redirectTo: "" },
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule],
})
export class AppRoutingModule {}
