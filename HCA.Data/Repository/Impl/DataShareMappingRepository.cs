using HCA.Data.Entities;
using HCA.Data.Repository.Core;
using HCA.Models;
using Microsoft.EntityFrameworkCore;

namespace HCA.Data.Repository.Impl
{
    /// <summary>
    /// DataShareMapping repository
    /// </summary>
    /// <param name="dbContext"></param>
    public class DataShareMappingRepository(HcaDbContext dbContext) : RepositoryBase<DataShareMappingEntity>(dbContext), IDataShareMappingRepository
    {
        private readonly HcaDbContext _hcaDbContext = dbContext;

        /// <summary>
        /// Get allowed data share mapping for source system
        /// </summary>
        /// <param name="sourceSystem"></param>
        /// <returns></returns>
        public async Task<List<DataShareMapping>> GetAllowedDataShareMappingForSourceSystemAsync(string sourceSystem)
        {
            var result  = await _hcaDbContext.DataShareMappings.Where(mapping => mapping.SourceSystem.SourceSystemName == sourceSystem)
                .Select(system => new DataShareMapping()
                {
                    DataSharingLevel = system.DataSharingLevel,
                    AllowedSystemName = system.AllowedSystem.SourceSystemName

                }).ToListAsync();

            return result;
        }
    }
}
