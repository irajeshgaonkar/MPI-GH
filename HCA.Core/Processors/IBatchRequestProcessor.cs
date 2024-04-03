using HCA.Models.SQS;

namespace HCA.Core.Processors;

public interface IBatchRequestProcessor
{
    Task ProcessRequest(BatchProcessMessage batchRequest);
}

public interface IUserRequestProcessor
{
    Task ProcessRequest(UserRequestMessage requestMessage);
}
