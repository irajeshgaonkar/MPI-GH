using HCA.Data.Entities;

namespace HCA.Data.Repository
{
    public interface IServiceAccountRepository
    {
        Task<ServiceAccountEntity?> GetServiceAccount(string appId);
        Task<IEnumerable<ServiceAccountEntity?>> GetServiceAccounts();
    }
}
