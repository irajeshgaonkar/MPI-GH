namespace HCA.Models.MuleSoft
{
    /// <summary>
    /// Represents the configuration options required for integrating with MuleSoft.
    /// </summary>
    public class MuleSoftOptions
    {
        /// <summary>
        /// Gets or sets the base URL of the MuleSoft API or endpoint.
        /// This URL is used as the foundation for making requests to the MuleSoft platform.
        /// </summary>
        public string BaseUrl { get; set; }

        /// <summary>
        /// Gets or sets the Azure Active Directory (AD) configuration options.
        /// These options are used for authenticating with Azure AD to retrieve tokens for accessing MuleSoft resources.
        /// </summary>
        public AdOptions AdOptions { get; set; }
    }
}
