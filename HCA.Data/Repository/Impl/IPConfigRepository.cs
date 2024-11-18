using HCA.Data.Entities;
using HCA.Data.Repository.Core;
using Microsoft.EntityFrameworkCore;

namespace HCA.Data.Repository.Impl
{
    public class IPConfigRepository : RepositoryBase<IpAddressesEntity>, IIPConfigRepository
    {
        private readonly HcaDbContext _hcaDbContext;
        public IPConfigRepository(HcaDbContext dbContext) : base(dbContext)
        {
            _hcaDbContext = dbContext;
        }

        public async Task<bool> IsIPAddressTrustedAsync(string sourceSystem, string ipAddress)
        {
            return await _hcaDbContext.IpAddresses.AnyAsync(ip => ip.SourceSystem == sourceSystem && ip.IpAddress == ipAddress);
        }

        public async Task<string> GetSourceSystemFromIp( string ipAddress ) 
        {
            return (await _hcaDbContext.IpAddresses.SingleAsync( ip => ip.IpAddress==ipAddress )).SourceSystem;
        }

        /// <summary>
        /// Get all Source systems with matching IP Address
        /// </summary>
        /// <param name="ipAddress"></param>
        /// <returns>List of source systems</returns>
        public async Task<List<string>> GetSourceSystemsFromIPAsync(string ipAddress)
        {
            return await _hcaDbContext.IpAddresses.Where(ip => ip.IpAddress == ipAddress).Select(a => a.SourceSystem).ToListAsync();
        }
    }
}
