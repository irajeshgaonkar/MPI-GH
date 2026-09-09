using HCA.Data.Entities;
using HCA.Data.Repository.Core;

namespace HCA.Data.Repository
{
    public interface IOnboardedSystemRepository : IRepositoryBase<OnboardedSystemEntity>
    {
        Task<List<string>> GetActiveSourceSystemsByIPAsync(string incomingIpAddress);

        Task<Dictionary<string, string>> GetTenantMapBySourceSystemsAsync(IEnumerable<string> sourceSystemNames);

    }
}
