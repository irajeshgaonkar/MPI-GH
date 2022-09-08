using HCA.Core.Mapper;
using HCA.Data.Entities;
using HCA.Data.Repository;
using HCA.Infrastructure.Extensions;
using HCA.Infrastructure.Logger;
using HCA.Infrastructure.Sqs;
using HCA.Models.Enums;
using HCA.Models.SQS;
using System.Text.Json;

namespace HCA.Core.Processors;

public static class MessageType
{
    public const string BatchProcess = "Batch";

    public const string UserRequest = "UserRequest";

    public const string UserRequestResponse = "UserRequestResponse";
}

public class ClientIdentitySQSPublisher : IClientIdentitySQSPublisher
{
    private readonly IAppLogger _logger;

    private readonly IFileRequestRepository _fileRequestRepository;

    private readonly IClientIdentityRequestRepository _clientIdentityRequestRepository;

    private readonly IRequestProcessLogRepository _requestProcessLogRepository;

    private readonly IClientIdentityRequestMapper _clientIdentityRequestMapper;

    private readonly ISqsPublisher _sqsPublisher;

    public ClientIdentitySQSPublisher(IAppLogger appLogger, IFileRequestRepository 
        fileRequestRepository, IClientIdentityRequestRepository clientIdentityRequestRepository,
        IRequestProcessLogRepository requestProcessLogRepository, IClientIdentityRequestMapper clientIdentityRequestMapper,
        ISqsPublisher sqsPublisher)
    {
        _logger = appLogger;
        _fileRequestRepository = fileRequestRepository;
        _clientIdentityRequestRepository = clientIdentityRequestRepository;
        _requestProcessLogRepository = requestProcessLogRepository;
        _clientIdentityRequestMapper = clientIdentityRequestMapper;
        _sqsPublisher = sqsPublisher;
    }

    public async Task Publish(string requestId)
    {
        var fileRequest = await _fileRequestRepository.GetRequest(requestId);

        if (null == fileRequest)
        {
            _logger.LogInformation("Couldn't able to find the file request details");
            return;
        }

        int i = 1;
        do
        {
            var requestEntities = await _clientIdentityRequestRepository.GetRequests(requestId, i);
            var requests = _clientIdentityRequestMapper.MapToModelCollection(requestEntities);
            if (null == requests || requests.Count() == 0) break;

            var batchProcessMessage = new BatchProcessMessage()
            {
                ApiCallType = fileRequest.ApiCallType,
                ClientIdentityRequests = requests
            };

            var sqsMessage = new SqsMessage()
            {
                MessageType = MessageType.BatchProcess,
                Payload = SerializationExtensions.SerializeWithoutCasing(batchProcessMessage)
            };

            await _sqsPublisher.PublishMessage(sqsMessage);
            UpdateProcessLog(fileRequest.RequestId, $"Posted message on to SQS for RequestId: {requestId} and BatchNumber: {i}");
            i++;
        } while (true);
    }

    private void UpdateProcessLog(string requestId, string message)
    {
        var requestProcessLogEntity = new RequestProcessLogEntity()
        {
            TrackingId = requestId,
            DateTime = DateTime.Now,
            Status = RequestStatus.Processing.GetStringValue(),
            Message = message
        };

        _requestProcessLogRepository.AddAsync(requestProcessLogEntity);
    }
}
