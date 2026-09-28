using HCA.Data.Entities;
using HCA.Data.Repository.Core;

namespace HCA.Data.Repository;

public class RequestProcessLogRepository : RepositoryBase<RequestProcessLogEntity>, IRequestProcessLogRepository
{
    public RequestProcessLogRepository(HcaDbContext dbContex) : base(dbContex)
    {
    }
}



