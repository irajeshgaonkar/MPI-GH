import { Component, OnInit } from '@angular/core';
import { DomSanitizer } from '@angular/platform-browser';
import { MPISupportEmail, SharePointLinkForHelp} from "../../../shared/constants";


@Component({
  selector: "help-landing",
  templateUrl: "./help-landing.component.html",
  styleUrls: ["./help-landing.component.scss"],
})

export class HelpLandingComponent  {

    // URL of the SharePoint site
    private sharepointUrl: string = SharePointLinkForHelp;
  
    // Method to open the SharePoint site in a new tab
    openSharePoint(): void {
      window.open(this.sharepointUrl, '_blank');
    }

    sendEmailToSupport():void {
    // Create the mailto link
    const mailtoLink = `mailto:${MPISupportEmail}`;
    
    // Open the default email client with the mailto link
    window.location.href = mailtoLink;
    }
  }

