using HCA.Core.Mapper;
using HCA.Core.Processors;
using HCA.Data.Repository;
using HCA.Infrastructure.Exceptions;
using HCA.Models.Enums;
using HCA.Models.Request;
using HCA.Models.Response;

namespace HCA.Core.Services;

public class ClientIdentityRequestExecutor : IClientIdentityRequestExecutor
{
    private readonly IClientIdentityRepository _clientIdentityRepository;
    private readonly IMuleSoftRequestExecuter _muleSoftRequestExecuter;
    private readonly IDictionary<ApiCallType, Func<BaseRequest, IRequestStatusUpdater, Task<BaseResponse>>> requestExecuters;

    public ClientIdentityRequestExecutor(IClientIdentityRepository clientIdentityRepository,
        IMuleSoftRequestExecuter muleSoftRequestExecuter)
    {
        _clientIdentityRepository = clientIdentityRepository;
        _muleSoftRequestExecuter = muleSoftRequestExecuter;
        requestExecuters = BuildRequestExecutors();
    }

    public async Task<T?> Execute<T>(BaseRequest request, IRequestStatusUpdater requestStatusUpdater) where T : BaseResponse
    {
        await requestStatusUpdater.UpdateStatus(request, RequestStatus.Processing, "Started Processing Request");
        var response = await requestExecuters[request.ApiCallType](request, requestStatusUpdater);
        if (response.Success) return response as T;
        await requestStatusUpdater.UpdateStatus(request, RequestStatus.Failed, "Error processing the request");
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
        var response = await _muleSoftRequestExecuter.Execute<PostClientIdentityResponse>(postIdentityRequest, requestStatusUpdater);

        if (null != response && response.Success && null != response.Content?.LinkId)
        {
            var entity = ClientIdentityMapper.MapFromRequestToEntity(response.Content.LinkId, DateTime.Now, postIdentityRequest.Content);
            await _clientIdentityRepository.Upsert(entity);
            return response;
        }

        throw new HcaBadRequestException("Error processing the request");
    }

    private async Task<BaseResponse> LinkIdentities(BaseRequest request, IRequestStatusUpdater requestStatusUpdater)
    {
        var linkIdentitiesRequest = Cast<LinkClientIdentityRequest>(request);
        var linkingSources = linkIdentitiesRequest.Content;
        var linkToIdentity = await _clientIdentityRepository.GetBySource(linkingSources.LinkToSource.Name, linkingSources.LinkToSource.Id);
        var sourceIdentity = await _clientIdentityRepository.GetBySource(linkingSources.Source.Name, linkingSources.Source.Id);

        if (null == linkToIdentity)
            throw new HcaBadRequestException("link source not found");

        if (null == sourceIdentity)
            throw new HcaBadRequestException("source not found");

        if (linkToIdentity.MpiLinkId == sourceIdentity.MpiLinkId)
            throw new HcaBadRequestException("sources are already linked");

        var response = await _muleSoftRequestExecuter.Execute<LinkClientIdentityResponse>(request, requestStatusUpdater);

        if (null != response && response.Success && null != response.Content?.LinkId)
        {
            _clientIdentityRepository.UpdateMpiLinkId(sourceIdentity, response!.Content!.LinkId);
            return response;
        }

        throw new HcaBadRequestException("Error processing the request");
    }

    private async Task<BaseResponse> UnLinkIdentities(BaseRequest request, IRequestStatusUpdater requestStatusUpdater)
    {
        var unLinkClientIdentityRequest = Cast<UnLinkClientIdentityRequest>(request);
        var unLinkingSources = unLinkClientIdentityRequest.Content;
        var unlinkFromIdentity = await _clientIdentityRepository.GetBySource(unLinkingSources.UnlinkFromSource.Name, unLinkingSources.UnlinkFromSource.Id);
        var sourceIdentity = await _clientIdentityRepository.GetBySource(unLinkingSources.Source.Name, unLinkingSources.Source.Id);

        if (null == unlinkFromIdentity)
            throw new HcaBadRequestException("Un link source not found");

        if (null == sourceIdentity)
            throw new HcaBadRequestException("source not found");

        var response = await _muleSoftRequestExecuter.Execute<UnLinkClientIdentityResponse>(request, requestStatusUpdater);

        if (null != response && response.Success && null != response.Content?.UnlinkedId)
        {
            _clientIdentityRepository.UpdateMpiLinkId(sourceIdentity, response.Content.UnlinkedId);
            return response;
        }

        throw new HcaBadRequestException("Error processing the request");
    }

    private async Task<BaseResponse> MergeIdentities(BaseRequest request, IRequestStatusUpdater requestStatusUpdater)
    {
        var mergeClientIdentityRequest = Cast<MergeClientIdentityRequest>(request);
        var mergingSources = mergeClientIdentityRequest.Content;
        var toSurviveIdentity = await _clientIdentityRepository.GetBySource(mergingSources.ToSurviveSource.Name, mergingSources.ToSurviveSource.Id);
        var toRetireIdentity = await _clientIdentityRepository.GetBySource(mergingSources.ToRetireSource.Name, mergingSources.ToRetireSource.Id);

        if (null == toSurviveIdentity)
            throw new HcaBadRequestException("To servive source not found");

        if (null == toRetireIdentity)
            throw new HcaBadRequestException("To retire source not found");
        var response = await _muleSoftRequestExecuter.Execute<MergeClientIdentityResponse>(request, requestStatusUpdater);

        if (null != response && response.Success && null != response.Content?.LinkId)
        {
            _clientIdentityRepository.UpdateMpiLinkId(toRetireIdentity, response.Content.LinkId);
            return response;
        }

        throw new HcaBadRequestException("Error processing the request");
    }

    private async Task<BaseResponse> UnMergeIdentities(BaseRequest request, IRequestStatusUpdater requestStatusUpdater)
    {
        var unMergeClientIdentityRequest = Cast<UnMergeClientIdentityRequest>(request);
        var unMergingSources = unMergeClientIdentityRequest.Content;
        var unmergeFromIdentity = await _clientIdentityRepository.GetBySource(unMergingSources.UnmergeFromSource.Name, unMergingSources.UnmergeFromSource.Id);
        var unmergeSourceIdentity = await _clientIdentityRepository.GetBySource(unMergingSources.UnmergeSource.Name, unMergingSources.UnmergeSource.Id);

        if (null == unmergeFromIdentity)
            throw new HcaBadRequestException("Un merge from source not found");

        if (null == unmergeSourceIdentity)
            throw new HcaBadRequestException("Un merge source not found");
        var response = await _muleSoftRequestExecuter.Execute<UnMergeClientIdentityResponse>(request, requestStatusUpdater);

        if (null != response && response.Success && null != response.Content?.UnmergedId)
        {
            _clientIdentityRepository.UpdateMpiLinkId(unmergeSourceIdentity, response.Content.UnmergedId);
            return response;
        }

        throw new HcaBadRequestException("Error processing the request");
    }

    private async Task<BaseResponse> DemographicSearch(BaseRequest request, IRequestStatusUpdater requestStatusUpdater)
    {
        var demographicSearchClientIdentityRequest = Cast<DemographicSearchClientIdentityRequest>(request);
        var response = await _muleSoftRequestExecuter.Execute<DemographicSearchClientIdentityResponse>(demographicSearchClientIdentityRequest, requestStatusUpdater);

        if (null != response && response.Success && null != response.Content)
        {
            return response;
        }

        throw new HcaBadRequestException("Error processing the request");
    }

    private T Cast<T>(BaseRequest request) where T : BaseRequest
    {
        T? muleSoftRequest = request as T;
        if (null == muleSoftRequest)
            throw new HcaMuleSoftException("Invalid input");
        return muleSoftRequest;
    }
}

