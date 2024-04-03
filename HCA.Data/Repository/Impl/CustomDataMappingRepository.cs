using HCA.Data.Entities;
using HCA.Data.Repository.Core;

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