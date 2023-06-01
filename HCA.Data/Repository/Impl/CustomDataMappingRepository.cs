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

        public async Task<dynamic?> AddCustomDataMapping(CustomDataMappingEntity fileResponse)
        {
            Add(fileResponse);
            return await Task.FromResult(fileResponse);
        }

        public async Task<CustomDataMappingEntity?> GetCustomDataMapping(string sourceSystemName)
        {
            var request = await GetSingleAsync(f => f.SourceSystemName == sourceSystemName);
            return await Task.FromResult(request);
        }

        public async Task<IEnumerable<CustomDataMappingEntity?>> GetCustomDataMappings()
        {
            var request = await GetAllAsync();
            return await Task.FromResult(request);
        }

        public Task<dynamic?> RemoveCustomDataMapping(int id)
        {
            throw new NotImplementedException();
        }

        public async Task<dynamic?> UpdateCustomDataMapping(CustomDataMappingEntity fileResponse)
        {
            Update(fileResponse);
            return await Task.FromResult(fileResponse);
        }
    }
}