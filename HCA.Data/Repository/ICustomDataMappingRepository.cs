using HCA.Data.Entities;
using HCA.Data.Repository.Core;

namespace HCA.Data.Repository
{
    public interface ICustomDataMappingRepository : IRepositoryBase<CustomDataMappingEntity>
    {
        Task<IEnumerable<CustomDataMappingEntity>> GetCustomDataMappingBySourceSystem(string sourceSystemName);
    }
}
