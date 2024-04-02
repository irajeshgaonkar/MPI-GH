using HCA.Data.Entities;
using HCA.Data.Repository.Core;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

    }
}
