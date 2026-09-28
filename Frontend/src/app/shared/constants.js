export const loginCreds = {
  USERNAME: new Set(["admin", "hcaAdmin"]),
  PASSWORD: "Hca@123",
};
export const Errors = {
  INVALID: {
    username: "Please enter valid username",
    password: "Please enter valid password",
  },
  REQUIRED: {
    username: "Username is required",
    password: "Password is required",
  },
};
export const ErrorTypes = {
  INVALID: "INVALID",
  REQUIRED: "REQUIRED",
};

export const BODY_DATA = {
  firstName: '',
  lastName: '',
  email: '',
  contact:'',
  dateOfBirth:'',
  addressLine1:'',
  addressLine2:'',
  city:'',
  state:'',
  zip:'',
  ssn: '',
  linkId: '',
  sourceSystemId: '',
  sourceSystemName: ''
};

// ROUTE USER BASED ON ROLE
export const getRouteByRoleId = (roleId) => {
  switch (roleId) {
    case 1:
      return "admin";
    case 2:
      return "viewer";
    case 3:
      return "source";
    default:
      return "";
  }
};

//Reports constants
export const PBIReportBaseUrl = "https://app.powerbigov.us/reportEmbed?autoAuth=true&ctid=11d0e217-264e-400a-8ba0-57dcc127d72d&reportId=";
export const LinkIdOperationsReportId = "7e198861-5dfb-48fc-ae6c-e4ff7dbb4afb";
export const ActualSystemsConnectedReportId = "1027abc4-0616-4aab-aa53-8a3df5d49c27";
export const LinkIdMatchingMultipleSourcesReportId = "995e9e27-1535-4dbf-9199-145e532241f2";
export const UserAccessByAgencyReportId = "ea58671c-5145-495c-b0d2-d587df6716f0";

//Help Sharepoint Constants
export const SharePointLinkForHelp = "https://stateofwa.sharepoint.com/sites/HCA-masterpersonindex/SitePages/Request-onboarding-and-user-access.aspx";
export const MPISupportEmail = "mpi@hca.wa.gov";