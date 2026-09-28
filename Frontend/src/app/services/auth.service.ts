import { Injectable } from "@angular/core";
import { BehaviorSubject, Observable } from "rxjs";
import { Router } from "@angular/router";
// import { NgxPermissionsService } from "ngx-permissions";

export interface Roles {
  id: number;
  name: string;
}

@Injectable({
  providedIn: "root",
})
export class AuthService {
  // public loggedIn: BehaviorSubject<boolean> = new BehaviorSubject(false);

  constructor(
    private router: Router // private permissionsService: NgxPermissionsService
  ) {}
  sendToken(
    firstName: string,
    lastName: string,
    token: string,
    role: string,
    customGroups?: string,
    profileImageUrl?: string
  ) {
    localStorage.setItem("LoggedInUser", `${firstName} ${lastName}`);
    localStorage.setItem("token", token);
    localStorage.setItem("hcaRole", role);
    if (customGroups) {
      localStorage.setItem("customGroups", customGroups);
    }
    if (profileImageUrl) {
      localStorage.setItem("profileImageUrl", profileImageUrl);
    } else {
      localStorage.removeItem("profileImageUrl");
    }
  }

  getToken() {
    return localStorage.getItem("LoggedInUser");
  }

  resolveRoleFromGroups(customGroups: string | string[] | null | undefined, adminGroupName: string) {
    if (!customGroups) {
      return "Viewer";
    }

    const groups = Array.isArray(customGroups)
      ? customGroups
      : customGroups.split(",");

    const isAdmin = groups.some((userGroup) => {
      const groupName = userGroup.replace("[", "").replace("]", "").trim();
      return groupName === adminGroupName;
    });

    return isAdmin ? "Admin" : "Viewer";
  }

  getRole(adminGroupName?: string) {
    if (adminGroupName) {
      const customGroups = localStorage.getItem("customGroups");
      if (customGroups) {
        return this.resolveRoleFromGroups(customGroups, adminGroupName);
      }
    }

    return localStorage.getItem("hcaRole");
  }

  isLoggedIn() {
    return this.getToken() !== null;
  }
  
  logout() {
    localStorage.removeItem("LoggedInUser");
    localStorage.removeItem("token");
    localStorage.removeItem("hcaRole");
    localStorage.removeItem("customGroups");
    this.router.navigate(["login"]);
  }
}
