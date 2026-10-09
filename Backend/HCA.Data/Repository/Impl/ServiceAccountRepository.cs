using HCA.Data.Entities;
using HCA.Data.Repository.Core;

namespace HCA.Data.Repository.Impl
{
    public class ServiceAccountRepository :  RepositoryBase<ServiceAccountEntity>, IServiceAccountRepository
    {
        public ServiceAccountRepository(HcaDbContext dbContext) : base(dbContext)
        {
        }
        public async Task<ServiceAccountEntity?> GetServiceAccount(string appId)
        {
            var request = await GetSingleAsync(f => f.AppId == appId);
            return await Task.FromResult(request);
        }

        public async Task<IEnumerable<ServiceAccountEntity?>> GetServiceAccounts()
        {
            var request = await GetAllAsync();
            return await Task.FromResult(request);
        }
    }
}
