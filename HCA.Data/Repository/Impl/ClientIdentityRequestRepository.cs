using System;
using System.Linq.Expressions;
using HCA.Data.Entities;
using HCA.Data.Repository.Core;
using HCA.Models;
using Microsoft.EntityFrameworkCore;

namespace HCA.Data.Repository;

public class ClientIdentityRequestRepository : RepositoryBase<ClientIdentityRequestEntity>, IClientIdentityRequestRepository
{
    public ClientIdentityRequestRepository(HcaDbContext dbContext) : base(dbContext)
    {
    }

    public async Task<IEnumerable<ClientIdentityRequestEntity>> GetRequests(string requestId, string? status = null)
    {
        if (null != status)
        {
            var requests = await GetAllAsync(c => c.RequestId == requestId && c.Status == status);
            return requests;
        }

        var result = await GetAllAsync(c => c.RequestId == requestId);
        return result;
    }

    public async Task<IEnumerable<ClientIdentityRequestEntity>> GetRequests(string requestId, int batchNumber)
    {
        var requests = await GetAllAsync(c => c.RequestId == requestId && c.BatchNumber == batchNumber);
        return requests;
    }

    public async Task UpdateStatus(List<int> ids, string? status, string? message, string? mpiLinkId, string? trackingId = null)
    {
        foreach (var id in ids)
        {
            var clientIdentityRequest = await GetSingleAsync(c => c.Id == id);
            if (null == clientIdentityRequest) continue;

            clientIdentityRequest.Status = status ?? clientIdentityRequest.Status;
            clientIdentityRequest.Message = message ?? clientIdentityRequest.Message;
            clientIdentityRequest.MpiLinkId = mpiLinkId ?? clientIdentityRequest.MpiLinkId;
            clientIdentityRequest.TrackingId = trackingId ?? clientIdentityRequest.TrackingId;
            Update(clientIdentityRequest);
        }
    }
}

