import { Component, OnInit } from '@angular/core';
import { DomSanitizer } from '@angular/platform-browser';
import { PBIReportBaseUrl, LinkIdOperationsReportId } from "../../../shared/constants";

@Component({
  selector: 'link-id-operations-report',
  styleUrls: ['link-id-operations.component.scss'],
  templateUrl: 'link-id-operations.component.html',
})

export class LinkIdOperationsComponent implements OnInit{
  public loading: boolean = false;
  iframeUrl: any;
  constructor(private sanitizer:DomSanitizer){
    this.iframeUrl = this.sanitizer.bypassSecurityTrustResourceUrl(`${PBIReportBaseUrl}${LinkIdOperationsReportId}`);
  }
  ngOnInit(): void {
    }  
}
