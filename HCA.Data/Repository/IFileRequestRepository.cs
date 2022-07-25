using HCA.Data.Entities;

namespace HCA.Data.Repository;

public interface IFileRequestRepository
{
    Task<FileRequestEntity> Insert(FileRequestEntity entity);

    Task<FileRequestEntity?> GetRequest(int id);

    Task<FileRequestEntity> Update(FileRequestEntity entity);
}
