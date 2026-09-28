using HCA.Models.Request;

namespace HCA.Core.Services
{
    public interface IServiceAccountService
    {
        Task<ServiceAccount?> GetServiceAccount(string appId);
        Task<IEnumerable<ServiceAccount?>> GetServiceAccounts();
    }
}
