using HCA.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace HCA.Data.Repository;

public class UserRepository : IUserRepository
{
    private readonly HcaDbContext _dbContext;
    private readonly IDataAdapter _dataAdapter;

    public UserRepository(HcaDbContext dbContext, IDataAdapter dataAdapter)
    {
        _dbContext = dbContext;
        _dataAdapter = dataAdapter;
    }

    public async Task<UserEntity?> FindByEmail(string email)
    {
        var user = _dbContext.Users.Include(u => u.UserRoles).SingleOrDefault(t => t.Email == email);
        return await Task.FromResult(user);
    }

    public async Task<IEnumerable<RoleEntity>> GetRoles()
    {
        var roles = _dbContext.Roles.ToList();
        return await Task.FromResult(roles);
    }
}

