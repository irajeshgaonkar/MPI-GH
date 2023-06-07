using HCA.Data.Entities;
using HCA.Models.Request;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HCA.Core.Services
{
    public interface ICustomDataMappingService
    {
        Task<IEnumerable<CustomDataMapping?>> GetCustomDataMappingBySourceSystem(string sourceSystemName);
        Task<IEnumerable<CustomDataMapping?>> GetCustomDataMappings();
        Task<CustomDataMapping?> AddCustomDataMapping(CustomDataMapping fileResponse);
        Task<CustomDataMapping?> UpdateCustomDataMapping(CustomDataMapping fileResponse);
        Task RemoveCustomDataMapping(int id);
    }
}
