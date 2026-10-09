export const environment = {
  production: true,
  consultantApiBaseUrl: "https://zr5n08vswj-vpce-04eda9a07ef81f1b1.execute-api.us-west-2.amazonaws.com/test/",
  adminConsoleApiBaseUrl: "https://x0fxb42r16-vpce-04eda9a07ef81f1b1.execute-api.us-west-2.amazonaws.com/test/",
  supportRequestUrl: "https://support.hca.wa.gov/hcasupport?id=sc_cat_item&sys_id=1772e0d71b947d5047842020f54bcbed",
  auth: {
    ssoBaseUrl: "https://mpi-test.auth.us-west-2.amazoncognito.com/oauth2/authorize",
    userInfoUrl: "https://mpi-test.auth.us-west-2.amazoncognito.com/oauth2/userInfo",
    identityUrl: "HCASSO",
    //redirectUri: "http://localhost:4200/login",
    redirectUri: "https://hhscoalitionmpi-test.hca.wa.gov/login",
    clientId: "20sqsajf3i7nhk676ni1iog5ne",
    adminGroupName: "U-S-HCA HHSCoalitionMPI-Test FC",
  },
};
