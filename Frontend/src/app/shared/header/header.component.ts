import { Component, OnInit } from "@angular/core";
import { AuthService } from './../../services/auth.service';
import { environment } from "src/environments/environment";

@Component({
  selector: "app-header",
  templateUrl: "./header.component.html",
  styleUrls: ["./header.component.scss"],
})
export class HeaderComponent implements OnInit {
  private adminGroupName = environment.auth.adminGroupName;

  constructor(
    private authService: AuthService
  ) {
    
  }

  ngOnInit(): void {}

  getUserName() {
    return localStorage.getItem("LoggedInUser");
  }

  getProfileImageUrl() {
    return localStorage.getItem("profileImageUrl");
  }

  getUserInitials() {
    const userName = this.getUserName();

    if (!userName) {
      return "U";
    }

    const nameParts = userName
      .split(" ")
      .map((part) => part.trim())
      .filter((part) => !!part);

    if (nameParts.length === 1) {
      return nameParts[0].slice(0, 2).toUpperCase();
    }

    return `${nameParts[0][0]}${nameParts[nameParts.length - 1][0]}`.toUpperCase();
  }

  getUserRole() {
    return this.authService.getRole(this.adminGroupName);
  }
  
  logout(): void{
    this.authService.logout();
  }
  
  
  }
