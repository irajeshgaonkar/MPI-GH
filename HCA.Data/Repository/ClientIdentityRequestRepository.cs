using System;
using HCA.Data.Entities;
using HCA.Models;
using Microsoft.EntityFrameworkCore;

namespace HCA.Data.Repository;

public class ClientIdentityRequestRepository : IClientIdentityRequestRepository
{
    private readonly HcaDbContext _dbContext;
    private readonly IDataAdapter _dataAdapter;

    public ClientIdentityRequestRepository(HcaDbContext dbContext, IDataAdapter dataAdapter)
    {
        _dbContext = dbContext;
        _dataAdapter = dataAdapter;
    }

    public async Task<IEnumerable<ClientIdentityRequestEntity>> GetRequests(Guid requestId) {
        var requests = _dbContext.ClientIdentityRequests
                            .Where(c => c.RequestId == requestId && c.Status == DataConstants.Statuses.NotStarted)
                            .AsEnumerable();
        return await Task.FromResult(requests);
    }

    public async Task InsertBulk(IEnumerable<ClientIdentityRequestEntity> entities)
    {
        foreach (var entity in entities)
        {
            _dbContext.ClientIdentityRequests.Add(entity);
        }

        await _dbContext.SaveChangesAsync();
    }

    public async Task Update(IEnumerable<ClientIdentityRequestEntity> entities)
    {
        foreach(var entity in entities)
        {
            _dbContext.Entry(entity).State = EntityState.Modified;
        }

        await _dbContext.SaveChangesAsync();
    }

    public async Task UpdateReqeust(ClientIdentityRequestEntity requestEntity)
    {
        _dbContext.Entry(requestEntity).State = EntityState.Modified;
        await _dbContext.SaveChangesAsync();
    }

    public async Task UpdateReqeustStatus(string trackingIds, string? status = null, string? message = null, int? retryCount = null)
    {
        var parameters = new Dictionary<string, object?>
        {
            { "p_tracking_ids", trackingIds },
            { "p_status", status },
            { "p_message", message },
            { "p_retry_count", retryCount }
        };

        await _dataAdapter.ExecuteNonQueryAsync("call usp_update_client_identity_request_status (:p_tracking_ids, :p_status, :p_message, :p_retry_count)", parameters);
        await _dbContext.SaveChangesAsync();
    }

    public async Task UpdateStatus(Guid requestId, IEnumerable<string?> trackingIds, string? status, string? message, string? mpiLinkId)
    {
        foreach(var trackingId in trackingIds)
        {
            if (null == trackingId) continue;

            var clientIdentityRequest = _dbContext.ClientIdentityRequests
                                            .SingleOrDefault(c => c.RequestId == requestId &&  c.TrackingId == trackingId);

            if (null == clientIdentityRequest) continue;

            clientIdentityRequest.Status = status ?? clientIdentityRequest.Status;
            clientIdentityRequest.Message = message ?? clientIdentityRequest.Message;
            clientIdentityRequest.MpiLinkId = mpiLinkId ?? clientIdentityRequest.MpiLinkId;
            await UpdateReqeust(clientIdentityRequest);
        }
    }
}

