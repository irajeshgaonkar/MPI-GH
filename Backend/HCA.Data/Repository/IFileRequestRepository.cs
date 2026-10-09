using HCA.Data.Entities;
using HCA.Data.Repository.Core;

namespace HCA.Data.Repository;

public interface IFileRequestRepository : IRepositoryBase<FileRequestEntity>
{
    Task<FileRequestEntity?> GetRequest(string requestId);
}
