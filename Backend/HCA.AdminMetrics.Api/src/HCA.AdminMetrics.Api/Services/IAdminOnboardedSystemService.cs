using HCA.AdminMetrics.Api.Models;

namespace HCA.AdminMetrics.Api.Services;

/// <summary>
/// Defines operations for managing onboarded source systems and IP whitelist entries.
/// </summary>
public interface IAdminOnboardedSystemService
{
    /// <summary>
    /// Gets all onboarded systems.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The onboarded system list response.</returns>
    Task<AdminOnboardedSystemListResponse> GetAllAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Gets a single onboarded system by identifier.
    /// </summary>
    /// <param name="id">The onboarded system identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The onboarded system, or <see langword="null"/> when not found.</returns>
    Task<AdminOnboardedSystemDto?> GetByIdAsync(int id, CancellationToken cancellationToken);

    /// <summary>
    /// Creates a new onboarded system.
    /// </summary>
    /// <param name="request">The creation request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The created onboarded system.</returns>
    Task<AdminOnboardedSystemDto> CreateAsync(AdminOnboardedSystemRequest request, CancellationToken cancellationToken);

    /// <summary>
    /// Updates an existing onboarded system.
    /// </summary>
    /// <param name="id">The onboarded system identifier.</param>
    /// <param name="request">The update request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The updated onboarded system.</returns>
    Task<AdminOnboardedSystemDto> UpdateAsync(int id, AdminOnboardedSystemRequest request, CancellationToken cancellationToken);

    /// <summary>
    /// Deletes an onboarded system.
    /// </summary>
    /// <param name="id">The onboarded system identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task DeleteAsync(int id, CancellationToken cancellationToken);

    /// <summary>
    /// Updates the notification flag for an onboarded system.
    /// </summary>
    /// <param name="id">The onboarded system identifier.</param>
    /// <param name="enableNotification">The notification enabled flag.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The updated onboarded system.</returns>
    Task<AdminOnboardedSystemDto> UpdateNotificationSettingsAsync(int id, bool enableNotification, CancellationToken cancellationToken);

    /// <summary>
    /// Adds an IP whitelist entry to an onboarded system.
    /// </summary>
    /// <param name="id">The onboarded system identifier.</param>
    /// <param name="request">The whitelist request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The updated onboarded system.</returns>
    Task<AdminOnboardedSystemDto> AddIpWhitelistAsync(int id, AdminOnboardedSystemIpWhitelistRequest request, CancellationToken cancellationToken);

    /// <summary>
    /// Updates an existing IP whitelist entry for an onboarded system.
    /// </summary>
    /// <param name="id">The onboarded system identifier.</param>
    /// <param name="whitelistId">The whitelist identifier.</param>
    /// <param name="request">The whitelist request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The updated onboarded system.</returns>
    Task<AdminOnboardedSystemDto> UpdateIpWhitelistAsync(int id, int whitelistId, AdminOnboardedSystemIpWhitelistRequest request, CancellationToken cancellationToken);

    /// <summary>
    /// Deletes an IP whitelist entry from an onboarded system.
    /// </summary>
    /// <param name="id">The onboarded system identifier.</param>
    /// <param name="whitelistId">The whitelist identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The updated onboarded system.</returns>
    Task<AdminOnboardedSystemDto> DeleteIpWhitelistAsync(int id, int whitelistId, CancellationToken cancellationToken);
}
