using HCA.Data.Entities;
using HCA.Data.Repository.Core;

namespace HCA.Data.Repository;

public class UserModifyRecordsRepository : RepositoryBase<UserModifyRecordsEntity>, IUserModifyRecordsRepository
{
    public UserModifyRecordsRepository(IHcaDbContextAccessor dbContextAccessor) : base(dbContextAccessor)
    {
    }
}
