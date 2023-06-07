using HCA.Data.Entities;
using HCA.Data.Repository.Core;
using HCA.Data.Repository;
using HCA.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HCA.Data.Repository.Impl;

namespace HCA.Data.Repository.Impl
{
    public class CustomDataMappingRepository : RepositoryBase<CustomDataMappingEntity>, ICustomDataMappingRepository
    {
        public CustomDataMappingRepository(HcaDbContext dbContext) : base(dbContext)
        {
        }

        public async Task<IEnumerable<CustomDataMappingEntity?>> GetCustomDataMappingBySourceSystem(string sourceSystemName)
        {
            var request = await GetAllAsync(f => f.SourceSystemName == sourceSystemName);
            return request;
        }
    }
}