using System.Security.Claims;
using System.Net;
using System.Text.RegularExpressions;
using HCA.AdminMetrics.Api.Models;
using HCA.AdminMetrics.Api.Options;
using HCA.Data;
using HCA.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace HCA.AdminMetrics.Api.Services;

/// <summary>
/// Provides CRUD operations for onboarded systems and their whitelist entries.
/// </summary>
public partial class AdminOnboardedSystemService(
    HcaDbContext dbContext,
    IHttpContextAccessor httpContextAccessor,
    IOptions<AdminMetricsOptions> options ) : IAdminOnboardedSystemService
{
    private readonly HashSet<string> _allowedTenants = options.Value.AllowedTenants
            .Select(CleanOptional)
            .Where(tenant => !string.IsNullOrWhiteSpace(tenant))
            .Select(tenant => tenant!)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

    /// <inheritdoc />
    public async Task<AdminOnboardedSystemListResponse> GetAllAsync(CancellationToken cancellationToken)
    {
        var rows = await dbContext.OnboardedSystem
            .AsNoTracking()
            .OrderBy(system => system.SourceSystemName)
            .ThenBy(system => system.AgencyName)
            .ThenBy(system => system.Id)
            .ToListAsync(cancellationToken);

        return new AdminOnboardedSystemListResponse
        {
            Systems = rows
                .GroupBy(row => BuildSystemGroupKey(row.SourceSystemName, row.Tenant), StringComparer.OrdinalIgnoreCase)
                .Select(MapGroup)
                .OrderBy(system => system.SourceSystemName)
                .ThenBy(system => system.Tenant)
                .ThenBy(system => system.AgencyName)
                .ToList()
        };
    }

    /// <inheritdoc />
    public async Task<AdminOnboardedSystemDto?> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        var anchor = await dbContext.OnboardedSystem
            .AsNoTracking()
            .FirstOrDefaultAsync(system => system.Id == id, cancellationToken);

        if (anchor == null)
        {
            return null;
        }

        var rows = await LoadGroupAsync(anchor.SourceSystemName, anchor.Tenant, cancellationToken);
        if (rows.Count == 0)
        {
            return null;
        }

        return MapGroup(rows);
    }

    /// <inheritdoc />
    public async Task<AdminOnboardedSystemDto> CreateAsync(AdminOnboardedSystemRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        ValidateSystemRequest(request, _allowedTenants);

        var normalizedSourceSystemName = CleanRequired(request.SourceSystemName, nameof(request.SourceSystemName));
        var normalizedTenant = CleanRequired(request.Tenant, nameof(request.Tenant));
        var existingRows = await LoadGroupForUpdateAsync(normalizedSourceSystemName, normalizedTenant, cancellationToken);
        if (existingRows.Count != 0 )
        {
            var existing = MapGroup(existingRows);
            throw new InvalidOperationException($"Source system '{existing.SourceSystemName}' for tenant '{existing.Tenant}' already exists. Edit the existing system instead of creating a duplicate.");
        }

        var actor = GetActor();
        var now = DateTime.UtcNow;
        var normalizedRows = NormalizeWhitelistRequests(request.IpWhitelists);

        if (normalizedRows.Count == 0 )
        {
            normalizedRows.Add(new AdminOnboardedSystemIpWhitelistRequest());
        }

        var entities = normalizedRows.Select(whitelist => new OnboardedSystemEntity
        {
            SourceSystemName = normalizedSourceSystemName,
            AgencyName = CleanRequired(request.AgencyName, nameof(request.AgencyName)),
            Tenant = normalizedTenant,
            ConnectivityMode = CleanOptional(request.ConnectivityMode),
            OnboardingDate = request.OnboardingDate == default ? now.Date : request.OnboardingDate,
            IsActive = request.IsActive,
            EnableNotification = request.EnableNotification,
            StartIpAddress = CleanOptional(whitelist.StartIpAddress),
            EndIpAddress = CleanOptional(whitelist.EndIpAddress),
            IpCidr = CleanOptional(whitelist.IpAddressCidr),
            CreatedBy = actor,
            CreatedDate = now,
            ModifiedBy = actor,
            ModifiedDate = now
        }).ToList();

        dbContext.OnboardedSystem.AddRange(entities);
        await dbContext.SaveChangesAsync(cancellationToken);

        return MapGroup(entities);
    }

    /// <inheritdoc />
    public async Task<AdminOnboardedSystemDto> UpdateAsync(int id, AdminOnboardedSystemRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        ValidateSystemRequest(request, _allowedTenants);

        var anchor = await dbContext.OnboardedSystem.FirstOrDefaultAsync(system => system.Id == id, cancellationToken)
            ?? throw new InvalidOperationException("Onboarded system was not found.");
        var rows = await LoadGroupForUpdateAsync(anchor.SourceSystemName, anchor.Tenant, cancellationToken);
        if (rows.Count == 0)
        {
            throw new InvalidOperationException("Onboarded system rows were not found.");
        }

        var actor = GetActor();
        var now = DateTime.UtcNow;
        var normalizedWhitelists = NormalizeWhitelistRequests(request.IpWhitelists);
        var normalizedSourceSystemName = CleanRequired(request.SourceSystemName, nameof(request.SourceSystemName));
        var normalizedTenant = CleanRequired(request.Tenant, nameof(request.Tenant));
        var currentGroupKey = BuildSystemGroupKey(anchor.SourceSystemName, anchor.Tenant);
        var requestedGroupKey = BuildSystemGroupKey(normalizedSourceSystemName, normalizedTenant);

        if (!string.Equals(currentGroupKey, requestedGroupKey, StringComparison.OrdinalIgnoreCase))
        {
            var conflictingRows = await LoadGroupForUpdateAsync(normalizedSourceSystemName, normalizedTenant, cancellationToken);
            if (conflictingRows.Any(row => row.Id != anchor.Id))
            {
                throw new InvalidOperationException($"Source system '{normalizedSourceSystemName}' for tenant '{normalizedTenant}' already exists.");
            }
        }

        if (normalizedWhitelists.Count == 0 )
        {
            normalizedWhitelists.Add(new AdminOnboardedSystemIpWhitelistRequest());
        }

        var existingById = rows.ToDictionary(row => row.Id);
        var requestedIds = normalizedWhitelists.Where(item => item.Id.HasValue && item.Id.Value > 0).Select(item => item.Id!.Value).ToHashSet();
        var invalidRequestedIds = requestedIds.Where(requestedId => !existingById.ContainsKey(requestedId)).ToList();
        if (invalidRequestedIds.Count != 0)
        {
            throw new InvalidOperationException("One or more whitelist rows do not belong to this onboarded system.");
        }

        foreach (var row in rows.Where(row => !requestedIds.Contains(row.Id)).ToList())
        {
            dbContext.OnboardedSystem.Remove(row);
            rows.Remove(row);
        }

        foreach (var whitelist in normalizedWhitelists)
        {
            OnboardedSystemEntity row;
            if (whitelist.Id.HasValue && whitelist.Id.Value > 0 && existingById.TryGetValue(whitelist.Id.Value, out var existing))
            {
                row = existing;
            }
            else
            {
                row = new OnboardedSystemEntity
                {
                    CreatedBy = actor,
                    CreatedDate = now
                };
                rows.Add(row);
                dbContext.OnboardedSystem.Add(row);
            }

            row.SourceSystemName = CleanRequired(request.SourceSystemName, nameof(request.SourceSystemName));
            row.AgencyName = CleanRequired(request.AgencyName, nameof(request.AgencyName));
            row.Tenant = normalizedTenant;
            row.ConnectivityMode = CleanOptional(request.ConnectivityMode);
            row.OnboardingDate = request.OnboardingDate == default ? row.OnboardingDate : request.OnboardingDate;
            row.IsActive = request.IsActive;
            row.EnableNotification = request.EnableNotification;
            row.StartIpAddress = CleanOptional(whitelist.StartIpAddress);
            row.EndIpAddress = CleanOptional(whitelist.EndIpAddress);
            row.IpCidr = CleanOptional(whitelist.IpAddressCidr);
            row.ModifiedBy = actor;
            row.ModifiedDate = now;
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        var refreshedRows = await LoadGroupAsync(normalizedSourceSystemName, normalizedTenant, cancellationToken);
        return MapGroup(refreshedRows);
    }

    /// <inheritdoc />
    public async Task DeleteAsync(int id, CancellationToken cancellationToken)
    {
        var anchor = await dbContext.OnboardedSystem.FirstOrDefaultAsync(system => system.Id == id, cancellationToken)
            ?? throw new InvalidOperationException("Onboarded system was not found.");
        var rows = await LoadGroupForUpdateAsync(anchor.SourceSystemName, anchor.Tenant, cancellationToken);
        if (rows.Count == 0)
        {
            throw new InvalidOperationException("Onboarded system rows were not found.");
        }

        dbContext.OnboardedSystem.RemoveRange(rows);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<AdminOnboardedSystemDto> UpdateNotificationSettingsAsync(int id, bool enableNotification, CancellationToken cancellationToken)
    {
        var anchor = await dbContext.OnboardedSystem.FirstOrDefaultAsync(system => system.Id == id, cancellationToken)
            ?? throw new InvalidOperationException("Onboarded system was not found.");
        var rows = await LoadGroupForUpdateAsync(anchor.SourceSystemName, anchor.Tenant, cancellationToken);
        if (rows.Count == 0)
        {
            throw new InvalidOperationException("Onboarded system rows were not found.");
        }

        var actor = GetActor();
        var now = DateTime.UtcNow;

        foreach (var row in rows)
        {
            row.EnableNotification = enableNotification;
            row.ModifiedBy = actor;
            row.ModifiedDate = now;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        return MapGroup(rows);
    }

    /// <inheritdoc />
    public async Task<AdminOnboardedSystemDto> AddIpWhitelistAsync(int id, AdminOnboardedSystemIpWhitelistRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        ValidateWhitelistRequest(request);
        var anchor = await dbContext.OnboardedSystem.FirstOrDefaultAsync(system => system.Id == id, cancellationToken)
            ?? throw new InvalidOperationException("Onboarded system was not found.");
        var rows = await LoadGroupForUpdateAsync(anchor.SourceSystemName, anchor.Tenant, cancellationToken);
        if (rows.Count == 0)
        {
            throw new InvalidOperationException("Onboarded system rows were not found.");
        }

        var actor = GetActor();
        var now = DateTime.UtcNow;
        var template = rows.OrderBy(row => row.Id).First();

        var entity = new OnboardedSystemEntity
        {
            SourceSystemName = template.SourceSystemName,
            AgencyName = template.AgencyName,
            Tenant = template.Tenant,
            ConnectivityMode = template.ConnectivityMode,
            OnboardingDate = template.OnboardingDate,
            IsActive = template.IsActive,
            EnableNotification = template.EnableNotification,
            StartIpAddress = CleanOptional(request.StartIpAddress),
            EndIpAddress = CleanOptional(request.EndIpAddress),
            IpCidr = CleanOptional(request.IpAddressCidr),
            CreatedBy = actor,
            CreatedDate = now,
            ModifiedBy = actor,
            ModifiedDate = now
        };

        dbContext.OnboardedSystem.Add(entity);
        await dbContext.SaveChangesAsync(cancellationToken);

        var refreshedRows = await LoadGroupAsync(anchor.SourceSystemName, anchor.Tenant, cancellationToken);
        return MapGroup(refreshedRows);
    }

    /// <inheritdoc />
    public async Task<AdminOnboardedSystemDto> UpdateIpWhitelistAsync(int id, int whitelistId, AdminOnboardedSystemIpWhitelistRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        ValidateWhitelistRequest(request);
        var anchor = await dbContext.OnboardedSystem.FirstOrDefaultAsync(system => system.Id == id, cancellationToken)
            ?? throw new InvalidOperationException("Onboarded system was not found.");
        var rows = await LoadGroupForUpdateAsync(anchor.SourceSystemName, anchor.Tenant, cancellationToken);
        if (rows.Count == 0)
        {
            throw new InvalidOperationException("Onboarded system rows were not found.");
        }

        var row = rows.FirstOrDefault(item => item.Id == whitelistId)
            ?? throw new InvalidOperationException("Whitelist row was not found.");

        row.StartIpAddress = CleanOptional(request.StartIpAddress);
        row.EndIpAddress = CleanOptional(request.EndIpAddress);
        row.IpCidr = CleanOptional(request.IpAddressCidr);
        row.ModifiedBy = GetActor();
        row.ModifiedDate = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);
        return MapGroup(rows);
    }

    /// <inheritdoc />
    public async Task<AdminOnboardedSystemDto> DeleteIpWhitelistAsync(int id, int whitelistId, CancellationToken cancellationToken)
    {
        var anchor = await dbContext.OnboardedSystem.FirstOrDefaultAsync(system => system.Id == id, cancellationToken)
            ?? throw new InvalidOperationException("Onboarded system was not found.");
        var rows = await LoadGroupForUpdateAsync(anchor.SourceSystemName, anchor.Tenant, cancellationToken);
        if (rows.Count == 0)
        {
            throw new InvalidOperationException("Onboarded system rows were not found.");
        }

        var row = rows.FirstOrDefault(item => item.Id == whitelistId)
            ?? throw new InvalidOperationException("Whitelist row was not found.");

        if (rows.Count == 1)
        {
            row.StartIpAddress = null;
            row.EndIpAddress = null;
            row.IpCidr = null;
            row.ModifiedBy = GetActor();
            row.ModifiedDate = DateTime.UtcNow;
        }
        else
        {
            dbContext.OnboardedSystem.Remove(row);
            rows.Remove(row);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        var refreshedRows = await LoadGroupAsync(anchor.SourceSystemName, anchor.Tenant, cancellationToken);
        return MapGroup(refreshedRows);
    }

    private async Task<List<OnboardedSystemEntity>> LoadGroupAsync(string? sourceSystemName, string? tenant, CancellationToken cancellationToken)
    {
        var key = BuildSystemGroupKey(sourceSystemName, tenant);
        var rows = await dbContext.OnboardedSystem
            .AsNoTracking()
            .OrderBy(system => system.Id)
            .ToListAsync(cancellationToken);

        return rows
            .Where(system => string.Equals(BuildSystemGroupKey(system.SourceSystemName, system.Tenant), key, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    private async Task<List<OnboardedSystemEntity>> LoadGroupForUpdateAsync(string? sourceSystemName, string? tenant, CancellationToken cancellationToken)
    {
        var key = BuildSystemGroupKey(sourceSystemName, tenant);
        var rows = await dbContext.OnboardedSystem
            .OrderBy(system => system.Id)
            .ToListAsync(cancellationToken);

        return rows
            .Where(system => string.Equals(BuildSystemGroupKey(system.SourceSystemName, system.Tenant), key, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    private static AdminOnboardedSystemDto MapGroup(IEnumerable<OnboardedSystemEntity> rows)
    {
        var orderedRows = rows.OrderBy(item => item.Id).ToList();
        if (orderedRows.Count == 0)
        {
            throw new InvalidOperationException("Onboarded system rows were not found.");
        }

        var primary = orderedRows.First();
        var sourceSystemName = PickPreferredValue(orderedRows.Select(item => item.SourceSystemName));
        var agencyName = PickPreferredValue(orderedRows.Select(item => item.AgencyName));
        var tenant = PickPreferredValue(orderedRows.Select(item => item.Tenant));
        var connectivityMode = PickPreferredOptionalValue(orderedRows.Select(item => item.ConnectivityMode));
        var createdBy = PickPreferredOptionalValue(orderedRows.Select(item => item.CreatedBy));
        var distinctWhitelistRows = orderedRows
            .GroupBy(item => BuildWhitelistKey(item.StartIpAddress, item.EndIpAddress, item.IpCidr), StringComparer.OrdinalIgnoreCase)
            .Select(group => group.OrderBy(item => item.Id).First())
            .ToList();

        return new AdminOnboardedSystemDto
        {
            Id = primary.Id,
            SourceSystemName = sourceSystemName,
            AgencyName = agencyName,
            Tenant = tenant,
            ConnectivityMode = connectivityMode,
            OnboardingDate = primary.OnboardingDate,
            IsActive = orderedRows.All(item => item.IsActive ?? false),
            EnableNotification = primary.EnableNotification ?? false,
            StartIpAddress = primary.StartIpAddress,
            EndIpAddress = primary.EndIpAddress,
            IpAddressCidr = primary.IpCidr,
            CreatedBy = createdBy,
            CreatedDate = primary.CreatedDate,
            ModifiedBy = primary.ModifiedBy,
            ModifiedDate = primary.ModifiedDate,
            IpWhitelists = distinctWhitelistRows
                .Select(item => new AdminOnboardedSystemIpWhitelistDto
                {
                    Id = item.Id,
                    StartIpAddress = item.StartIpAddress,
                    EndIpAddress = item.EndIpAddress,
                    IpAddressCidr = item.IpCidr
                })
                .ToList()
        };
    }

    private static void ValidateSystemRequest(AdminOnboardedSystemRequest request, HashSet<string> allowedTenants)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (string.IsNullOrWhiteSpace(request.SourceSystemName))
        {
            throw new InvalidOperationException("Source system name is required.");
        }

        if (string.IsNullOrWhiteSpace(request.AgencyName))
        {
            throw new InvalidOperationException("Agency name is required.");
        }

        if (string.IsNullOrWhiteSpace(request.Tenant))
        {
            throw new InvalidOperationException("Tenant is required.");
        }

        var normalizedTenant = CleanRequired(request.Tenant, nameof(request.Tenant));
        if (!allowedTenants.Contains(normalizedTenant))
        {
            throw new InvalidOperationException($"Tenant must be one of: {string.Join(", ", allowedTenants.OrderBy(tenant => tenant))}.");
        }

        foreach (var whitelist in request.IpWhitelists ?? [])
        {
            ValidateWhitelistRequest(whitelist);
        }
    }

    private static void ValidateWhitelistRequest(AdminOnboardedSystemIpWhitelistRequest? request)
    {
        if (request == null)
        {
            throw new InvalidOperationException("IP whitelist entry is required.");
        }

        var start = CleanOptional(request.StartIpAddress);
        var end = CleanOptional(request.EndIpAddress);
        var cidr = CleanOptional(request.IpAddressCidr);
        var hasRange = !string.IsNullOrWhiteSpace(start) || !string.IsNullOrWhiteSpace(end);

        if (!string.IsNullOrWhiteSpace(start) ^ !string.IsNullOrWhiteSpace(end))
        {
            throw new InvalidOperationException("IP whitelist range entries require both a start and end IP address.");
        }

        if (!hasRange && string.IsNullOrWhiteSpace(cidr))
        {
            return;
        }

        if (hasRange && !string.IsNullOrWhiteSpace(cidr))
        {
            throw new InvalidOperationException("An IP whitelist entry must use either a CIDR block or a start/end range, not both.");
        }

        if (hasRange)
        {
            if (!TryParseIpv4(start, out var startAddress) || !TryParseIpv4(end, out var endAddress))
            {
                throw new InvalidOperationException("IP whitelist range entries must use valid IPv4 addresses.");
            }

            if (Ipv4ToUInt32(startAddress) > Ipv4ToUInt32(endAddress))
            {
                throw new InvalidOperationException("IP whitelist range end address must be greater than or equal to the start address.");
            }
        }

        if (!string.IsNullOrWhiteSpace(cidr) && !IsValidCidr(cidr))
        {
            throw new InvalidOperationException("IP whitelist CIDR entries must use valid IPv4 CIDR notation.");
        }
    }

    private static List<AdminOnboardedSystemIpWhitelistRequest> NormalizeWhitelistRequests(List<AdminOnboardedSystemIpWhitelistRequest>? requests)
    {
        return (requests ?? [])
            .Where(request => request != null)
            .Select(request => new AdminOnboardedSystemIpWhitelistRequest
            {
                Id = request.Id,
                StartIpAddress = CleanOptional(request.StartIpAddress),
                EndIpAddress = CleanOptional(request.EndIpAddress),
                IpAddressCidr = CleanOptional(request.IpAddressCidr)
            })
            .Where(request => !string.IsNullOrWhiteSpace(request.StartIpAddress)
                || !string.IsNullOrWhiteSpace(request.EndIpAddress)
                || !string.IsNullOrWhiteSpace(request.IpAddressCidr))
            .GroupBy(request => $"{request.StartIpAddress}|{request.EndIpAddress}|{request.IpAddressCidr}", StringComparer.OrdinalIgnoreCase)
            .Select(group => group.First())
            .ToList();
    }

    private static bool IsValidCidr(string cidr)
    {
        var parts = cidr.Split('/');
        if (parts.Length != 2 || !TryParseIpv4(parts[0], out _) || !int.TryParse(parts[1], out var prefix))
        {
            return false;
        }

        return prefix is >= 0 and <= 32;
    }

    private static bool TryParseIpv4(string? value, out IPAddress address)
    {
        return IPAddress.TryParse(value, out address!)
            && address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork;
    }

    private static uint Ipv4ToUInt32(IPAddress address)
    {
        var bytes = address.GetAddressBytes();
        return ((uint)bytes[0] << 24) | ((uint)bytes[1] << 16) | ((uint)bytes[2] << 8) | bytes[3];
    }

    private string GetActor()
    {
        return httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier) ?? "system";
    }

    private static string NormalizeKey(string? value)
    {
        return NormalizeWhitespace(value).ToLowerInvariant();
    }

    private static string BuildSystemGroupKey(string? sourceSystemName, string? tenant)
    {
        return $"{NormalizeKey(sourceSystemName)}|{NormalizeKey(tenant)}";
    }

    private static string CleanRequired(string value, string fieldName)
    {
        var cleaned = CleanOptional(value);
        if (string.IsNullOrWhiteSpace(cleaned))
        {
            throw new InvalidOperationException($"{fieldName} is required.");
        }

        return cleaned;
    }

    private static string? CleanOptional(string? value)
    {
        var normalized = NormalizeWhitespace(value);
        return string.IsNullOrWhiteSpace(normalized) ? null : normalized;
    }

    private static string NormalizeWhitespace(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        return WhiteSpaceRegex().Replace( value.Trim(), " " );
    }

    private static string PickPreferredValue(IEnumerable<string?> values)
    {
        return values
            .Select(CleanOptional)
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .GroupBy(value => value!, StringComparer.OrdinalIgnoreCase)
            .OrderByDescending(group => group.Count())
            .ThenBy(group => group.Key, StringComparer.OrdinalIgnoreCase)
            .Select(group => group.First())
            .FirstOrDefault() ?? string.Empty;
    }

    private static string? PickPreferredOptionalValue(IEnumerable<string?> values)
    {
        return values
            .Select(CleanOptional)
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .GroupBy(value => value!, StringComparer.OrdinalIgnoreCase)
            .OrderByDescending(group => group.Count())
            .ThenBy(group => group.Key, StringComparer.OrdinalIgnoreCase)
            .Select(group => group.First())
            .FirstOrDefault();
    }

    private static string BuildWhitelistKey(string? startIpAddress, string? endIpAddress, string? ipCidr)
    {
        return $"{NormalizeKey(startIpAddress)}|{NormalizeKey(endIpAddress)}|{NormalizeKey(ipCidr)}";
    }

    [GeneratedRegex( "\\s+" )]
    private static partial Regex WhiteSpaceRegex();
}
