using HCA.Data.Entities;
using HCA.Data.Repository.Core;

namespace HCA.Data.Repository
{
    public interface IIPConfigRepository : IRepositoryBase<IpAddressesEntity>
    {
        Task<bool> IsIPAddressTrustedAsync(string sourceSystem, string ipAddress);

        Task<string> GetSourceSystemFromIp( string ipAddress );
    }
}
