namespace HCA.AdminMetrics.Api.Models;

/// <summary>
/// Represents a list response for onboarded systems.
/// </summary>
public class AdminOnboardedSystemListResponse
{
    /// <summary>
    /// Gets or sets the onboarded systems included in the response.
    /// </summary>
    public List<AdminOnboardedSystemDto> Systems { get; set; } = [];
}

/// <summary>
/// Represents a single onboarded system.
/// </summary>
public class AdminOnboardedSystemDto
{
    /// <summary>
    /// Gets or sets the onboarded system identifier.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the source system name.
    /// </summary>
    public string SourceSystemName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the agency name.
    /// </summary>
    public string AgencyName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the tenant name.
    /// </summary>
    public string Tenant { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the connectivity mode.
    /// </summary>
    public string? ConnectivityMode { get; set; }

    /// <summary>
    /// Gets or sets the onboarding date.
    /// </summary>
    public DateTime? OnboardingDate { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the system is active.
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether notifications are enabled.
    /// </summary>
    public bool EnableNotification { get; set; }

    /// <summary>
    /// Gets or sets the start IP address for the primary row.
    /// </summary>
    public string? StartIpAddress { get; set; }

    /// <summary>
    /// Gets or sets the end IP address for the primary row.
    /// </summary>
    public string? EndIpAddress { get; set; }

    /// <summary>
    /// Gets or sets the CIDR notation for the primary row.
    /// </summary>
    public string? IpAddressCidr { get; set; }

    /// <summary>
    /// Gets or sets the user who created the system.
    /// </summary>
    public string? CreatedBy { get; set; }

    /// <summary>
    /// Gets or sets the creation timestamp.
    /// </summary>
    public DateTime? CreatedDate { get; set; }

    /// <summary>
    /// Gets or sets the user who last modified the system.
    /// </summary>
    public string? ModifiedBy { get; set; }

    /// <summary>
    /// Gets or sets the last modification timestamp.
    /// </summary>
    public DateTime? ModifiedDate { get; set; }

    /// <summary>
    /// Gets or sets the IP whitelist entries for the system.
    /// </summary>
    public List<AdminOnboardedSystemIpWhitelistDto> IpWhitelists { get; set; } = [];
}

/// <summary>
/// Represents an IP whitelist entry for an onboarded system.
/// </summary>
public class AdminOnboardedSystemIpWhitelistDto
{
    /// <summary>
    /// Gets or sets the whitelist identifier.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the starting IP address.
    /// </summary>
    public string? StartIpAddress { get; set; }

    /// <summary>
    /// Gets or sets the ending IP address.
    /// </summary>
    public string? EndIpAddress { get; set; }

    /// <summary>
    /// Gets or sets the CIDR notation for the whitelist entry.
    /// </summary>
    public string? IpAddressCidr { get; set; }
}

/// <summary>
/// Represents a request to create or update an onboarded system.
/// </summary>
public class AdminOnboardedSystemRequest
{
    /// <summary>
    /// Gets or sets the source system name.
    /// </summary>
    public string SourceSystemName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the agency name.
    /// </summary>
    public string AgencyName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the tenant name.
    /// </summary>
    public string Tenant { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the connectivity mode.
    /// </summary>
    public string? ConnectivityMode { get; set; }

    /// <summary>
    /// Gets or sets the onboarding date.
    /// </summary>
    public DateTime OnboardingDate { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the system is active.
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether notifications are enabled.
    /// </summary>
    public bool EnableNotification { get; set; }

    /// <summary>
    /// Gets or sets the requested IP whitelist entries.
    /// </summary>
    public List<AdminOnboardedSystemIpWhitelistRequest> IpWhitelists { get; set; } = [];
}

/// <summary>
/// Represents a request to create or update an onboarded system IP whitelist entry.
/// </summary>
public class AdminOnboardedSystemIpWhitelistRequest
{
    /// <summary>
    /// Gets or sets the whitelist identifier when updating an existing row.
    /// </summary>
    public int? Id { get; set; }

    /// <summary>
    /// Gets or sets the starting IP address.
    /// </summary>
    public string? StartIpAddress { get; set; }

    /// <summary>
    /// Gets or sets the ending IP address.
    /// </summary>
    public string? EndIpAddress { get; set; }

    /// <summary>
    /// Gets or sets the CIDR notation.
    /// </summary>
    public string? IpAddressCidr { get; set; }
}

/// <summary>
/// Represents a request to update notification settings for an onboarded system.
/// </summary>
public class AdminUpdateNotificationSettingsRequest
{
    /// <summary>
    /// Gets or sets a value indicating whether notifications are enabled.
    /// </summary>
    public bool EnableNotification { get; set; }
}
