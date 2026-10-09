import { Component, OnInit } from "@angular/core";
import { MODIFY_DATA, MERGE_RECORDS } from "../../shared/mocks";
import { CommonApiService } from "../../services/common-api.service";
import { SharedService } from "../../services/sharedService";
import { Router } from "@angular/router";
import { NotificationService } from '../../services/notification.service';
import { AuthService } from "src/app/services/auth.service";

@Component({
  selector: "app-modified-records",
  templateUrl: "./modified-records.component.html",
  styleUrls: ["./modified-records.component.scss"],
})
export class ModifiedRecordsComponent implements OnInit {
  constructor(
    public commonApiService: CommonApiService,
    public sharedService: SharedService,
    private router: Router,
    private notifyService: NotificationService,
    public authService: AuthService
  ) { }
  public data: any = MODIFY_DATA;
  public selectedItems = new Set();
  public selectedRecords: any = [];
  public loading: boolean = false;
  public compareFields: any[] = [
    { key: "mpiLinkId", label: "MPI Link ID" },
    { key: "sourceSystemId", label: "Source System ID" },
    { key: "sourceName", label: "Source System Name" },
    { key: "sourceSystemLastUpdate", label: "Source System Last Updated" },
    { key: "firstName", label: "Firstname" },
    { key: "middleName", label: "Middlename" },
    { key: "lastName", label: "Lastname" },
    { key: "suffix", label: "Suffix" },
    { key: "birthDate", label: "Date of Birth", sensitive: true },
    { key: "gender", label: "Gender" },
    { key: "ssn", label: "SSN", sensitive: true },
    { key: "emailAddress", label: "Email" },
    { key: "addressType", label: "Address Type" },
    { key: "addressLine1", label: "Address Line 1" },
  ];

  ngOnInit(): void { }

  get hasSingleSelection(): boolean {
    return this.selectedItems.size === 1;
  }

  get hasTwoSelections(): boolean {
    return this.selectedItems.size === 2;
  }

  selectItem(event: any, item: any): void {
    
    const value = Number(event.target.value);
    if (this.selectedItems.has(value)) {
      this.selectedItems.delete(value);
      let index = this.selectedRecords.findIndex((el: any) => el.id == value);
      this.selectedRecords.splice(index, 1);
    } else {
      if(this.selectedItems.size >= 2){
        event.target.checked = false;
        this.sharedService.showToast("Reached maximum limit of select count")
        event.preventDefault();
        event.stopPropagation();
        return;
      }
      if (this.selectedItems.size < 2) {
        this.selectedItems.add(value);
        MERGE_RECORDS.push(item);
        this.selectedRecords.push(item);
      }
    }
  }

  showSensitiveData() {
    var res = this.authService.getRole() == 'Admin';
    return res;
  }

  getSelectedData() {
    const data = this.data.filter((el: any) => this.selectedItems.has(el.id));
    return data;
  }

  deleteSelected() {
    if (this.loading || !this.hasSingleSelection) {
      return;
    }

    this.loading = true;
    const data = this.getSelectedData();
    const selectedRecord = data[0];
    const bodyData = {
      content: {
        name: selectedRecord.sourceName,
        id: selectedRecord.sourceSystemId,
      },
    };

    this.commonApiService.delete(bodyData).subscribe((res) => {
      this.notifyService.showSuccess("Deleted record successfully.", '');
      this.removeSelectedRecords(data);
      this.closePopUp();
    },
      (err) => {
        this.notifyService.showError(err.error, '');
        this.removeSelectedRecords(data);
        this.closePopUp();
      });
  }

  link() {
    if (this.loading || !this.hasTwoSelections) {
      return;
    }

    this.loading = true;
    const data = this.getSelectedData();
    const bodyData = {
      linkToSource: {
        name: data[0].sourceName,
        id: data[0].sourceSystemId,
      },
      source: {
        name: data[1].sourceName,
        id: data[1].sourceSystemId,
      },
    };
    this.commonApiService.link(bodyData).subscribe((res) => {
      this.notifyService.showSuccess("Linked records successfully.", '');
      this.removeSelectedRecords(data);
      this.closePopUp();
    },
      (err) => {
        this.notifyService.showError(err.error, '');
        this.removeSelectedRecords(data);
        this.closePopUp();
      });
  }

  unlink() {
    if (this.loading || !this.hasTwoSelections) {
      return;
    }

    this.loading = true;
    const data = this.getSelectedData();
    const bodyData = {
      unlinkFromSource: {
        name: data[0].sourceName,
        id: data[0].sourceSystemId,
      },
      source: {
        name: data[1].sourceName,
        id: data[1].sourceSystemId,
      },
    };
    this.commonApiService.unlink(bodyData).subscribe((res) => {
      this.notifyService.showSuccess("Unlinked records successfully.", '');
      this.removeSelectedRecords(data);
      this.closePopUp();
    },
      (err) => {
        this.notifyService.showError(err.error, '');
        this.removeSelectedRecords(data);
        this.closePopUp();
      });
  }

  merge() {
    if (this.loading || !this.hasTwoSelections) {
      return;
    }

    this.loading = true;
    const data = this.getSelectedData();
    const bodyData = {
      toSurviveSource: {
        name: data[0].sourceName,
        id: data[0].sourceSystemId,
      },
      toRetireSource: {
        name: data[1].sourceName,
        id: data[1].sourceSystemId,
      },
    };
    this.commonApiService.merge(bodyData).subscribe((res) => {
      this.notifyService.showSuccess("Merged records successfully.", '');
      this.removeSelectedRecords(data);
      this.closePopUp();
    },
      (err) => {
        this.notifyService.showError(err.error, '');
        this.removeSelectedRecords(data);
        this.closePopUp();
      });
  }

  unmerge() {
    if (this.loading || !this.hasTwoSelections) {
      return;
    }

    this.loading = true;
    const data = this.getSelectedData();
    const bodyData = {
      unmergeFromSource: {
        name: data[0].sourceName,
        id: data[0].sourceSystemId,
      },
      unmergeSource: {
        name: data[1].sourceName,
        id: data[1].sourceSystemId,
      },
    };
    this.commonApiService.unmerge(bodyData).subscribe((res) => {
      this.notifyService.showSuccess("Unmerged records successfully.", '');
      this.removeSelectedRecords(data);
      this.closePopUp();
    },
      (err) => {
        this.notifyService.showError(err.error, '');
        this.removeSelectedRecords(data);
        this.closePopUp();
      });
  }

  removeSelectedRecords(data: any) {
    this.data = this.data.filter((el: any) => !this.selectedItems.has(el.id));
    MODIFY_DATA.splice(0);
    MODIFY_DATA.push(...this.data);
    this.selectedItems = new Set();
    this.selectedRecords = [];
  }

  getCompareValue(item: any, key: string) {
    const value = item?.[key];
    return value || "--";
  }

  isCompareFieldDifferent(key: string): boolean {
    if (this.selectedRecords.length < 2) {
      return false;
    }

    const first = this.selectedRecords[0]?.[key] ?? "";
    const second = this.selectedRecords[1]?.[key] ?? "";
    return first !== second;
  }

  compareDifferenceCount(): number {
    return this.compareFields.filter((field: any) => this.isCompareFieldDifferent(field.key)).length;
  }

  closePopUp() {
    this.loading = false;
    this.router.navigate(['/admin/modify-records']);
  }
}
