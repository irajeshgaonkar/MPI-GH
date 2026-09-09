
import {Component, OnInit, ViewChild} from '@angular/core';
import { ApiCallService } from 'src/app/services/api-call.service';
import { AuthService } from 'src/app/services/auth.service';
import { CommonApiService } from 'src/app/services/common-api.service';
import { SharedService } from 'src/app/services/sharedService';
import { MatPaginator, PageEvent } from "@angular/material/paginator";
import { MatTableDataSource } from '@angular/material/table';
import { MatSort, Sort } from '@angular/material/sort';
import { Subject } from 'rxjs';
import { debounceTime, distinctUntilChanged } from 'rxjs/operators';

const ELEMENT_DATA: any = [];

/**
 * @title Basic use of `<table mat-table>`
 */
@Component({
  selector: 'user-actions-report',
  styleUrls: ['user-actions.component.scss'],
  templateUrl: 'user-actions.component.html',
})
export class UserActionsReportComponent implements OnInit {
  displayedColumns: string[] = ['userName', 'action', 'requestDateTime', 'processStartTime', 'processEndTime', 'status', 'message' ];
  pageData: any;
  totalRecords: number = 0;
  pageEvent!: PageEvent;
  userNameFilter: string = '';
  userNameSearchFilter: string = '';
  filterTextChanged: Subject<string> = new Subject<string>();

  orderBy: string = '';
  dataSource = new MatTableDataSource(ELEMENT_DATA);
  public loading: boolean = false;

  @ViewChild(MatSort, { static: true })
  sort!: MatSort;
  @ViewChild(MatPaginator, { static: true })
  paginator!: MatPaginator;

  constructor(private apiCallService: ApiCallService,
    public commonApiService: CommonApiService,
    public sharedService: SharedService,
    public authService: AuthService) {

  }


  
  ngOnInit(): void {
    this.getReportData();
    this.pageData = this.apiCallService.userActionsReportPageFilter.getValue();
  }

  handleUserNameFilterChange(event: any): void {
    
    if (this.filterTextChanged.observers.length === 0) {
      this.filterTextChanged
        .pipe(debounceTime(500), distinctUntilChanged())
        .subscribe(searchQuery => {
          this.userNameSearchFilter = searchQuery;
          this.getReportData();
        });
    }

    this.filterTextChanged.next(this.userNameFilter);
  }

  showUserFilter() {
    var res = this.authService.getRole() == 'Admin';
    return res;
  }

  onPaginationChange(event: any) {
    this.apiCallService.dashboardPagefilter.next(event);
    this.getReportData();
    this.pageData = this.apiCallService.dashboardPagefilter.getValue();
  }

  isGroup(index: number, item: any): boolean{
    return item.isGroupBy;
  }

  getReportData() {
    this.loading = true;
    const { pageIndex, pageSize }: any =
    this.apiCallService.dashboardPagefilter.getValue();
    let orderBy: string = '';

    this.commonApiService
      .getUserOperationsReport(pageIndex, pageSize, orderBy, this.userNameSearchFilter)
      .subscribe((res) => {
        let { data, pageNumber, recordsCount, recordsPerPage }: any = res;
        this.dataSource = data;
        this.totalRecords = recordsCount;
        this.loading = false;
       });
  }
}



/**  Copyright 2018 Google Inc. All Rights Reserved.
    Use of this source code is governed by an MIT-style license that
    can be found in the LICENSE file at http://angular.io/license */
