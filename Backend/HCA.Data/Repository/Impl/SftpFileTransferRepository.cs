using HCA.Data.Entities;
using HCA.Data.Repository.Core;

namespace HCA.Data.Repository;

public class SftpFileTransferRepository : RepositoryBase<SftpFileTransferEntity>, ISftpFileTransferRepository
{
    public SftpFileTransferRepository(HcaDbContext dbContext) : base(dbContext)
    {
    }

    public async Task<IEnumerable<SftpFileTransferEntity>> GetTransferedFiles(string path)
    {
        var requests = await GetAllAsync(f => f.Path == path);
        return requests;
    }
}