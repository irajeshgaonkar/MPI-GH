using HCA.Data.Repository;
using HCA.Infrastructure.Logger;

namespace HCA.Core.Processors;

public abstract class RequestProcessorBase
{
    protected string LogTextPrefix;

    protected readonly IAppLogger Logger;

    protected readonly IRequestProcessLogRepository RequestProcessLogRepository;

    protected Guid RequestId;

    protected RequestProcessorBase(IAppLogger logger, IRequestProcessLogRepository requestProcessLogRepository)
    {
        Logger = logger;
        RequestProcessLogRepository = requestProcessLogRepository;
    }

    protected async Task LogInDatabase(string message)
    {
        await RequestProcessLogRepository.LogStatus(RequestId, message);
    }

    protected void LogInformation(string message)
    {
        Logger.LogInformation($"{LogTextPrefix} Request Processor : Request Id {RequestId} => {message}");
    }

    protected async Task ProcessRequest(string operationType)
    {

    }
}
