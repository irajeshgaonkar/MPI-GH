using HCA.Core.Mapper;
using HCA.Core.Services;
using HCA.Data.Entities;
using HCA.Data.Repository;
using HCA.Infrastructure.Extensions;
using HCA.Infrastructure.Logger;
using HCA.Infrastructure.Sqs;
using HCA.Models.Enums;
using HCA.Models.Request;
using HCA.Models.SQS;

namespace HCA.Core.Processors;

public class ClientIdentitySQSPublisher : IClientIdentitySQSPublisher
{
    private readonly IAppLogger _logger;

    private readonly IFileRequestService _fileRequestService;

    private readonly IClientIdentityRequestRepository _clientIdentityRequestRepository;

    private readonly IRequestProcessLogRepository _requestProcessLogRepository;

    private readonly IClientIdentityRequestMapper _clientIdentityRequestMapper;

    private readonly ISqsPublisher _sqsPublisher;

    public ClientIdentitySQSPublisher(IAppLogger appLogger, IFileRequestService
        fileRequestService, IClientIdentityRequestRepository clientIdentityRequestRepository,
        IRequestProcessLogRepository requestProcessLogRepository, IClientIdentityRequestMapper clientIdentityRequestMapper,
        ISqsPublisher sqsPublisher)
    {
        _logger = appLogger;
        _fileRequestService = fileRequestService;
        _clientIdentityRequestRepository = clientIdentityRequestRepository;
        _requestProcessLogRepository = requestProcessLogRepository;
        _clientIdentityRequestMapper = clientIdentityRequestMapper;
        _sqsPublisher = sqsPublisher;
    }

    public async Task Publish(string requestId)
    {
        var fileRequest = await _fileRequestService.UpdatefileRequestStatus(requestId, RequestStatus.Processing.GetStringValue());

        if (null == fileRequest)
        {
            _logger.LogInformation("Couldn't able to find the file request details");
            return;
        }


        var requestEntities = await _clientIdentityRequestRepository.GetRequests(requestId);
        var allrequests = _clientIdentityRequestMapper.MapToModelCollection(requestEntities);

        var groupedRqeusts = allrequests.GroupBy(g => g.BatchNumber);
        //_logger.LogInformation($"Started Publishing records to SQS for requestId: {requestId}");
        int maxDegreeOfParallelism = 1;
        await groupedRqeusts.ParallelForEachAsync((requests) => PublishMessageToQueue(requests.ToList(), requests.Key, fileRequest.RequestId, fileRequest.ApiCallType), maxDegreeOfParallelism);
        _logger.LogInformation($"Completed Publishing records to SQS for requestId: {requestId}");
    }

    private async Task PublishMessageToQueue(IEnumerable<ClientIdentityRequest> identityRequests, int batchNumber, string requestId, string apiCallType)
    {

        var batchProcessMessage = new BatchProcessMessage()
        {
            ApiCallType = apiCallType,
            ClientIdentityRequests = identityRequests
        };

        var sqsMessage = new SqsMessage()
        {
            MessageType = MessageType.BatchProcess,
            Payload = SerializationExtensions.SerializeWithoutCasing(batchProcessMessage)
        };

        await _sqsPublisher.PublishMessage(sqsMessage);
        //UpdateProcessLog(requestId, $"Posted message on to SQS for RequestId: {requestId} and BatchNumber: {batchNumber}");
    }

    private void UpdateProcessLog(string requestId, string message)
    {
        _logger.LogInformation(message);
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
