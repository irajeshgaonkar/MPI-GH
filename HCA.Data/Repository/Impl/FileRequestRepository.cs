using HCA.Data.Entities;
using HCA.Data.Repository.Core;
using Microsoft.EntityFrameworkCore;

namespace HCA.Data.Repository;

public class FileRequestRepository : RepositoryBase<FileRequestEntity>, IFileRequestRepository
{
    public FileRequestRepository(HcaDbContext dbContext) : base(dbContext)
    {
    }

    public async Task<FileRequestEntity?> GetRequest(string requestId)
    {
        var request = await GetSingleAsync(f => f.RequestId == requestId);
        return await Task.FromResult(request);
    }
}