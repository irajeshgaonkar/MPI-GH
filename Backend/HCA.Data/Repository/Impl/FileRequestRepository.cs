using HCA.Data.Entities;
using HCA.Data.Repository.Core;
using HCA.Infrastructure.Logger;
using Microsoft.EntityFrameworkCore;

namespace HCA.Data.Repository;

public class FileRequestRepository( IHcaDbContextAccessor dbContextAccessor, ITenantContext tenantContext, IAppLogger logger ) : RepositoryBase<FileRequestEntity>(dbContextAccessor), IFileRequestRepository
{
    public async Task<FileRequestEntity?> GetRequest(string requestId)
    {
        var request = await GetSingleAsync(f => f.RequestId == requestId);
        return await Task.FromResult(request);
    }

    public async Task<TenantDatabaseKind?> GetTenantDatabaseByRequestId(string requestId)
    {
        if (string.IsNullOrWhiteSpace(requestId))
        {
            logger.LogInformation("Tenant database lookup skipped because requestId is empty.");
            return null;
        }

        logger.LogInformation($"Tenant database lookup started for requestId: {requestId}. Checking coalition database first.");
        var coalitionRequest = await dbContextAccessor.Coalition.FileRequests
            .AsNoTracking()
            .FirstOrDefaultAsync(f => f.RequestId == requestId);

        if (coalitionRequest != null)
        {
            var tenantDatabase = ParseTenantDatabase(coalitionRequest.TenantDatabase, TenantDatabaseKind.Coalition);
            logger.LogInformation($"Tenant database lookup found requestId: {requestId} in coalition database. Stored tenant value: '{coalitionRequest.TenantDatabase}', resolved tenant: '{tenantDatabase.ToTenantValue()}'.");
            return tenantDatabase;
        }

        logger.LogInformation($"Tenant database lookup did not find requestId: {requestId} in coalition database. Non-coalition database configured: {dbContextAccessor.HasNonCoalitionDatabase}.");
        if (!dbContextAccessor.HasNonCoalitionDatabase)
        {
            logger.LogInformation($"Tenant database lookup cannot check non-coalition database for requestId: {requestId} because NonCoalitionDbConnectionStr is missing.");
            return null;
        }

        logger.LogInformation($"Tenant database lookup checking non-coalition database for requestId: {requestId}.");
        var nonCoalitionRequest = await dbContextAccessor.NonCoalition.FileRequests
            .AsNoTracking()
            .FirstOrDefaultAsync(f => f.RequestId == requestId);

        if (nonCoalitionRequest == null)
        {
            logger.LogInformation($"Tenant database lookup did not find requestId: {requestId} in non-coalition database.");
            return null;
        }

        var nonCoalitionTenantDatabase = ParseTenantDatabase(nonCoalitionRequest.TenantDatabase, TenantDatabaseKind.NonCoalition);
        logger.LogInformation($"Tenant database lookup found requestId: {requestId} in non-coalition database. Stored tenant value: '{nonCoalitionRequest.TenantDatabase}', resolved tenant: '{nonCoalitionTenantDatabase.ToTenantValue()}'.");
        return nonCoalitionTenantDatabase;
    }

    private static TenantDatabaseKind ParseTenantDatabase(string? value, TenantDatabaseKind fallback)
        => TenantDatabaseKindExtensions.TryParseTenantValue(value, out var tenantDatabase)
            ? tenantDatabase
            : fallback;

    public override void Add(FileRequestEntity entity)
    {
        entity.TenantDatabase = tenantContext.CurrentTenantDatabase.ToTenantValue();
        base.Add(entity);
    }

    public override void AddAsync(FileRequestEntity entity)
    {
        entity.TenantDatabase = tenantContext.CurrentTenantDatabase.ToTenantValue();
        base.AddAsync(entity);
    }
}
