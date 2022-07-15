using HCA.Data.Entities;

namespace HCA.Data.Repository;

public interface IRequestProcessLogRepository
{
    Task<RequestProcessLogEntity> LogStatus(Guid requestId, string message);
}



