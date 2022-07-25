using HCA.Data.Entities;

namespace HCA.Data.Repository;

public interface IUserRequestRepository
{
    Task<UserRequestEntity> Insert(UserRequestEntity entity);

    Task<UserRequestEntity?> GetRequest(string trackingId);

    Task<UserRequestEntity> Update(UserRequestEntity entity);
}

