using HCA.Core.Mapper;
using HCA.Core.Processors;
using HCA.Data.Entities;
using HCA.Data.Repository;
using HCA.Infrastructure.Exceptions;
using HCA.Infrastructure.Extensions;
using HCA.Infrastructure.Extensions.ModelExtensions;
using HCA.Infrastructure.Sqs;
using HCA.Models;
using HCA.Models.Enums;
using HCA.Models.MuleSoft;
using HCA.Models.MuleSoft.Response;
using HCA.Models.Request;
using HCA.Models.Response;
using HCA.Models.SQS;

namespace HCA.Core.Services;

public class ClientIdentityService : IClientIdentityService
{
    private readonly IUserRequestRepository _userRequestRepository;

    private readonly IUserModifyRecordsService _userModifyRecordsService;

    private readonly IClientIdentityRequestExecutor _clientIdentityRequestExecutor;

    private readonly IClientIdentityRepository _clientIdentityRepository;

    private readonly IUserModifyRecordsRepository _userModifyRecordsRepository;

    private readonly IRequestProcessLogRepository _requestProcessLogRepository;

    private readonly ISqsPublisher _sqsPublisher;

    private readonly IUserRequestMapper _userRequestMapper;

    public ClientIdentityService(IUserRequestRepository userRequestRepository,
        IClientIdentityRequestExecutor clientIdentityRequestExecutor,
        IClientIdentityRepository clientIdentityRepository,
        IUserModifyRecordsRepository userModifyRecordsRepository,
        IRequestProcessLogRepository requestProcessLogRepository,
        IUserModifyRecordsService userModifyRecordsService,
        ISqsPublisher sqsPublisher, IUserRequestMapper userRequestMapper)
    {
        _userRequestRepository = userRequestRepository;
        _clientIdentityRequestExecutor = clientIdentityRequestExecutor;
        _clientIdentityRepository = clientIdentityRepository;
        _userModifyRecordsRepository = userModifyRecordsRepository;
        _requestProcessLogRepository = requestProcessLogRepository;
        _sqsPublisher = sqsPublisher;
        _userRequestMapper = userRequestMapper;
        _userModifyRecordsService = userModifyRecordsService;
    }

    public async Task<(int, IEnumerable<ClientIdentityModel>)> GetAll(string currentUser, Dictionary<string, string> searchFilter, int pageNumber = 0, int recordsPerPage = 10, string orderBy = "")
    {
        //var userModifyRecords = (await _userModifyRecordsRepository.GetAllAsync(r => r.UserName == currentUser)).Select(t => t.ClientIdentityId).ToList();
        var userModifyRecords = new List<int>();
        var (count, entities) = await _clientIdentityRepository.GetAll(searchFilter, userModifyRecords, pageNumber, recordsPerPage, orderBy);
        var models = ClientIdentityMapper.MapToClientIdentityModel(entities);
        return (count, models);
    }

    public async Task<dynamic?> LinkIdentities(LinkingSources linkingSources, string currentUser, ProcessType processType, NotificationOptions? notificationOptions)
    {
        var trackingId = $"{ApiCallType.VEUnLink.GetStringValue()}-{linkingSources.Source.GetTrackingId(linkingSources.LinkToSource)}";
        var userRequestEntity = CreateUserRequest(linkingSources, ApiCallType.VELink, currentUser, trackingId, notificationOptions);

        try
        {
            if (processType == ProcessType.Async)
            {
                await PublishMessageToSqs(ApiCallType.VELink, userRequestEntity);
                return trackingId;
            }

            //await RemoveUserModifyRecords(currentUser, linkingSources.LinkToSource, linkingSources.Source);
            return await LinkIdentities(userRequestEntity, linkingSources);
        }
        catch (HcaBadRequestException e)
        {
            UpdateProcessStatus(userRequestEntity, RequestStatus.Failed, e.Message);
            throw;
        }
        catch (HcaMuleSoftException e)
        {
            UpdateProcessStatus(userRequestEntity, RequestStatus.Failed, e.ToString());
            throw;
        }
    }

    public async Task<dynamic?> UnLinkIdentities(UnLinkingSources unLinkingSources, string currentUser, ProcessType processType, NotificationOptions? notificationOptions)
    {
        var trackingId = $"{ApiCallType.VEUnLink.GetStringValue()}-{unLinkingSources.Source.GetTrackingId(unLinkingSources.UnlinkFromSource)}";
        var userRequestEntity = CreateUserRequest(unLinkingSources, ApiCallType.VEUnLink, currentUser, trackingId, notificationOptions);

        try
        {
            if (processType == ProcessType.Async)
            {
                await PublishMessageToSqs(ApiCallType.VEUnLink, userRequestEntity);
                return trackingId;
            }

            //await RemoveUserModifyRecords(currentUser, unLinkingSources.UnlinkFromSource, unLinkingSources.Source);
            return await UnLinkIdentities(userRequestEntity, unLinkingSources);
        }
        catch (HcaBadRequestException e)
        {
            UpdateProcessStatus(userRequestEntity, RequestStatus.Failed, e.Message);
            throw;
        }
        catch (HcaMuleSoftException e)
        {
            UpdateProcessStatus(userRequestEntity, RequestStatus.Failed, e.ToString());
            throw;
        }
    }

    public async Task<dynamic?> MergeIdentities(MergingSources mergingSources, string currentUser, ProcessType processType, NotificationOptions? notificationOptions)
    {
        var trackingId = $"{ApiCallType.VEMerge.GetStringValue()}-{mergingSources.ToSurviveSource.GetTrackingId(mergingSources.ToRetireSource)}";
        var userRequestEntity = CreateUserRequest(mergingSources, ApiCallType.VEMerge, currentUser, trackingId, notificationOptions);

        try
        {
            if (processType == ProcessType.Async)
            {
                await PublishMessageToSqs(ApiCallType.VEMerge, userRequestEntity);
                return trackingId;
            }

            //await RemoveUserModifyRecords(currentUser, mergingSources.ToSurviveSource, mergingSources.ToRetireSource);
            return await MergeIdentities(userRequestEntity, mergingSources);
        }
        catch (HcaBadRequestException e)
        {
            UpdateProcessStatus(userRequestEntity, RequestStatus.Failed, e.Message);
            throw;
        }
        catch (HcaMuleSoftException e)
        {
            UpdateProcessStatus(userRequestEntity, RequestStatus.Failed, e.ToString());
            throw;
        }
    }

    public async Task<dynamic?> UnMergeIdentities(UnMergingSources unMergingSources, string currentUser, ProcessType processType, NotificationOptions? notificationOptions)
    {
        var trackingId = $"{ApiCallType.VEUnMerge.GetStringValue()}-{unMergingSources.UnmergeSource.GetTrackingId(unMergingSources.UnmergeSource)}";
        var userRequestEntity = CreateUserRequest(unMergingSources, ApiCallType.VEUnMerge, currentUser, trackingId, notificationOptions);

        try
        {
            if (processType == ProcessType.Async)
            {
                await PublishMessageToSqs(ApiCallType.VEUnMerge, userRequestEntity);
                return trackingId;
            }

            //await RemoveUserModifyRecords(currentUser, unMergingSources.UnmergeSource, unMergingSources.UnmergeFromSource);
            return await UnMergeIdentities(userRequestEntity, unMergingSources);
        }
        catch (HcaBadRequestException e)
        {
            UpdateProcessStatus(userRequestEntity, RequestStatus.Failed, e.Message);
            throw;
        }
        catch (HcaMuleSoftException e)
        {
            UpdateProcessStatus(userRequestEntity, RequestStatus.Failed, e.ToString());
            throw;
        }
    }

    public async Task<dynamic?> DemographicSearch(Identity filter, string currentUser, ProcessType processType, NotificationOptions? notificationOptions)
    {
        var trackingId = $"{ApiCallType.VEDemographicSearch.GetStringValue()}-{ClientIdentityRequestExtension.GetTrackingId()}";
        var userRequestEntity = CreateUserRequest(filter, ApiCallType.VEDemographicSearch, currentUser, trackingId, notificationOptions);

        try
        {
            if (processType == ProcessType.Async)
            {
                await PublishMessageToSqs(ApiCallType.VEDemographicSearch, userRequestEntity);
                return trackingId;
            }

            return await DemographicSearch(userRequestEntity, filter);
        }
        catch (HcaMuleSoftException e)
        {
            UpdateProcessStatus(userRequestEntity, RequestStatus.Success, e.ToString());
            throw;
        }
    }

    public async Task<dynamic?> DemographicQuery(Identity filter, string currentUser, ProcessType processType, NotificationOptions? notificationOptions)
    {
        var trackingId = $"{ApiCallType.VEDemographicQuery.GetStringValue()}-{ClientIdentityRequestExtension.GetTrackingId()}";
        var userRequestEntity = CreateUserRequest(filter, ApiCallType.VEDemographicQuery, currentUser, trackingId, notificationOptions);

        try
        {
            if (processType == ProcessType.Async)
            {
                await PublishMessageToSqs(ApiCallType.VEDemographicQuery, userRequestEntity);
                return trackingId;
            }

            return await DemographicQuery(userRequestEntity, filter);
        }
        catch (HcaMuleSoftException e)
        {
            UpdateProcessStatus(userRequestEntity, RequestStatus.Success, e.ToString());
            throw;
        }
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

    private async Task<DemographicQueryResponseContent?> DemographicQuery(UserRequestEntity userRequestEntity, Identity filter)
    {
        var requestStatusUpdater = new UserRequestStatusUpdater(_userRequestRepository, _requestProcessLogRepository);
        var demographicSearhRequest = new DemographicQueryClientIdentityRequest(userRequestEntity.TrackingId)
        {
            Content = filter
        };

        var response = await _clientIdentityRequestExecutor.Execute<DemographicQueryClientIdentityResponse>(demographicSearhRequest, requestStatusUpdater);
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

    private async Task PublishMessageToSqs(ApiCallType apiCallType, UserRequestEntity userRequestEntity)
    {
        var messageType = MessageType.UserRequest;
        if (messageType == null) throw new ArgumentException($"ClientIdentityService:PublishMessageToSqs: cannot process message for {apiCallType.GetStringValue()}");

        var userRequest = _userRequestMapper.MapToModel(userRequestEntity);
        var UserRequestMessage = new UserRequestMessage()
        {
            ApiCallType = apiCallType.GetStringValue(),
            UserRequest = userRequest
        };

        var sqsMessage = new SqsMessage()
        {
            MessageType = messageType,
            Payload = SerializationExtensions.Serialize(UserRequestMessage)
        };

        await _sqsPublisher.PublishMessage(sqsMessage);
    }

    private UserRequestEntity CreateUserRequest<T>(T request, ApiCallType apiCallType, string userName, string trackingId, NotificationOptions? notificationOptions)
    {
        if (request == null) throw new ArgumentNullException("ClientIdentityService:CreateUserRequest:Request cannot be nulle");

        var userRequest = new UserRequestEntity()
        {
            TrackingId = trackingId,
            UserName = userName,
            ApiCallType = apiCallType.GetStringValue(),
            RequestJson = SerializationExtensions.Serialize(request),
            ResponseJson = string.Empty,
            NotificationOptions = null == notificationOptions ? string.Empty : SerializationExtensions.Serialize(notificationOptions),
            RequestDateTime = DateTime.Now,
            ProcessStartTime = DateTime.Now,
            Status = RequestStatus.Processing.GetStringValue(),
            Message = string.Empty,
            RetryCount = 0
        };

        _userRequestRepository.AddAsync(userRequest);
        return userRequest;
    }

    private void UpdateProcessStatus(UserRequestEntity userRequest, RequestStatus status, string message)
    {
        userRequest.Status = status.GetStringValue();
        userRequest.Message = message;
        userRequest.ProcessEndTime = DateTime.Now;
        _userRequestRepository.Update(userRequest);
    }

    private async Task RemoveUserModifyRecords(string userName, Source s1, Source s2)
    {
        var source1Id = await _clientIdentityRepository.GetIdBySource(s1.Name, s1.Id);
        var source2Id = await _clientIdentityRepository.GetIdBySource(s2.Name, s2.Id);

        if (source1Id != null)
            await _userModifyRecordsService.RemoveModify(userName, source1Id ?? 0);

        if (source2Id != null)
            await _userModifyRecordsService.RemoveModify(userName, source2Id ?? 0);
    }
}

