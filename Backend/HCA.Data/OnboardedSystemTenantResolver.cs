using Microsoft.EntityFrameworkCore;

namespace HCA.Data;

public sealed class OnboardedSystemTenantResolver(IHcaDbContextAccessor dbContextAccessor) : IOnboardedSystemTenantResolver
{
    private readonly IHcaDbContextAccessor _dbContextAccessor = dbContextAccessor;

    public async Task<OnboardedSystemIpLookupResult> ResolveByIpAsync(string incomingIpAddress)
    {
        var coalitionLookupTask = GetActiveSourceSystemsByIpAsync(_dbContextAccessor.Coalition, incomingIpAddress);
        Task<List<string>>? nonCoalitionLookupTask = null;

        if (_dbContextAccessor.HasNonCoalitionDatabase)
        {
            nonCoalitionLookupTask = GetActiveSourceSystemsByIpAsync(_dbContextAccessor.NonCoalition, incomingIpAddress);
        }

        if (nonCoalitionLookupTask != null)
        {
            await Task.WhenAll(coalitionLookupTask, nonCoalitionLookupTask);
        }

        var coalitionSourceSystems = await coalitionLookupTask;
        if (coalitionSourceSystems.Count > 0)
        {
            return new OnboardedSystemIpLookupResult
            {
                TenantDatabase = TenantDatabaseKind.Coalition,
                SourceSystems = coalitionSourceSystems
            };
        }

        var nonCoalitionSourceSystems = nonCoalitionLookupTask == null
            ? []
            : await nonCoalitionLookupTask;

        return new OnboardedSystemIpLookupResult
        {
            TenantDatabase = TenantDatabaseKind.NonCoalition,
            SourceSystems = nonCoalitionSourceSystems
        };
    }

    private static async Task<List<string>> GetActiveSourceSystemsByIpAsync(HcaDbContext dbContext, string incomingIpAddress)
    {
        return await dbContext.OnboardedSystem
            .FromSqlInterpolated($@"
SELECT *
FROM onboarded_system
WHERE (
        (INET({incomingIpAddress}) BETWEEN INET(start_ip_address) AND INET(end_ip_address))
        OR (INET({incomingIpAddress}) <<= INET(ip_address_cidr))
      )
  AND is_active = TRUE")
            .Select(system => system.SourceSystemName!)
            .ToListAsync();
    }
}
