import { Injectable } from "@angular/core";
import {
  CanActivate,
  ActivatedRouteSnapshot,
  RouterStateSnapshot,
  UrlTree,
  Router,
} from "@angular/router";
import { Observable } from "rxjs";
import { AuthService } from "./auth.service";
import { NgxPermissionsService } from "ngx-permissions";

@Injectable({
  providedIn: "root",
})
export class AuthGuard implements CanActivate {
  constructor(
    private auth: AuthService,
    private myRoute: Router,
    private permissionsService: NgxPermissionsService
  ) {}
  canActivate(
    next: ActivatedRouteSnapshot,
    state: RouterStateSnapshot
  ):
    | Observable<boolean | UrlTree>
    | Promise<boolean | UrlTree>
    | boolean
    | UrlTree {
    if (this.auth.isLoggedIn()) {
      let permissions: any = [];
      permissions.push(this.auth.getRole());
      this.permissionsService.loadPermissions(permissions);
      return true;
    } else {
      this.myRoute.navigate(["login"]);
      return false;
    }
  }
}
