using HCA.Core.Mapper;
using HCA.Core.Processors.File;
using HCA.Core.Services;
using HCA.Data.Entities;
using HCA.Data.Repository;
using HCA.Infrastructure.Exceptions;
using HCA.Infrastructure.Extensions;
using HCA.Infrastructure.Extensions.ModelExtensions;
using HCA.Infrastructure.Logger;
using HCA.Infrastructure.Sqs;
using HCA.Models;
using HCA.Models.Enums;
using HCA.Models.MuleSoft;
using HCA.Models.MuleSoft.Response;
using HCA.Models.Request;
using HCA.Models.Response;
using HCA.Models.SQS;
using Amazon.S3;

namespace HCA.Core.Processors;

public class NotificationDetails
{
    public Dictionary<string, string> Notifiers { get; set; }
}

public class UserRequestProcessor : IUserRequestProcessor
{
    private readonly NotificationDetails _notificationOptions;

    private readonly IUserRequestRepository _userRequestRepository;

    private readonly IRequestProcessLogRepository _requestProcessLogRepository;

    private readonly IClientIdentityRequestExecutor _clientIdentityRequestExecutor;

    public async Task ProcessRequest(UserRequestMessage requestMessage)
    {
        var userRequest = requestMessage.UserRequest;
        var userRequestEntity = await _userRequestRepository.GetSingleAsync(ur => ur.Id == userRequest.Id);
        if (userRequestEntity == null) return;

        var apiCallType = Enum.Parse<ApiCallType>(userRequestEntity.ApiCallType);

        switch (apiCallType)
        {
            case ApiCallType.VELink:
                var linkingSources = SerializationExtensions.DeSerialize<LinkingSources>(userRequestEntity.RequestJson);
                if (linkingSources == null) throw new ArgumentException("UserRequestProcessor:ProcessRequest: linkingSources cannot be null");
                var linkIdentitiesResponse = await LinkIdentities(userRequestEntity, linkingSources);
                await NotifyResult(userRequestEntity, linkIdentitiesResponse);
                break;
            case ApiCallType.VEUnLink:
                var unLinkingSources = SerializationExtensions.DeSerialize<UnLinkingSources>(userRequestEntity.RequestJson);
                if (unLinkingSources == null) throw new ArgumentException("UserRequestProcessor:ProcessRequest: unLinkingSources cannot be null");
                var unlinkIdentitiesResponse = await UnLinkIdentities(userRequestEntity, unLinkingSources);
                await NotifyResult(userRequestEntity, unlinkIdentitiesResponse);
                break;
            case ApiCallType.VEMerge:
                var mergingSources = SerializationExtensions.DeSerialize<MergingSources>(userRequestEntity.RequestJson);
                if (mergingSources == null) throw new ArgumentException("UserRequestProcessor:ProcessRequest: mergingSources cannot be null");
                var mergeIdentitiesResponse = await MergeIdentities(userRequestEntity, mergingSources);
                await NotifyResult(userRequestEntity, mergeIdentitiesResponse);
                break;
            case ApiCallType.VEUnMerge:
                var unMergingSources = SerializationExtensions.DeSerialize<UnMergingSources>(userRequestEntity.RequestJson);
                if (unMergingSources == null) throw new ArgumentException("UserRequestProcessor:ProcessRequest: unMergingSources cannot be null");
                var unMergeIdentitiesResponse = await UnMergeIdentities(userRequestEntity, unMergingSources);
                await NotifyResult(userRequestEntity, unMergeIdentitiesResponse);
                break;
            case ApiCallType.VEDelete:
                break;
            case ApiCallType.VEDemographicSearch:
                var searchFilter = SerializationExtensions.DeSerialize<Identity>(userRequestEntity.RequestJson);
                if (searchFilter == null) throw new ArgumentException("UserRequestProcessor:ProcessRequest: unMergingSources cannot be null");
                var demographicSearchResponse = await DemographicSearch(userRequestEntity, searchFilter);
                await NotifyResult(userRequestEntity, demographicSearchResponse);
                break;
            case ApiCallType.VEPost:
                break;
            default:
                break;
        }
    }

    private async Task NotifyResult<T>(UserRequestEntity userRequest, T? message)
    {
        if (userRequest.NotificationOptions == null || message == null) return;
        var notificationDetails = SerializationExtensions.DeSerialize<NotificationOptions>(userRequest.NotificationOptions);
        if (notificationDetails == null) return;

        var queueUrlKey = $"{notificationDetails.AppName}-{notificationDetails.MessageGroup}-queueUrl";
        var messagegroupIdKey = $"{notificationDetails.AppName}-{notificationDetails.MessageGroup}-messageId";
        var notifiers = _notificationOptions.Notifiers;

        var queueUrl = notifiers.ContainsKey(queueUrlKey) ? notifiers[queueUrlKey] : string.Empty;
        var messageGroupId = notifiers.ContainsKey(messagegroupIdKey) ? notifiers[messagegroupIdKey] : string.Empty;

        if (queueUrl == string.Empty || messageGroupId == string.Empty) return;

        var sqsOptions = new SqsOptions()
        {
            QueueUrl = queueUrl,
            MessageGroupId = messageGroupId
        };

        var sqsMessage = new SqsMessage()
        {
            MessageType = MessageType.UserRequestResponse,
            Payload = SerializationExtensions.Serialize(message)
        };

        var sqsPublisher = new SqsPublisher(sqsOptions);
        await sqsPublisher.PublishMessage(sqsMessage);
    }


    private async Task<LinkIdentitiesResponseContent?> LinkIdentities(UserRequestEntity userRequestEntity, LinkingSources linkingSources)
    {
        var requestStatusUpdater = new UserRequestStatusUpdater(_userRequestRepository, _requestProcessLogRepository);
        var linkIdentityRequest = new LinkClientIdentityRequest(userRequestEntity.TrackingId)
        {
            Content = linkingSources
        };

        var response = await _clientIdentityRequestExecutor.Execute<LinkClientIdentityResponse>(linkIdentityRequest, requestStatusUpdater);
        UpdateProcessStatus(userRequestEntity, RequestStatus.Success, "Request Processed Successfully");
        return response?.Content;
    }

    private async Task<List<PostIdentityResponseContent>?> DemographicSearch(UserRequestEntity userRequestEntity, Identity filter)
    {
        var requestStatusUpdater = new UserRequestStatusUpdater(_userRequestRepository, _requestProcessLogRepository);
        var demographicSearhRequest = new DemographicSearchClientIdentityRequest(userRequestEntity.TrackingId)
        {
            Content = filter
        };

        var response = await _clientIdentityRequestExecutor.Execute<DemographicSearchClientIdentityResponse>(demographicSearhRequest, requestStatusUpdater);
        UpdateProcessStatus(userRequestEntity, RequestStatus.Success, "Request Processed Successfully");
        return response?.Content;
    }

    private async Task<UnLinkIdentitiesResponseContent?> UnLinkIdentities(UserRequestEntity userRequestEntity, UnLinkingSources unLinkingSources)
    {
        var requestStatusUpdater = new UserRequestStatusUpdater(_userRequestRepository, _requestProcessLogRepository);
        var unLinkClientIdentityRequest = new UnLinkClientIdentityRequest(userRequestEntity.TrackingId)
        {
            Content = unLinkingSources
        };

        var response = await _clientIdentityRequestExecutor.Execute<UnLinkClientIdentityResponse>(unLinkClientIdentityRequest, requestStatusUpdater);
        UpdateProcessStatus(userRequestEntity, RequestStatus.Success, "Request Processed Successfully");
        return response?.Content;
    }

    private async Task<MergeIdentitiesResponseContent?> MergeIdentities(UserRequestEntity userRequestEntity, MergingSources mergingSources)
    {
        var requestStatusUpdater = new UserRequestStatusUpdater(_userRequestRepository, _requestProcessLogRepository);

        var mergeClientIdentityRequest = new MergeClientIdentityRequest(userRequestEntity.TrackingId)
        {
            Content = mergingSources
        };
        var response = await _clientIdentityRequestExecutor.Execute<MergeClientIdentityResponse>(mergeClientIdentityRequest, requestStatusUpdater);
        UpdateProcessStatus(userRequestEntity, RequestStatus.Success, "Request Processed Successfully");
        return response?.Content;
    }

    private async Task<UnMergeIdentitiesResponseContent?> UnMergeIdentities(UserRequestEntity userRequestEntity, UnMergingSources unMergingSources)
    {
        var requestStatusUpdater = new UserRequestStatusUpdater(_userRequestRepository, _requestProcessLogRepository);

        var unMergeClientIdentityRequest = new UnMergeClientIdentityRequest(userRequestEntity.TrackingId)
        {
            Content = unMergingSources
        };
        var response = await _clientIdentityRequestExecutor.Execute<UnMergeClientIdentityResponse>(unMergeClientIdentityRequest, requestStatusUpdater);
        UpdateProcessStatus(userRequestEntity, RequestStatus.Success, "Request Processed Successfully");
        return response?.Content;
    }

    private void UpdateProcessStatus(UserRequestEntity userRequest, RequestStatus status, string message)
    {
        userRequest.Status = status.GetStringValue();
        userRequest.Message = message;
        userRequest.ProcessEndTime = DateTime.Now;
        _userRequestRepository.Update(userRequest);
    }
}

public class BatchRequestProcessor : IBatchRequestProcessor
{
    private readonly IAppLogger _logger;
    private readonly IClientIdentityRequestRepository _clientIdentityRequestRepository;
    private readonly IClientIdentityRequestExecutor _clientIdentityRequestExecutor;
    private readonly IRequestProcessLogRepository _requestProcessLogRepository;
    private readonly IFileWriter _fileWriter;
    private readonly IFileRequestService _fileRequestService;
    private readonly S3Options _s3Options;

    public BatchRequestProcessor(IAppLogger logger,
        IClientIdentityRequestRepository clientIdentityRequestRepository,
        IClientIdentityRequestExecutor clientIdentityRequestExecutor,
        IRequestProcessLogRepository requestProcessLogRepository,
        IFileWriter fileWriter, IFileRequestService fileRequestService, S3Options s3Options)
    {
        _logger = logger;
        _clientIdentityRequestRepository = clientIdentityRequestRepository;
        _clientIdentityRequestExecutor = clientIdentityRequestExecutor;
        _requestProcessLogRepository = requestProcessLogRepository;
        _fileWriter = fileWriter;
        _fileRequestService = fileRequestService;
        _s3Options = s3Options;
    }

    public async Task ProcessRequest(BatchProcessMessage batchRequest)
    {
        try
        {
            await ProcessPostIdentityRequest(batchRequest);
            var requestId = batchRequest.ClientIdentityRequests.FirstOrDefault()?.RequestId;

            if(requestId != null && IsFileRequestComplete(requestId))
            {
                var fileRequestEntity = await _fileRequestService.UpdatefileRequestComplete(requestId);

                if (fileRequestEntity != null)
                {
                    var memoryStream = await _fileWriter.WriteFile(fileRequestEntity);
                    await WriteToS3(fileRequestEntity, memoryStream);
                }
            }
        }
        catch(Exception e)
        {
            _logger.LogError(e, "Error Processing the request");
            throw;
        }
    }

    public async Task WriteToS3(FileRequestEntity fileRequest, MemoryStream memoryStream)
    {
        try
        {
            IAmazonS3 s3Client = new AmazonS3Client();
            await s3Client.UploadObjectFromStreamAsync(_s3Options.OutputBucketName, fileRequest.OutputFileName, memoryStream, new Dictionary<string, object>());
            _logger.LogInformation("Successfully wrote file to s3");
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error Processing the request");
            throw;
        }
    }

    private bool IsFileRequestComplete(string requestId)
    {
        var res = _clientIdentityRequestRepository.All(c =>
            c.Status == RequestStatus.Failed.GetStringValue() ||
            c.Status == RequestStatus.Success.GetStringValue());
        return res;
    }

    private async Task ProcessPostIdentityRequest(BatchProcessMessage batchRequest)
    {
        int maxDegreeOfParallelism = 1;
        var groupedRequests = GetGroupedRequests(batchRequest);
        if (groupedRequests == null) return;
        await groupedRequests.ParallelForEachAsync((requests) => ProcessPostIdentityRequests(requests), maxDegreeOfParallelism);
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
}
