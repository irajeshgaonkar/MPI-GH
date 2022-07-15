using HCA.Data.Entities;

namespace HCA.Data.Repository;

public interface IUserRepository
{
    Task<UserEntity?> FindByEmail(string email);

    Task<IEnumerable<RoleEntity>> GetRoles();
}

