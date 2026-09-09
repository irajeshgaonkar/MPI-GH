import { Component, OnInit } from "@angular/core";
import { environment } from "src/environments/environment";
import { SharedService } from "src/app/services/sharedService";

@Component({
  selector: "app-left-nav",
  templateUrl: "./left-nav.component.html",
  styleUrls: ["./left-nav.component.scss"],
})
export class LeftNavComponent implements OnInit {
  supportRequestUrl: string = environment.supportRequestUrl;

  constructor(private sharedService: SharedService) {}

  ngOnInit(): void {}

  openSupportRequest(): void {
    if (!this.supportRequestUrl) {
      this.sharedService.showToast("Support URL is not configured");
      return;
    }

    window.open(this.supportRequestUrl, "_blank", "noopener");
  }
}
