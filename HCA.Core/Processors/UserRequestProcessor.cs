using HCA.Data.Repository;
using HCA.Infrastructure.Logger;

namespace HCA.Core.Processors;


public class UserRequestProcessor : RequestProcessorBase
{
    private readonly IUserRequestRepository userRequestRepository;

    public UserRequestProcessor(ILogger logger, IRequestProcessLogRepository requestProcessLogRepository)
        : base(logger, requestProcessLogRepository)
    {

    }

    public async Task ProcessRequest(Guid requestId)
    {
        var userRequest = await userRequestRepository.GetRequest(requestId);
        if (null == userRequest)
        {
            await LogInDatabase("Couldn't able to find the file request details");
            return;
        }

        await LogInDatabase("Completed fetching request details from databse");
    }
}
