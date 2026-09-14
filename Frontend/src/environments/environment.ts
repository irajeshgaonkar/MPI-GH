// This file can be replaced during build by using the `fileReplacements` array.
// `ng build` replaces `environment.ts` with `environment.prod.ts`.
// The list of file replacements can be found in `angular.json`.

export const environment = {
  production: false,
  consultantApiBaseUrl: "https://xti5pzg5w7-vpce-0b68a431807fa86e6.execute-api.us-west-2.amazonaws.com/dev/",
  adminConsoleApiBaseUrl: "https://hd85q67nj8-vpce-0b68a431807fa86e6.execute-api.us-west-2.amazonaws.com/dev/",
  supportRequestUrl: "https://support.hca.wa.gov/hcasupport?id=sc_cat_item&sys_id=1772e0d71b947d5047842020f54bcbed",
  auth: {
    ssoBaseUrl: "https://mpi-dev.auth.us-west-2.amazoncognito.com/oauth2/authorize",
    userInfoUrl: "https://mpi-dev.auth.us-west-2.amazoncognito.com/oauth2/userInfo",
    identityUrl: "HCASSO",
    redirectUri: "http://localhost:4200/login",
    //redirectUri: "https://hhscoalitionmpi-dev.hca.wa.gov/login",
    clientId: "6mffm638q1vpacvtu8g8693u11",
    adminGroupName: "U-S-HCA HHSCoalitionMPI-Dev FC",
  },
};

/*
 * For easier debugging in development mode, you can import the following file
 * to ignore zone related error stack frames such as `zone.run`, `zoneDelegate.invokeTask`.
 *
 * This import should be commented out in production mode because it will have a negative impact
 * on performance if an error is thrown.
 */
// import 'zone.js/plugins/zone-error';  // Included with Angular CLI.
