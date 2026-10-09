import { Component, OnInit } from '@angular/core';
import { DomSanitizer } from '@angular/platform-browser';
import { PBIReportBaseUrl, UserAccessByAgencyReportId } from "../../../shared/constants";

@Component({
  selector: 'user-access-report',
  styleUrls: ['user-access-by-agency.component.scss'],
  templateUrl: 'user-access-by-agency.component.html',
})

export class UserAccessByAgencyComponent implements OnInit{
  public loading: boolean = false;
  iframeUrl: any;
  constructor(private sanitizer:DomSanitizer){
    this.iframeUrl = this.sanitizer.bypassSecurityTrustResourceUrl(`${PBIReportBaseUrl}${UserAccessByAgencyReportId}`);
  }
  ngOnInit(): void {
    }  
}
