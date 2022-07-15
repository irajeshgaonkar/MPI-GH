using HCA.Data.Entities;

namespace HCA.Data.Repository;

public interface IClientIdentityRepository
{
    Task Upsert(ClientIdentityEntity clientIdentity);

    Task<IEnumerable<ClientIdentityEntity>> GetAll(int skip, int take);

    Task<IEnumerable<ClientIdentityEntity?>> GetBySourceAndId(string sourceSystemName, string sourceSystemId, string? mpiLinkId = null);

    Task Update(ClientIdentityEntity clientIdentity);

    Task UpdateMpiLinkId(string sourceSystemName, string sourceSystemId, string newMpiLinkId);

    Task<int> GetCount();
}
