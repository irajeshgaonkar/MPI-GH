export const environment = {
  production: true,
  consultantApiBaseUrl: "https://woadn5fn0c-vpce-0b17317da64a14a3d.execute-api.us-west-2.amazonaws.com/prod/",
  adminConsoleApiBaseUrl: "https://tfpjripbh0-vpce-0b17317da64a14a3d.execute-api.us-west-2.amazonaws.com/prod/",
  supportRequestUrl: "",
  auth: {
    ssoBaseUrl: "https://mpi.auth.us-west-2.amazoncognito.com/oauth2/authorize",
    userInfoUrl: "https://mpi.auth.us-west-2.amazoncognito.com/oauth2/userInfo",
    identityUrl: "HCASSO",
    redirectUri: "http://localhost:4200/login",
    //redirectUri: "https://hhscoalitionmpi.hca.wa.gov/login",
    clientId: "2k2oo5gqiv7a672q0675i6f35s",
    adminGroupName: "U-S-HCA HHSCoalitionMPI-Prod FC",
  },
};
