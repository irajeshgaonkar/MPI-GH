import { Component, OnInit } from "@angular/core";
import { FormBuilder, FormGroup, Validators } from "@angular/forms";
import { loginCreds, ErrorTypes, getRouteByRoleId } from "../shared/constants";
import { Router } from "@angular/router";
import { CommonApiService } from "../services/common-api.service";
import { AuthService } from "../services/auth.service";
import { SharedService } from "../services/sharedService";
import { first } from "rxjs";
import { environment } from "src/environments/environment";

@Component({
  selector: "app-login",
  templateUrl: "./login.component.html",
  styleUrls: ["./login.component.scss"],
})
export class LoginComponent implements OnInit {
  constructor(
    private form: FormBuilder,
    private commonApiService: CommonApiService,
    private authService: AuthService,
    private router: Router,
    private sharedService: SharedService
  ) {}
  public loginForm!: FormGroup;
  private errorType: string = "";
  public loading: boolean = false;
  public welpopup: boolean = true;
  public ssoUrl: string = "";
  public ssoBaseUrl: string = environment.auth.ssoBaseUrl;
  public identityUrl: string = environment.auth.identityUrl;
  public redirectUri: string = environment.auth.redirectUri;
  public clientId: string = environment.auth.clientId;
  public scope: string = "email openid profile";
  public adminGroupName: string = environment.auth.adminGroupName;

  ngOnInit(): void {
    // this.initLoginForm();
    this.formSsoUrl();
    this.login();
  }

  close(){
    this.welpopup = false;
  }

  initLoginForm(): void {
    this.loginForm = this.form.group({
      email: ["", Validators.required],
      password: ["", Validators.required],
    });
  }

  formSsoUrl(): void {
    this.ssoUrl = `${this.ssoBaseUrl}?identity_provider=${this.identityUrl}&redirect_uri=${this.redirectUri}&response_type=TOKEN&client_id=${this.clientId}&scope=${this.scope}`
  }

  login(): void {
    const currentUrl = this.router.url;
    let accessToken = '';
    let idToken = '';

    if(currentUrl.includes('access_token=')) {
      accessToken = currentUrl.split('access_token=')[1].split('&')[0]
    }

    if(currentUrl.includes('id_token=')) {
      idToken = currentUrl.split('id_token=')[1].split('&')[0]
    }

    if (idToken == '' || accessToken == '') return;

      this.commonApiService.login(accessToken, idToken).subscribe(
        (res) => {
          const customGroups = res["custom:groups"];
          const role = this.authService.resolveRoleFromGroups(
            customGroups,
            this.adminGroupName
          );

          this.authService.sendToken(
            res.given_name,
            res.family_name,
            idToken,
            role,
            customGroups,
            res.picture
          );
          this.router.navigate([`/${role.toLowerCase()}/user-grid`]);
          this.sharedService.showToast("Logged in successfully");
          this.loading = false;
        },
        (err) => {
          this.loading = false;
          const { error }: any = err;
          if (error) {
            this.sharedService.showToast(error);
          }
        }
      );
  }

  getError(field: any): any {
    const Errors: any = {
      INVALID: {
        username: "Please enter valid username",
        password: "Please enter valid password",
      },
      REQUIRED: {
        username: "Username is required",
        password: "Password is required",
      },
    };
    return Errors[this.errorType][field];
  }

  isInvalid(field: string): boolean {
    if (!field || !this.loginForm.controls[field]) return false;
    return !this.loginForm.controls[field].valid && !!this.errorType;
  }
}
