import { Injectable } from "@angular/core";
import { BehaviorSubject } from "rxjs";
import { HttpClient } from "@angular/common/http";
@Injectable({
  providedIn: "root",
})
export class ApiCallService {
  dashboardPagefilter: BehaviorSubject<Object> = new BehaviorSubject<Object>({
    pageIndex: 0,
    pageSize: 20,
  });

  userActionsReportPageFilter: BehaviorSubject<Object> = new BehaviorSubject<Object>({
    pageIndex: 0,
    pageSize: 20,
  });

  matchingLinkIdsReportPageFilter: BehaviorSubject<Object> = new BehaviorSubject<Object>({
    pageIndex: 0,
    pageSize: 20,
  });

  constructor(public http: HttpClient) {}
}
