using HCA.Core.Mapper;
using HCA.Core.Processors;
using HCA.Data.Repository;
using HCA.Infrastructure.DynamoDb;
using HCA.Infrastructure.Exceptions;
using HCA.Infrastructure.Logger;
using HCA.Models.Enums;
using HCA.Models.Request;
using HCA.Models.Response;

namespace HCA.Core.Services;

public class ClientIdentityRequestExecutor : IClientIdentityRequestExecutor
{
    private readonly IClientIdentityRepository _clientIdentityRepository;
    private readonly IMuleSoftRequestExecuter _muleSoftRequestExecuter;
    private readonly IDictionary<ApiCallType, Func<BaseRequest, IRequestStatusUpdater, Task<BaseResponse>>> requestExecuters;
    private readonly NotificationBuilder _notificationBuilder;
    private readonly HcaDynamoDbClient _dynamoDbClient;
    private readonly IAppLogger _logger;

    public ClientIdentityRequestExecutor(IClientIdentityRepository clientIdentityRepository,
        IMuleSoftRequestExecuter muleSoftRequestExecuter, IAppLogger logger)
    {
        _clientIdentityRepository = clientIdentityRepository;
        _muleSoftRequestExecuter = muleSoftRequestExecuter;
        requestExecuters = BuildRequestExecutors();
        _notificationBuilder = new NotificationBuilder();
        _dynamoDbClient = new HcaDynamoDbClient();
        _logger = logger;
    }

    public async Task<T?> Execute<T>(BaseRequest request, IRequestStatusUpdater requestStatusUpdater) where T : BaseResponse
    {
        //await requestStatusUpdater.UpdateStatus(request, RequestStatus.Processing, "Started Processing Request");
        var response = await requestExecuters[request.ApiCallType](request, requestStatusUpdater);
        if (response.Success) return response as T;
        //await requestStatusUpdater.UpdateStatus(request, RequestStatus.Failed, "Error processing the request");
        throw new HcaMuleSoftException("Error processing the request");
    }

    private IDictionary<ApiCallType, Func<BaseRequest, IRequestStatusUpdater, Task<BaseResponse>>> BuildRequestExecutors()
    {
        var requestExecuters = new Dictionary<ApiCallType, Func<BaseRequest, IRequestStatusUpdater, Task<BaseResponse>>>
        {
            [ApiCallType.VEPost] = PostIdentity,
            [ApiCallType.VELink] = LinkIdentities,
            [ApiCallType.VEUnLink] = UnLinkIdentities,
            [ApiCallType.VEMerge] = MergeIdentities,
            [ApiCallType.VEUnMerge] = UnMergeIdentities,
            [ApiCallType.VEDemographicSearch] = DemographicSearch,
        };

        return requestExecuters;
    }

    private async Task<BaseResponse> PostIdentity(BaseRequest request, IRequestStatusUpdater requestStatusUpdater)
    {
        var postIdentityRequest = Cast<PostClientIdentityRequest>(request);
        var notificationsUpdated = false;

        try
        {
            var response = await _muleSoftRequestExecuter.Execute<PostClientIdentityResponse>(postIdentityRequest, requestStatusUpdater);
            await UpdatePostIdentitiesNotification(postIdentityRequest, response);
            notificationsUpdated = true;

            if (null != response && response.Success && null != response.Content?.LinkId)
            {
                var entity = ClientIdentityMapper.MapFromRequestToEntity(response.Content.LinkId, DateTime.Now, postIdentityRequest.Content);
                await _clientIdentityRepository.Upsert(entity);
                return response;
            }

            throw new HcaBadRequestException("Error processing the request");
        }
        catch (HcaBadRequestException e)
        {
            if (!notificationsUpdated)
                await UpdatePostIdentitiesNotification(postIdentityRequest, null);
            throw;
        }
        catch (HcaMuleSoftException e)
        {
            if (!notificationsUpdated)
                await UpdatePostIdentitiesNotification(postIdentityRequest, null);
            throw;
        }
    }

    private async Task<BaseResponse> LinkIdentities(BaseRequest request, IRequestStatusUpdater requestStatusUpdater)
    {
        var linkIdentitiesRequest = Cast<LinkClientIdentityRequest>(request);
        var linkingSources = linkIdentitiesRequest.Content;
        var linkToIdentity = await _clientIdentityRepository.GetBySource(linkingSources.LinkToSource.Name, linkingSources.LinkToSource.Id);
        var sourceIdentity = await _clientIdentityRepository.GetBySource(linkingSources.Source.Name, linkingSources.Source.Id);
        var notificationsUpdated = false;

        try
        {
            if (null == linkToIdentity)
                throw new HcaBadRequestException("link source not found");

            if (null == sourceIdentity)
                throw new HcaBadRequestException("source not found");

            if (linkToIdentity.MpiLinkId == sourceIdentity.MpiLinkId)
                throw new HcaBadRequestException("sources are already linked");

            var response = await _muleSoftRequestExecuter.Execute<LinkClientIdentityResponse>(request, requestStatusUpdater);
            await UpdateLinkIdentitiesNotification(linkIdentitiesRequest, response, sourceIdentity.MpiLinkId);
            notificationsUpdated = true;

            if (null != response && response.Success && null != response.Content?.LinkId)
            {
                _clientIdentityRepository.UpdateMpiLinkId(sourceIdentity, response!.Content!.LinkId);
                return response;
            }

            throw new HcaBadRequestException("Error processing the request");
        }
        catch (HcaBadRequestException e)
        {
            if (!notificationsUpdated)
                await UpdateLinkIdentitiesNotification(linkIdentitiesRequest, null, sourceIdentity?.MpiLinkId ?? "");
            throw;
        }
        catch (HcaMuleSoftException e)
        {
            if (!notificationsUpdated)
                await UpdateLinkIdentitiesNotification(linkIdentitiesRequest, null, sourceIdentity?.MpiLinkId ?? "");
            throw;
        }
    }


    private async Task<BaseResponse> UnLinkIdentities(BaseRequest request, IRequestStatusUpdater requestStatusUpdater)
    {
        var unLinkClientIdentityRequest = Cast<UnLinkClientIdentityRequest>(request);
        var unLinkingSources = unLinkClientIdentityRequest.Content;
        var unlinkFromIdentity = await _clientIdentityRepository.GetBySource(unLinkingSources.UnlinkFromSource.Name, unLinkingSources.UnlinkFromSource.Id);
        var sourceIdentity = await _clientIdentityRepository.GetBySource(unLinkingSources.Source.Name, unLinkingSources.Source.Id);
        var notificationsUpdated = false;

        try
        {
            if (null == unlinkFromIdentity)
                throw new HcaBadRequestException("Un link source not found");

            if (null == sourceIdentity)
                throw new HcaBadRequestException("source not found");

            var response = await _muleSoftRequestExecuter.Execute<UnLinkClientIdentityResponse>(request, requestStatusUpdater);
            await UpdateUnLinkIdentitiesNotification(unLinkClientIdentityRequest, response, sourceIdentity.MpiLinkId);
            notificationsUpdated = true;

            if (null != response && response.Success && null != response.Content?.UnlinkedId)
            {
                _clientIdentityRepository.UpdateMpiLinkId(sourceIdentity, response.Content.UnlinkedId);
                return response;
            }

            throw new HcaBadRequestException("Error processing the request");
        }
        catch (HcaBadRequestException e)
        {
            if (!notificationsUpdated)
                await UpdateUnLinkIdentitiesNotification(unLinkClientIdentityRequest, null, sourceIdentity?.MpiLinkId ?? "");
            throw;
        }
        catch (HcaMuleSoftException e)
        {
            if (!notificationsUpdated)
                await UpdateUnLinkIdentitiesNotification(unLinkClientIdentityRequest, null, sourceIdentity?.MpiLinkId ?? "");
            throw;
        }
    }

    private async Task<BaseResponse> MergeIdentities(BaseRequest request, IRequestStatusUpdater requestStatusUpdater)
    {
        var mergeClientIdentityRequest = Cast<MergeClientIdentityRequest>(request);
        var mergingSources = mergeClientIdentityRequest.Content;
        var toSurviveIdentity = await _clientIdentityRepository.GetBySource(mergingSources.ToSurviveSource.Name, mergingSources.ToSurviveSource.Id);
        var toRetireIdentity = await _clientIdentityRepository.GetBySource(mergingSources.ToRetireSource.Name, mergingSources.ToRetireSource.Id);
        var notificationsUpdated = false;

        try
        {
            if (null == toSurviveIdentity)
                throw new HcaBadRequestException("To servive source not found");

            if (null == toRetireIdentity)
                throw new HcaBadRequestException("To retire source not found");
            var response = await _muleSoftRequestExecuter.Execute<MergeClientIdentityResponse>(request, requestStatusUpdater);
            await UpdateMergeIdentitiesNotification(mergeClientIdentityRequest, response, toRetireIdentity.MpiLinkId);
            notificationsUpdated = true;

            if (null != response && response.Success && null != response.Content?.LinkId)
            {
                _clientIdentityRepository.UpdateMpiLinkId(toRetireIdentity, response.Content.LinkId);
                return response;
            }

            throw new HcaBadRequestException("Error processing the request");
        }
        catch (HcaBadRequestException e)
        {
            if (!notificationsUpdated)
                await UpdateMergeIdentitiesNotification(mergeClientIdentityRequest, null, toRetireIdentity?.MpiLinkId ?? "");
            throw;
        }
        catch (HcaMuleSoftException e)
        {
            if (!notificationsUpdated)
                await UpdateMergeIdentitiesNotification(mergeClientIdentityRequest, null, toRetireIdentity?.MpiLinkId ?? "");
            throw;
        }
    }

    private async Task<BaseResponse> UnMergeIdentities(BaseRequest request, IRequestStatusUpdater requestStatusUpdater)
    {
        var unMergeClientIdentityRequest = Cast<UnMergeClientIdentityRequest>(request);
        var unMergingSources = unMergeClientIdentityRequest.Content;
        var unmergeFromIdentity = await _clientIdentityRepository.GetBySource(unMergingSources.UnmergeFromSource.Name, unMergingSources.UnmergeFromSource.Id);
        var unmergeSourceIdentity = await _clientIdentityRepository.GetBySource(unMergingSources.UnmergeSource.Name, unMergingSources.UnmergeSource.Id);
        var notificationsUpdated = false;

        try
        {
            if (null == unmergeFromIdentity)
                throw new HcaBadRequestException("Un merge from source not found");

            if (null == unmergeSourceIdentity)
                throw new HcaBadRequestException("Un merge source not found");
            var response = await _muleSoftRequestExecuter.Execute<UnMergeClientIdentityResponse>(request, requestStatusUpdater);
            await UpdateUnMergeIdentitiesNotification(unMergeClientIdentityRequest, response, unmergeSourceIdentity.MpiLinkId);
            notificationsUpdated = true;

            if (null != response && response.Success && null != response.Content?.UnmergedId)
            {
                _clientIdentityRepository.UpdateMpiLinkId(unmergeSourceIdentity, response.Content.UnmergedId);
                return response;
            }

            throw new HcaBadRequestException("Error processing the request");
        }
        catch (HcaBadRequestException e)
        {
            if (!notificationsUpdated)
                await UpdateUnMergeIdentitiesNotification(unMergeClientIdentityRequest, null, unmergeSourceIdentity?.MpiLinkId ?? "");
            throw;
        }
        catch (HcaMuleSoftException e)
        {
            if (!notificationsUpdated)
                await UpdateUnMergeIdentitiesNotification(unMergeClientIdentityRequest, null, unmergeSourceIdentity?.MpiLinkId ?? "");
            throw;
        }
    }

    private async Task<BaseResponse> DemographicSearch(BaseRequest request, IRequestStatusUpdater requestStatusUpdater)
    {
        var demographicSearchClientIdentityRequest = Cast<DemographicSearchClientIdentityRequest>(request);
        var notificationsUpdated = false;


        try
        {
            var response = await _muleSoftRequestExecuter.Execute<DemographicSearchClientIdentityResponse>(demographicSearchClientIdentityRequest, requestStatusUpdater);
            await UpdateDemographicSearchNotification(demographicSearchClientIdentityRequest, response);
            notificationsUpdated = true;

            if (null != response && response.Success && null != response.Content)
            {
                return response;
            }

            throw new HcaBadRequestException("Error processing the request");
        }
        catch (HcaBadRequestException e)
        {
            if (!notificationsUpdated)
                await UpdateDemographicSearchNotification(demographicSearchClientIdentityRequest, null);
            throw;
        }
        catch (HcaMuleSoftException e)
        {
            if (!notificationsUpdated)
                await UpdateDemographicSearchNotification(demographicSearchClientIdentityRequest, null);
            throw;
        }
    }

    private async Task UpdatePostIdentitiesNotification(PostClientIdentityRequest request, PostClientIdentityResponse? response)
    {
        try
        {
            var notification = _notificationBuilder.BuildPostIdentityNotification(request, response);
            await _dynamoDbClient.CreateItem(notification);
        }
        catch (Exception e)
        {
            _logger.LogError(e);
        }
    }

    private async Task UpdateLinkIdentitiesNotification(LinkClientIdentityRequest request, LinkClientIdentityResponse? response, string mpiLInkId)
    {
        try
        {
            var notification = _notificationBuilder.BuildLinkIdentityNotification(request, response, mpiLInkId);
            await _dynamoDbClient.CreateItem(notification);
        }
        catch (Exception e)
        {
            _logger.LogError(e);
        }
    }

    private async Task UpdateUnLinkIdentitiesNotification(UnLinkClientIdentityRequest request, UnLinkClientIdentityResponse? response, string previousLinkId)
    {
        try
        {
            var notification = _notificationBuilder.BuildUnLinkIdentityNotification(request, response, previousLinkId);
            await _dynamoDbClient.CreateItem(notification);
        }
        catch (Exception e)
        {
            _logger.LogError(e);
        }
    }

    private async Task UpdateMergeIdentitiesNotification(MergeClientIdentityRequest request, MergeClientIdentityResponse? response, string previousLinkId)
    {
        try
        {
            var notification = _notificationBuilder.BuildMergeIdentityNotification(request, response, previousLinkId);
            await _dynamoDbClient.CreateItem(notification);
        }
        catch (Exception e)
        {
            _logger.LogError(e);
        }
    }

    private async Task UpdateUnMergeIdentitiesNotification(UnMergeClientIdentityRequest request, UnMergeClientIdentityResponse? response, string previousLinkId)
    {
        try
        {
            var notification = _notificationBuilder.BuildUnMergeIdentityNotification(request, response, previousLinkId);
            await _dynamoDbClient.CreateItem(notification);
        }
        catch (Exception e)
        {
            _logger.LogError(e);
        }
    }

    private async Task UpdateDemographicSearchNotification(DemographicSearchClientIdentityRequest request, DemographicSearchClientIdentityResponse response)
    {
        try
        {
            var notification = _notificationBuilder.BuildDemographicSearchNotification(request, response);
            await _dynamoDbClient.CreateItem(notification);
        }
        catch (Exception e)
        {
            _logger.LogError(e);
        }
    }

    private T Cast<T>(BaseRequest request) where T : BaseRequest
    {
        T? muleSoftRequest = request as T;
        if (null == muleSoftRequest)
            throw new HcaMuleSoftException("Invalid input");
        return muleSoftRequest;
    }
}

