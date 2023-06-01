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
        Task<CustomDataMapping?> GetCustomDataMapping( string sourceSystemName);
        Task<CustomDataMapping?> AddCustomDataMapping(CustomDataMapping fileResponse);
        Task<IEnumerable<CustomDataMapping?>> GetCustomDataMappings();
        Task<CustomDataMapping?> UpdateCustomDataMapping(CustomDataMapping fileResponse);
        Task RemoveCustomDataMapping(int id);
    }
}
