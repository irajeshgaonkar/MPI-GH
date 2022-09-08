using HCA.Data.Entities;
using HCA.Data.Repository.Core;
namespace HCA.Data.Repository;

public class UserRequestRepository : RepositoryBase<UserRequestEntity>, IUserRequestRepository
{
    public UserRequestRepository(HcaDbContext dbContext) : base(dbContext)
    {
    }
}