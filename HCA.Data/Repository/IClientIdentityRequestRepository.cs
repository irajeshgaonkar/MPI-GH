using HCA.Data.Entities;

namespace HCA.Data.Repository;

public interface IClientIdentityRequestRepository
{
    Task<IEnumerable<ClientIdentityRequestEntity>> GetRequests(Guid requestId);

    Task InsertBulk(IEnumerable<ClientIdentityRequestEntity> entities);

    Task UpdateReqeust(ClientIdentityRequestEntity requestEntity);

    Task Update(IEnumerable<ClientIdentityRequestEntity> entities);

    Task UpdateStatus(Guid requestId, IEnumerable<string?> trackingIds, string? status, string? message, string? mpiLinkId);
}



