import { Component, OnInit, Output, EventEmitter } from "@angular/core";
import { AuthService } from "../../services/auth.service";

interface IDropdownOption {
  key: string,
  value: string
}

export interface Address {
  line1: string;
  line2: string;
  city: string;
  state: string;
  postalCode: string;
}

export interface Name {
  first: string;
  middle: string;
  last: string;
  suffix: string;
}

export interface PhoneNumber {
  number: string;
}

export interface Identity {
  emails: string[];
  addresses: Address[];
  names: Name[];
  ssns: string[];
  genders: string[];
  datesOfBirth: string[];
  phoneNumbers: PhoneNumber[];
}

@Component({
  selector: "app-filter-header",
  templateUrl: "./filter-header.component.html",
  styleUrls: ["./filter-header.component.scss"],
})
export class FilterHeaderComponent implements OnInit {

  constructor(public authService : AuthService) {

  }

  public filter: string = "first";
  public firstNameValue: string = "";
  public secondFilter: string = "last";
  public lastNameValue: string = "";
  public emailValue: string = "";
  public phoneValue: string = "";
  public line1Value: string = "";
  public line2Value: string = "";
  public cityValue: string = "";
  public stateValue: string = "";
  public zipValue: string = "";
  public linkIdValue: string = "";
  public sourceIdValue: string = "";
  public sourceNameValue: string = "";
  public ssnValue: string = "";
  public dobValue: string = "";
  public firstFilterOptions: IDropdownOption[] = this.initiDropDownValues();
  public secondFilterOptions: IDropdownOption[] = this.getSecondDropDownFilter();
  //public firstNameFilterOptions: IDropdownOption[] = this.initNameFilterDropDownValues();
  //public secondNameFilterOptions: IDropdownOption[] = this.getSecondNameFilter();


  @Output() handleInputChange = new EventEmitter<any>();

  ngOnInit(): void {
  }

  quickFilterGroups(): Array<{ label: string; count: number }> {
    return [
      { label: "Identity", count: this.countValues([this.firstNameValue, this.lastNameValue, this.linkIdValue, this.sourceIdValue, this.sourceNameValue, this.ssnValue, this.dobValue]) },
      { label: "Contact", count: this.countValues([this.emailValue, this.phoneValue]) },
      { label: "Address", count: this.countValues([this.line1Value, this.line2Value, this.cityValue, this.stateValue, this.zipValue]) },
    ];
  }

  activeFilterCount(): number {
    return this.countValues([
      this.firstNameValue,
      this.lastNameValue,
      this.emailValue,
      this.phoneValue,
      this.line1Value,
      this.line2Value,
      this.cityValue,
      this.stateValue,
      this.zipValue,
      this.linkIdValue,
      this.sourceIdValue,
      this.sourceNameValue,
      this.ssnValue,
      this.dobValue,
    ]);
  }

  initiDropDownValues() : IDropdownOption[] {
    if (this.authService.getRole() == 'Admin') {
      return [
        {key: 'first', value: 'FirstName'}, 
        {key: 'last', value: 'Lastname'}, 
        {key: 'email', value: 'Email'}, 
        {key: 'ssn', value: 'SSN'}, 
        {key: 'mpilinkid', value: 'Link Id'}, 
        {key: 'sourceid', value: 'Source System Id'}
      ];
    }

    return [
      {key: 'first', value: 'FirstName'}, 
      {key: 'last', value: 'Lastname'}, 
      {key: 'email', value: 'Email'}, 
      {key: 'mpilinkid', value: 'Link Id'}, 
      {key: 'sourceid', value: 'Source System Id'}
    ];
  }

  getSecondDropDownFilter() : IDropdownOption[]{
    let dropDownOptions = this.initiDropDownValues();
    let secondFilterOptions = dropDownOptions.filter(item => item.key != this.filter);
    return secondFilterOptions;
  }

  resetSecondFilter() {
      this.secondFilterOptions = this.getSecondDropDownFilter();
      this.secondFilter = this.secondFilterOptions[0].key;
      //this.secondvalue = '';
  }

  

  handleChange(event: any): void {
    if(this.secondFilter == this.filter) {
      this.resetSecondFilter();
    }

    let names: Name[] = [];
    const updatedName: Name = {
      first: this.firstNameValue,
      middle: '',
      last: this.lastNameValue,
      suffix: ''
    };
    names.push(updatedName);

    let addresses: Address[] = [];
    const updatedAddress: Address = {
      line1: this.line1Value,
      line2: this.line2Value,
      city: this.cityValue,
      state: this.stateValue,
      postalCode: this.zipValue
    }
    addresses.push(updatedAddress);

    let phoneNumbers: PhoneNumber[] = [];
    const updatedPhone: PhoneNumber ={
      number: this.phoneValue
    }
    phoneNumbers.push(updatedPhone);

    let ssns: string[] = [];
    let genders: string[] =[];
    let dobs: string[] =[];
    let emails: string[] = [];

    const updatedIdentity: Identity = {
      names: names,
      addresses: addresses,
      ssns: ssns,
      emails: emails,
      datesOfBirth: dobs,
      genders: genders,
      phoneNumbers: phoneNumbers
    }

    //this.handleInputChange.emit([{ value: this.firstNameValue, filter: this.filter}, { value: this.lastNameValue, filter: this.secondFilter}]);
    this.handleInputChange.emit([
      { value: this.firstNameValue, filter: "firstName"}, 
      { value: this.lastNameValue, filter: "lastName"},
      { value: this.emailValue, filter: "email"},
      { value: this.phoneValue, filter: "contact"},
      { value: this.dobValue, filter: "dateOfBirth"},
      { value: this.line1Value, filter: "addressLine1"},
      { value: this.line2Value, filter: "addressLine2"},
      { value: this.cityValue, filter: "city"},
      { value: this.stateValue, filter: "state"},
      { value: this.zipValue, filter: "zip"},
      { value:this.ssnValue, filter: "ssn"},
      { value:this.linkIdValue, filter: "mpilinkid"},
      { value:this.sourceIdValue, filter: "sourceid"},
      { value:this.sourceNameValue, filter: "sourcename"}
    ]);
  }

  clearAll(): void {
    this.firstNameValue = "";
    this.lastNameValue = "";
    this.emailValue = "";
    this.phoneValue = "";
    this.line1Value = "";
    this.line2Value = "";
    this.cityValue = "";
    this.stateValue = "";
    this.zipValue = "";
    this.linkIdValue = "";
    this.sourceIdValue = "";
    this.sourceNameValue = "";
    this.ssnValue = "";
    this.dobValue = "";
    this.handleChange(null);
  }

  private countValues(values: Array<string | null | undefined>): number {
    return values.filter((value) => !!String(value || "").trim()).length;
  }

}
