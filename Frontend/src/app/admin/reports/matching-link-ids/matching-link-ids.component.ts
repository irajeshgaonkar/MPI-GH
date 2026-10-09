

import {Component, OnInit, ViewChild} from '@angular/core';
import { ApiCallService } from 'src/app/services/api-call.service';
import { AuthService } from 'src/app/services/auth.service';
import { CommonApiService } from 'src/app/services/common-api.service';
import { SharedService } from 'src/app/services/sharedService';
import { MatPaginator, PageEvent } from "@angular/material/paginator";
import { MatTableDataSource } from '@angular/material/table';
import { MatSort, Sort } from '@angular/material/sort';
import { Subject } from 'rxjs';
import { debounceTime, distinctUntilChanged, groupBy } from 'rxjs/operators';
import { group } from '@angular/animations';

export class MpiLinkData {
    linkId: string = '';
    sourceSystemName : string = '';
    sourceSystemId:string = '';
    firstName: string = '';
    lastName: string = '';
    middleName: string = '';
    gender: string = '';
}

export interface GroupBy {
  linkId: string;
  isGroupBy: boolean;
}

const ELEMENT_DATA: (MpiLinkData | GroupBy)[] = [];


@Component({
  selector: 'matching-link-ids-report',
  styleUrls: ['matching-link-ids.component.scss'],
  templateUrl: 'matching-link-ids.component.html',
})
export class MatchingLinkIdsReportComponent implements OnInit {
  displayedColumns: string[] = ['linkId', 'sourceSystemName', 'sourceSystemId', 'firstName', 'middleName', 'lastName', 'gender'];
  pageData: any;
  totalRecords: number = 0;
  pageEvent!: PageEvent;
  linkId: string = '';
  linkIdFilter: string = '';
  filterTextChanged: Subject<string> = new Subject<string>();

  orderBy: string = '';
  dataSource = new MatTableDataSource(ELEMENT_DATA);
  public loading: boolean = false;

  @ViewChild(MatSort, { static: true })
  sort!: MatSort;
  @ViewChild(MatPaginator, { static: true })
  paginator!: MatPaginator;

  ngOnInit(): void {
    this.getReportData();
    this.pageData = this.apiCallService.matchingLinkIdsReportPageFilter.getValue();
  }

  constructor(private apiCallService: ApiCallService,
    public commonApiService: CommonApiService,
    public sharedService: SharedService,
    public authService: AuthService) {
  }

  isGroup(index: number, item: any): boolean{
    return item.isGroupBy;
  }

  getReportData() {
    this.loading = true;
    const { pageIndex, pageSize }: any =
    this.apiCallService.matchingLinkIdsReportPageFilter.getValue();
    let orderBy: string = '';

    this.commonApiService
      .getMatchingLinkIdsReport(pageIndex, pageSize, orderBy, this.linkIdFilter)
      .subscribe((res) => {
        debugger;
        let { data, pageNumber, recordsCount, recordsPerPage }: any = res;
        this.dataSource = new MatTableDataSource(this.prepareTableData(data));
        this.totalRecords = recordsCount;
        this.loading = false;
       });
  }

  handleUserNameFilterChange(event: any): void {
    
    if (this.filterTextChanged.observers.length === 0) {
      this.filterTextChanged
        .pipe(debounceTime(500), distinctUntilChanged())
        .subscribe(searchQuery => {
          this.linkIdFilter = searchQuery;
          this.getReportData();
        });
    }

    this.filterTextChanged.next(this.linkId);
  }
  
  onPaginationChange(event: any) {
    this.apiCallService.matchingLinkIdsReportPageFilter.next(event);
    this.getReportData();
    this.pageData = this.apiCallService.matchingLinkIdsReportPageFilter.getValue();
  }

  prepareTableData(data: any) {
    let groupedData : (MpiLinkData | GroupBy)[] = [];
    
    for (const k in data) {
      groupedData.push({linkId : k + ' (' + data[k].length + ') ', isGroupBy: true});
      data[k].forEach((i : any) => {
        groupedData.push({linkId: '', sourceSystemName: i.sourceName, 
        sourceSystemId: i.sourceSystemId, 
        firstName: i.firstName, 
        middleName: i.middleName, 
        lastName: i.lastName, 
        gender: i.gender,});
      })
    }

  return groupedData;
  }

}



/**  Copyright 2018 Google Inc. All Rights Reserved.
    Use of this source code is governed by an MIT-style license that
    can be found in the LICENSE file at http://angular.io/license */
