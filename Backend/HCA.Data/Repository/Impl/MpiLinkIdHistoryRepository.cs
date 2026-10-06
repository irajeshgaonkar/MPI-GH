using HCA.Data.Entities;
using HCA.Data.Repository.Core;
namespace HCA.Data.Repository;

public class MpiLinkIdHistoryRepository : RepositoryBase<MpiLinkIdHistoryEntity>, IMpiLinkIdHistoryRepository
{
    public MpiLinkIdHistoryRepository(IHcaDbContextAccessor dbContextAccessor) : base(dbContextAccessor)
    {
    }
}
