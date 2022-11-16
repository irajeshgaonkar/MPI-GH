using HCA.Data.Entities;
using HCA.Data.Repository.Core;

namespace HCA.Data.Repository;

public interface IClientIdentityRequestRepository : IRepositoryBase<ClientIdentityRequestEntity>
{
    Task<IEnumerable<ClientIdentityRequestEntity>> GetRequests(string requestId, string? status = null);

    Task<IEnumerable<ClientIdentityRequestEntity>> GetRequests(string requestId, int batchNumber);

    Task UpdateStatus(List<int> ids, string? status, string? message, string? mpiLinkId, string? trackingId = null);

    void MoveDataToHistoryTable(string requestId);
 }



