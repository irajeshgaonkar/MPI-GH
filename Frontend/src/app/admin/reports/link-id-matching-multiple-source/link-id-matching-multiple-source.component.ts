import { Component, OnInit } from '@angular/core';
import { DomSanitizer } from '@angular/platform-browser';
import { PBIReportBaseUrl, LinkIdMatchingMultipleSourcesReportId } from "../../../shared/constants";

@Component({
  selector: 'link-id-matching-multiple-sources-report',
  styleUrls: ['link-id-matching-multiple-source.component.scss'],
  templateUrl: 'link-id-matching-multiple-source.component.html',
})

export class LinkIdMatchingMultipleSourcesComponent implements OnInit{
    public loading: boolean = false;
    iframeUrl: any;
    constructor(private sanitizer:DomSanitizer){
      this.iframeUrl = this.sanitizer.bypassSecurityTrustResourceUrl(`${PBIReportBaseUrl}${LinkIdMatchingMultipleSourcesReportId}`);
    }
    ngOnInit(): void {
      } 
}