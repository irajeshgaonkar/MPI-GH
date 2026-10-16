using HCA.Models;

namespace HCA.Data.Repository
{
    public interface IDataShareMappingRepository
    {
        Task<List<DataShareMapping>> GetAllowedDataShareMappingForSourceSystemAsync(string sourceSystem);

        Task<List<string>> GetActiveAllowedSystemNamesByLevelAsync(string sourceSystem, string dataSharingLevel);
    }
}
