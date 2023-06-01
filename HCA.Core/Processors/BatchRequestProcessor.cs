using HCA.Core.Mapper;
using HCA.Core.Processors.File;
using HCA.Core.Services;
using HCA.Data.Entities;
using HCA.Data.Repository;
using HCA.Infrastructure.Exceptions;
using HCA.Infrastructure.Extensions;
using HCA.Infrastructure.Extensions.ModelExtensions;
using HCA.Infrastructure.Logger;
using HCA.Models;
using HCA.Models.Enums;
using HCA.Models.Request;
using HCA.Models.Response;
using HCA.Models.SQS;
using Amazon.S3;
using HCA.Infrastructure.Sqs;

namespace HCA.Core.Processors;

public class BatchRequestProcessor : IBatchRequestProcessor
{
    private readonly IAppLogger _logger;
    private readonly IClientIdentityRequestRepository _clientIdentityRequestRepository;
    private readonly IClientIdentityRequestExecutor _clientIdentityRequestExecutor;
    private readonly IRequestProcessLogRepository _requestProcessLogRepository;
    private readonly ISqsPublisher _sqsPublisher;
    private readonly IOutputFileWriter _outputFileWriter;
    private readonly ICustomDataMappingRepository _customDataMappingRepository;


    public BatchRequestProcessor(IAppLogger logger,
        IClientIdentityRequestRepository clientIdentityRequestRepository,
        IClientIdentityRequestExecutor clientIdentityRequestExecutor,
        IRequestProcessLogRepository requestProcessLogRepository,
        IOutputFileWriter outputFileWriter,
        ISqsPublisher sqsPublisher,
         ICustomDataMappingRepository customDataMappingRepository
        )
    {
        _logger = logger;
        _clientIdentityRequestRepository = clientIdentityRequestRepository;
        _clientIdentityRequestExecutor = clientIdentityRequestExecutor;
        _requestProcessLogRepository = requestProcessLogRepository;
        _sqsPublisher = sqsPublisher;
        _outputFileWriter = outputFileWriter;
        _customDataMappingRepository = customDataMappingRepository;
    }

    public async Task ProcessRequest(BatchProcessMessage batchRequest)
    {
        try
        {
            var clientIdentityRequest = batchRequest.ClientIdentityRequests.FirstOrDefault();
            var requestId = clientIdentityRequest?.RequestId;
            var batchNumber = clientIdentityRequest?.BatchNumber;
            _logger.LogInformation($"Started Processing the request requestId: {requestId}, BatchNumber: {batchNumber}");
            await ProcessPostIdentityRequest(batchRequest);
            _logger.LogInformation($"Completed Processing the request requestId: {requestId}, BatchNumber: {batchNumber}");

            if (requestId != null && IsFileRequestComplete(requestId))
            {
                _logger.LogInformation($"Completed Processing all the requests requestId: {requestId}");
                await PublishOuputFileGenerationMessage(requestId);
            }
        }
        catch(Exception e)
        {
            _logger.LogError(e, "Error Processing the request");
            throw;
        }
    }

    private bool IsFileRequestComplete(string requestId)
    {
        var res = _clientIdentityRequestRepository.All(c => c.RequestId == requestId, c => c.Status == RequestStatus.Failed.GetStringValue() || c.Status == RequestStatus.Success.GetStringValue());
        _logger.LogInformation($"All requests completed for request : {requestId}, {res}");
        return res;
    }

    private async Task ProcessPostIdentityRequest(BatchProcessMessage batchRequest)
    {
        //int maxDegreeOfParallelism = 1;
        var groupedRequests = GetGroupedRequests(batchRequest);
        if (groupedRequests == null) return;
        //await groupedRequests.ParallelForEachAsync((requests) => ProcessPostIdentityRequests(requests), maxDegreeOfParallelism);
        foreach(var groupedRequest in groupedRequests)
        {
            await ProcessPostIdentityRequests(groupedRequest);
        }
    }

    private async Task ProcessPostIdentityRequests(IEnumerable<ClientIdentityRequest> requests)
    {
        var firstRequest = requests.First();
        var trackingId = ClientIdentityRequestExtension.GetTrackingId(firstRequest.SourceSystemName, firstRequest.SourceSystemId);

        _logger.LogInformation($"Processing request TrackingId: {trackingId}");
        var requestStatusUpdater = new ClientIdentityRequestStatusUpdater(_clientIdentityRequestRepository, _requestProcessLogRepository);
        requests = await RemoveDuplicates(requests);
        _logger.LogInformation($"Completed removing duplicates: {trackingId}");

        if (!requests.Any())
            return;
        var request = requests.Take(1);
        var customDataMapping = await _customDataMappingRepository.GetCustomDataMapping("WAHCA.Providerone");

        await Update(requests, trackingId, RequestStatus.Processing, "Processing", null);

        var postIdentityRequest = new PostClientIdentityRequest(trackingId)
        {
            Content = requests.ToList()
        };

        try
        {
            var response = await _clientIdentityRequestExecutor.Execute<PostClientIdentityResponse>(postIdentityRequest, requestStatusUpdater);

            if (null == response || response.Success == false)
            {
                await Update(requests, trackingId, RequestStatus.Failed, response?.Errors.JoinBy("|") ?? "", null);
                return;
            }

            await Update(requests, trackingId, RequestStatus.Success, "", response.Content.LinkId);
        }
        catch (HcaMuleSoftException e)
        {
            await Update(requests, trackingId, RequestStatus.Failed, e.Message, null);
        }
    }

    private async Task Update(IEnumerable<ClientIdentityRequest> requests, string? trackingId, RequestStatus? status, string? message, string? mpiLinkId)
    {
        var ids = requests.Select(r => r.Id).ToList();
        await _clientIdentityRequestRepository.UpdateStatus(ids, status?.GetStringValue(), message, mpiLinkId, trackingId);
        _logger.LogInformation($"Completed updating the status TrackingId: {trackingId}, Status: {status?.GetStringValue()}");
    }

    private IEnumerable<IEnumerable<ClientIdentityRequest>>? GetGroupedRequests(BatchProcessMessage batchRequest)
    {
        var groupedRequests = batchRequest.ClientIdentityRequests.GroupBySourceNameAndId();
        return groupedRequests;
    }

    private async Task<IList<ClientIdentityRequest>> RemoveDuplicates(IEnumerable<ClientIdentityRequest> requests)
    {
        var (records, duplicateRecords) = requests.GetDuplicateRecords();
        if (null == duplicateRecords || duplicateRecords.Count() == 0)
            return records.ToList();

        await Update(duplicateRecords, string.Empty, RequestStatus.Failed, "Duplicate Record", null);
        return records.ToList();
    }

    private async Task PublishOuputFileGenerationMessage(string requestId)
    {
        try
        {
            _logger.LogInformation($"Strated Posting output file generation message to queue");
            var ouputFileGenerationRequest = new OuputFileGenerationMessage() { RequestId = requestId };
            var sqsMessage = new SqsMessage()
            {
                MessageType = MessageType.GenerateOutput,
                Payload = SerializationExtensions.SerializeWithoutCasing(ouputFileGenerationRequest)
            };

            await _sqsPublisher.PublishMessage(sqsMessage);
            _logger.LogInformation($"Completed Posting output file generation message to queue");
        }
        catch(Exception e)
        {
            _logger.LogError(e, "Unable to post message to queue");
            //await _outputFileWriter.WriteFile(requestId);
        }
    }
}
