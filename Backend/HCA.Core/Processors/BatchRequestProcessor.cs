using HCA.Core.Processors.File;
using HCA.Core.Services;
using HCA.Data.Repository;
using HCA.Infrastructure.Extensions;
using HCA.Infrastructure.Extensions.ModelExtensions;
using HCA.Infrastructure.Logger;
using HCA.Models.Enums;
using HCA.Models.Request;
using HCA.Models.Response;
using HCA.Models.SQS;
using HCA.Infrastructure.Sqs;
using HCA.Models.Logging;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace HCA.Core.Processors;

public class BatchRequestProcessor : IBatchRequestProcessor
{
    private readonly IAppLogger _logger;
    private readonly IClientIdentityRequestRepository _clientIdentityRequestRepository;
    private readonly IClientIdentityRequestExecutor _clientIdentityRequestExecutor;
    private readonly IRequestProcessLogRepository _requestProcessLogRepository;
    private readonly IFileRequestRepository _fileRequestRepository;
    private readonly ISqsPublisher _sqsPublisher;
    private readonly IOutputFileWriter _outputFileWriter;


    public BatchRequestProcessor(IAppLogger logger,
        IClientIdentityRequestRepository clientIdentityRequestRepository,
        IClientIdentityRequestExecutor clientIdentityRequestExecutor,
        IRequestProcessLogRepository requestProcessLogRepository,
        IFileRequestRepository fileRequestRepository,
        IOutputFileWriter outputFileWriter,
        ISqsPublisher sqsPublisher
        )
    {
        _logger = logger;
        _clientIdentityRequestRepository = clientIdentityRequestRepository;
        _clientIdentityRequestExecutor = clientIdentityRequestExecutor;
        _requestProcessLogRepository = requestProcessLogRepository;
        _fileRequestRepository = fileRequestRepository;
        _sqsPublisher = sqsPublisher;
        _outputFileWriter = outputFileWriter;
    }

    public async Task ProcessRequest(BatchProcessMessage batchRequest)
    {
        try
        {
            var clientIdentityRequest = batchRequest.ClientIdentityRequests.FirstOrDefault();
            var requestId = clientIdentityRequest?.RequestId;
            var batchNumber = clientIdentityRequest?.BatchNumber;
            _logger.LogInformation($"Started Processing the request requestId: {requestId}, BatchNumber: {batchNumber}");

            var fileRequest = await _fileRequestRepository.GetSingleAsync(f => f.RequestId == requestId);

            if (fileRequest?.ApiCallType == "VE Delete")
            {
                await ProcessDeleteIdentityRequest (batchRequest);
                _logger.LogInformation($"Completed Processing the request requestId: {requestId}, BatchNumber: {batchNumber}");
            }
            else
            {
                await ProcessPostIdentityRequest(batchRequest);
                _logger.LogInformation($"Completed Processing the request requestId: {requestId}, BatchNumber: {batchNumber}");
            }


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

    private async Task ProcessDeleteIdentityRequest(BatchProcessMessage batchRequest)
    {
        //int maxDegreeOfParallelism = 1;
        var groupedRequests = GetGroupedRequests(batchRequest);
        if (groupedRequests == null) return;
        //await groupedRequests.ParallelForEachAsync((requests) => ProcessPostIdentityRequests(requests), maxDegreeOfParallelism);
        foreach (var groupedRequest in groupedRequests)
        {
            await ProcessDeleteIdentityRequests(groupedRequest);
        }
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

    private async Task ProcessDeleteIdentityRequests(IEnumerable<ClientIdentityRequest> requests)
    {
        var firstRequest = requests.First();
        var trackingId = ClientIdentityRequestExtension.GetTrackingId(firstRequest.SourceSystemName, firstRequest.SourceSystemId);

        try
        {
            _logger.LogInformation($"Processing request TrackingId: {trackingId}");
            var requestStatusUpdater = new ClientIdentityRequestStatusUpdater(_clientIdentityRequestRepository, _requestProcessLogRepository);

            if (!requests.Any())
                return;

            await Update(requests, trackingId, RequestStatus.Processing, "Processing", null);

            var request = requests.First();
            var deleteIdentityRequest = new DeleteClientIdentityRequest(trackingId)
            {
                Content = new Models.Verato.Source(request.SourceSystemName, request.SourceSystemId)
            };

            var response = await _clientIdentityRequestExecutor.Execute<DeleteClientIdentityResponse>(deleteIdentityRequest, requestStatusUpdater);

            if (null == response || response.Success == false)
            {
                await Update(requests, trackingId, RequestStatus.Failed, response?.Errors.JoinBy("|") ?? "", null);
                return;
            }
            var linkIdsDeleted = response.Content.LinkIdsDeleted.Aggregate(string.Empty, (s1, s2) => $"{s1} {s2}");
            await Update(requests, trackingId, RequestStatus.Success, "", linkIdsDeleted);
        }
        catch (Exception e)
        {
            await Update(requests, trackingId, RequestStatus.Failed, e.Message, null);

            var exceptionCustomProperties = new ExceptionCustomProperties
            {
                User = "BatchProcess",
                Agency = firstRequest.SourceSystemAgency,
                FunctionName = nameof(ProcessDeleteIdentityRequests),
                ErrorMessage = e.Message,
                StackTrace = e.StackTrace,
                ErrorCode = "500"
            };
            var errorLogItem = new LogItem()
            {
                Name = $"{Models.Logging.Constants.LogPrefix_Batch}{nameof(ProcessDeleteIdentityRequests)}-Failed",
                TrackingId = string.IsNullOrEmpty(firstRequest.TrackingId) ? trackingId : firstRequest.TrackingId,
                Layer = ServiceLayer.Batch.ToString(),
                ExceptionCustomProperties = exceptionCustomProperties
            };

            _logger.LogError(e, JsonConvert.SerializeObject(errorLogItem));
            //Log error and move on
            //Do not throw as we need to process the remaining items in the request
        }
    }

    private async Task ProcessPostIdentityRequests(IEnumerable<ClientIdentityRequest> requests)
    {
        var firstRequest = requests.First();
        var trackingId = ClientIdentityRequestExtension.GetTrackingId(firstRequest.SourceSystemName, firstRequest.SourceSystemId);

        try
        {
            _logger.LogInformation($"Processing request TrackingId: {trackingId}");
            var requestStatusUpdater = new ClientIdentityRequestStatusUpdater(_clientIdentityRequestRepository, _requestProcessLogRepository);
            requests = await RemoveDuplicates(requests);
            _logger.LogInformation($"Completed removing duplicates: {trackingId}");

            if (!requests.Any())
                return;

            await Update(requests, trackingId, RequestStatus.Processing, "Processing", null);

            var postIdentityRequest = new PostClientIdentityRequest(trackingId)
            {
                Content = requests.ToList()
            };

            var response = await _clientIdentityRequestExecutor.Execute<PostClientIdentityResponse>(postIdentityRequest, requestStatusUpdater);

            if (null == response || response.Success == false)
            {
                await Update(requests, trackingId, RequestStatus.Failed, response?.Errors.JoinBy("|") ?? "", null);
                return;
            }

            await Update(requests, trackingId, RequestStatus.Success, "", response.Content.LinkId);
        }
        catch (Exception e)
        {
            await Update(requests, trackingId, RequestStatus.Failed, e.Message, null);

            var exceptionCustomProperties = new ExceptionCustomProperties
            {
                User = "BatchProcess",
                Agency = firstRequest.SourceSystemAgency,
                FunctionName = nameof(ProcessPostIdentityRequests),
                ErrorMessage = e.Message,
                StackTrace = e.StackTrace,
                ErrorCode = "500"
            };
            var errorLogItem = new LogItem()
            {
                Name = $"{Models.Logging.Constants.LogPrefix_Batch}{nameof(ProcessPostIdentityRequests)}-Failed",
                TrackingId = string.IsNullOrEmpty(firstRequest.TrackingId) ? trackingId : firstRequest.TrackingId,
                Layer = ServiceLayer.Batch.ToString(),
                ExceptionCustomProperties = exceptionCustomProperties
            };

            _logger.LogError(e, JsonConvert.SerializeObject(errorLogItem));
            //Log error and move on
            //Do not throw as we need to process the remaining items in the request
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
