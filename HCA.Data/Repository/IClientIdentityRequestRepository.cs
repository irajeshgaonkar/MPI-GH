using HCA.Data.Entities;

namespace HCA.Data.Repository;

public interface IClientIdentityRequestRepository
{
    Task<IEnumerable<ClientIdentityRequestEntity>> GetRequests(int id, string? status = null);

    Task InsertBulk(IEnumerable<ClientIdentityRequestEntity> entities);

    Task UpdateRequest(ClientIdentityRequestEntity requestEntity);

    Task Update(IEnumerable<ClientIdentityRequestEntity> entities);

    Task UpdateStatus(int id, IEnumerable<string> trackingIds, string? status, string? message, string? mpiLinkId);

    Task Update(IList<int> ids, string? status, string? message, string? trackingId, string? mpiLinkId);
}



