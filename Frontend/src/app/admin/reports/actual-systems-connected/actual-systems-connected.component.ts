import { Component, OnInit } from '@angular/core';
import { DomSanitizer } from '@angular/platform-browser';
import { PBIReportBaseUrl, ActualSystemsConnectedReportId } from "../../../shared/constants";

@Component({
  selector: 'actual-systems-connected-report',
  styleUrls: ['actual-systems-connected.component.scss'],
  templateUrl: 'actual-systems-connected.component.html',
})

export class ActualSystemsConnectedComponent implements OnInit{
  public loading: boolean = false;
  iframeUrl: any;
  constructor(private sanitizer:DomSanitizer){
    this.iframeUrl = this.sanitizer.bypassSecurityTrustResourceUrl(`${PBIReportBaseUrl}${ActualSystemsConnectedReportId}`);
  }
  ngOnInit(): void {
    } 
}
