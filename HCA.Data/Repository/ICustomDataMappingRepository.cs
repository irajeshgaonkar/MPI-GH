using HCA.Data.Entities;
using HCA.Data.Repository.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HCA.Data.Repository
{
    public interface ICustomDataMappingRepository : IRepositoryBase<CustomDataMappingEntity>
    {
        Task<IEnumerable<CustomDataMappingEntity?>> GetCustomDataMappings();
        Task<CustomDataMappingEntity?> GetCustomDataMapping(int id);
        Task<dynamic?> AddCustomDataMapping(CustomDataMappingEntity customData);
        Task<dynamic?> UpdateCustomDataMapping(CustomDataMappingEntity customData);
        Task<dynamic?> RemoveCustomDataMapping(int id);
    }
}
