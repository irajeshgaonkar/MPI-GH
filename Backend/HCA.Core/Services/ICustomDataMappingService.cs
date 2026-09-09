using HCA.Models.Request;
using Newtonsoft.Json.Linq;

namespace HCA.Core.Services
{
    public interface ICustomDataMappingService
    {
        Task<IEnumerable<CustomDataMapping?>> GetCustomDataMappingBySourceSystem(string sourceSystemName);
        Task<IEnumerable<CustomDataMapping?>> GetCustomDataMappings();
        Task<CustomDataMapping?> AddCustomDataMapping(CustomDataMapping fileResponse);
        Task<CustomDataMapping?> UpdateCustomDataMapping(CustomDataMapping fileResponse);
        Task RemoveCustomDataMapping(int id);

        JObject MapCustomJson(IEnumerable<CustomDataMapping?> customDataMappings, Dictionary<string, object> customData);
    }
}
