using HCA.Data.Entities;
using HCA.Data.Repository.Core;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace HCA.Data.Repository.Impl
{
    public class OnboardedSystemRepository(IHcaDbContextAccessor dbContextAccessor) : RepositoryBase<OnboardedSystemEntity>(dbContextAccessor), IOnboardedSystemRepository
    {
        private readonly IHcaDbContextAccessor _dbContextAccessor = dbContextAccessor;

        public async Task<List<string>> GetActiveSourceSystemsByIPAsync(string incomingIpAddress)
        {
            // Custom query to filter the sources at the database level for better performance.
            // We are checking if the incoming IP address falls within the specified range 
            // (start_ipaddress to end_ipaddress) or if it belongs to the CIDR block defined in ip_cidr.
            // Additionally, only systems with an active status (isactive = TRUE) are considered.

            var query = $"SELECT source_system_name FROM onboarded_system " +
                        $"WHERE (INET('{incomingIpAddress}') BETWEEN INET(start_ip_address) AND INET(end_ip_address)) " +
                        $"OR (INET('{incomingIpAddress}') <<= INET(ip_address_cidr)) " +
                        "AND is_active = TRUE";

            return await _dbContextAccessor.Current.OnboardedSystem.FromSqlRaw(query, new NpgsqlParameter("@incoming_ip", incomingIpAddress))
                .Select(system => system.SourceSystemName).ToListAsync();
        }

        public async Task<Dictionary<string, string>> GetTenantMapBySourceSystemsAsync(IEnumerable<string> sourceSystemNames)
        {
            var normalizedSourceSystems = sourceSystemNames
                .Where(sourceSystemName => !string.IsNullOrWhiteSpace(sourceSystemName))
                .Select(sourceSystemName => sourceSystemName.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (normalizedSourceSystems.Count == 0)
            {
                return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            }

            var rows = await _dbContextAccessor.Current.OnboardedSystem
                .AsNoTracking()
                .Where(system => system.IsActive == true && normalizedSourceSystems.Contains(system.SourceSystemName!))
                .Select(system => new
                {
                    system.SourceSystemName,
                    system.Tenant
                })
                .ToListAsync();

            return rows
                .GroupBy(row => row.SourceSystemName ?? string.Empty, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(
                    group => group.Key,
                    group => string.Join(", ",
                        group.Select(row => row.Tenant)
                            .Where(tenant => !string.IsNullOrWhiteSpace(tenant))
                            .Select(tenant => tenant!.Trim())
                            .Distinct(StringComparer.OrdinalIgnoreCase)
                            .OrderBy(tenant => tenant, StringComparer.OrdinalIgnoreCase)),
                    StringComparer.OrdinalIgnoreCase);
        }

        public async Task<bool> ActiveSourceSystemExistsAsync(string sourceSystemName)
        {
            if (string.IsNullOrWhiteSpace(sourceSystemName))
            {
                return false;
            }

            var normalizedSourceSystemName = sourceSystemName.Trim().ToLower();

            return await _dbContextAccessor.Current.OnboardedSystem
                .AsNoTracking()
                .AnyAsync(system => system.IsActive == true && system.SourceSystemName != null && system.SourceSystemName.ToLower() == normalizedSourceSystemName);
        }
    }
}