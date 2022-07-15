using HCA.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace HCA.Data.Repository;

public class FileRequestRepository : IFileRequestRepository
{
    private readonly HcaDbContext _dbContext;
    private readonly IDataAdapter _dataAdapter;

    public FileRequestRepository(HcaDbContext dbContext, IDataAdapter dataAdapter)
    {
        _dbContext = dbContext;
        _dataAdapter = dataAdapter;
    }

    public async Task<FileRequestEntity?> GetRequest(Guid requestId)
    {
        var requests = _dbContext.FileRequests.SingleOrDefault(c => c.RequestId == requestId);
        return await Task.FromResult(requests);
    }

    public async Task<FileRequestEntity> Insert(FileRequestEntity entity)
    {
        _dbContext.FileRequests.Add(entity);
        await _dbContext.SaveChangesAsync();
        return entity;
    }

    public async Task<FileRequestEntity> Update(FileRequestEntity entity)
    {
        _dbContext.Entry(entity).State = EntityState.Modified;
        await _dbContext.SaveChangesAsync();
        return entity;
    }
}