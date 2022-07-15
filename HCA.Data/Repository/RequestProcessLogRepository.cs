using HCA.Data.Entities;

namespace HCA.Data.Repository;

public class RequestProcessLogRepository : IRequestProcessLogRepository
{
    private readonly HcaDbContext _dbContext;
    private readonly IDataAdapter _dataAdapter;

    public RequestProcessLogRepository(HcaDbContext dbContext, IDataAdapter dataAdapter)
    {
        _dbContext = dbContext;
        _dataAdapter = dataAdapter;
    }

    public async Task<RequestProcessLogEntity> LogStatus(Guid requestId, string message)
    {
        var entity = new RequestProcessLogEntity()
        {
            RequestId = requestId,
            Message = message,
            DateTime = DateTime.Now,
        };

        var requests = _dbContext.RequestProcessLogs.Add(entity);
        await _dbContext.SaveChangesAsync();
        return await Task.FromResult(entity);
    }
}



