using HCA.Data.Entities;
using HCA.Data.Repository.Core;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace HCA.Data.Repository.Impl
{
    public class OnboardedSystemRepository(HcaDbContext dbContext) : RepositoryBase<OnboardedSystemEntity>(dbContext), IOnboardedSystemRepository
    {
        private readonly HcaDbContext _hcaDbContext = dbContext;

        public async Task<List<string>> GetActiveSourceSystemsByIPAsync(string incomingIpAddress)
        {
            // Custom query to filter the sources at the database level for better performance.
            // We are checking if the incoming IP address falls within the specified range 
            // (start_ipaddress to end_ipaddress) or if it belongs to the CIDR block defined in ip_cidr.
            // Additionally, only systems with an active status (isactive = TRUE) are considered.

            var query = $"SELECT source_system_name FROM coalitionmpi.onboarded_system " +
                        $"WHERE (INET('{incomingIpAddress}') BETWEEN INET(start_ip_address) AND INET(end_ip_address)) " +
                        $"OR (INET('{incomingIpAddress}') <<= INET(ip_address_cidr)) " +
                        "AND is_active = TRUE";

            return await _hcaDbContext.OnboardedSystem.FromSqlRaw(query, new NpgsqlParameter("@incoming_ip", incomingIpAddress))
                .Select(system => system.SourceSystemName).ToListAsync();
        }
    }
}
