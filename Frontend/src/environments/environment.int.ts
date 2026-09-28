export const environment = {
  production: true,
  consultantApiBaseUrl: "https://g1m76k306d-vpce-0a3996237c8c46915.execute-api.us-west-2.amazonaws.com/INTUI_Search/",
  adminConsoleApiBaseUrl: "https://j759lmfbga-vpce-0a3996237c8c46915.execute-api.us-west-2.amazonaws.com/int/",
  supportRequestUrl: "https://support.hca.wa.gov/hcasupport?id=sc_cat_item&sys_id=1772e0d71b947d5047842020f54bcbed",
  auth: {
    ssoBaseUrl: "https://mpi-int.auth.us-west-2.amazoncognito.com/oauth2/authorize",
    userInfoUrl: "https://mpi-int.auth.us-west-2.amazoncognito.com/oauth2/userInfo",
    identityUrl: "HCASSO",
    //redirectUri: "http://localhost:4200/login",
    redirectUri: "https://hhscoalitionmpi-int.hca.wa.gov/login",
    clientId: "52u09eogosqfpufvr18j98m7c3",
    adminGroupName: "U-S-HCA HHSCoalitionMPI-Int FC",
  },
};
