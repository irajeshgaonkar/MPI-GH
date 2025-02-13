namespace HCA.Models.MuleSoft
{
    /// <summary>
    /// Represents the configuration options required for authenticating with Azure Active Directory (AD).
    /// </summary>
    public class AdOptions
    {
        /// <summary>
        /// Gets or sets the tenant identifier for the Azure Active Directory.
        /// This is typically the directory ID or the domain name of your organization.
        /// </summary>
        public string Tenant { get; set; }

        /// <summary>
        /// Gets or sets the client ID of the application registered in Azure AD.
        /// The client ID is used to identify the application to Azure AD.
        /// </summary>
        public string ClientId { get; set; }

        /// <summary>
        /// Gets or sets the client secret for the application registered in Azure AD.
        /// The client secret is used in combination with the client ID to authenticate the application.
        /// </summary>
        public string ClientSecret { get; set; }

        /// <summary>
        /// Gets or sets the grant type for the OAuth2 flow used to authenticate with Azure AD.
        /// Common values include "client_credentials", "authorization_code", etc.
        /// </summary>
        public string GrantType { get; set; }

        /// <summary>
        /// Gets or sets the scope of the access request.
        /// Scopes define the permissions being requested from Azure AD, such as access to APIs or resources.
        /// </summary>
        public string Scope { get; set; }
    }

}
