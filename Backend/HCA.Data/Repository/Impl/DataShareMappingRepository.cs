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

        public async Task<List<string>> GetActiveAllowedSystemNamesByLevelAsync(string sourceSystem, string dataSharingLevel)
        {
            var caller = sourceSystem.ToLower();

            // data_sharing_level is a Postgres enum in some environments; lower() is not valid on enums.
            // Compare the level with the exact stored value (e.g. DataSharingLevel.LinkId -> "LinkId").
            return await _hcaDbContext.DataShareMappings
                .Where(mapping => mapping.IsActive
                    && mapping.SourceSystem.SourceSystemName.ToLower() == caller
                    && mapping.DataSharingLevel == dataSharingLevel)
                .Select(mapping => mapping.AllowedSystem.SourceSystemName)
                .ToListAsync();
        }
    }
}
