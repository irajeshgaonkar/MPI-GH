using HCA.Core.Mapper;
using HCA.Core.Processors;
using HCA.Data.Entities;
using HCA.Data.Repository;
using HCA.Infrastructure.Exceptions;
using HCA.Infrastructure.Extensions;
using HCA.Infrastructure.Extensions.ModelExtensions;
using HCA.Models;
using HCA.Models.Enums;
using HCA.Models.MuleSoft;
using HCA.Models.MuleSoft.Response;
using HCA.Models.Request;

namespace HCA.Core.Services;

public interface IClientIdentityRequestExecutor
{
    Task<T?> Execute<T>(BaseRequest request) where T : BaseResponse;
}

public class ClientIdentityRequestExecutor : IClientIdentityRequestExecutor
{
    private readonly IClientIdentityRepository _clientIdentityRepository;

    private readonly IMuleSoftRequestExecuter _muleSoftRequestExecuter;

    private readonly IRequestUpdater _requestUpdater;

    private readonly IDictionary<ApiCallType, Func<BaseRequest, Task<BaseResponse>>> requestExecuters;

    public ClientIdentityRequestExecutor(IClientIdentityRepository clientIdentityRepository,
        IMuleSoftRequestExecuter muleSoftRequestExecuter, IRequestUpdater requestUpdater)
    {
        _clientIdentityRepository = clientIdentityRepository;
        _muleSoftRequestExecuter = muleSoftRequestExecuter;
        _requestUpdater = requestUpdater;
        requestExecuters = BuildRequestExecutors();
    }

    public async Task<T?> Execute<T>(BaseRequest request) where T : BaseResponse
    {
        await _requestUpdater.UpdateRequestStatus(request, RequestStatus.Processing, "Started Processing Request");
        var response = await requestExecuters[request.ApiCallType](request);
        if (response.Success) return response as T;
        await _requestUpdater.UpdateRequestStatus(request, RequestStatus.Failed, "Error processing the request");
        throw new HcaMuleSoftException("Error processing the request");
    }

    private IDictionary<ApiCallType, Func<BaseRequest, Task<BaseResponse>>> BuildRequestExecutors()
    {
        var requestExecuters = new Dictionary<ApiCallType, Func<BaseRequest, Task<BaseResponse>>>
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

    private async Task<BaseResponse> PostIdentity(BaseRequest request)
    {
        var postIdentityRequest = Cast<PostClientIdentityRequest>(request);
        var response = await _muleSoftRequestExecuter.Execute<PostClientIdentityResponse>(postIdentityRequest);

        if (null != response && response.Success && null != response.Content?.LinkId)
        {
            var entity = ClientIdentityMapper.MapFromRequestToEntity(response.Content.LinkId, DateTime.Now, postIdentityRequest.Content);
            await _clientIdentityRepository.Upsert(entity);
            return response;
        }

        throw new HcaBadRequestException("Error processing the request");
    }

    private async Task<BaseResponse> LinkIdentities(BaseRequest request)
    {
        var linkIdentitiesRequest = Cast<LinkClientIdentityRequest>(request);
        var linkingSources = linkIdentitiesRequest.Content;
        var linkToIdentity = (await _clientIdentityRepository.GetBySourceAndId(linkingSources.LinkToSource.Name, linkingSources.LinkToSource.Id)).FirstOrDefault();
        var sourceIdentity = (await _clientIdentityRepository.GetBySourceAndId(linkingSources.Source.Name, linkingSources.Source.Id)).FirstOrDefault();

        if (null == linkToIdentity)
            throw new HcaBadRequestException("link source not found");

        if (null == sourceIdentity)
            throw new HcaBadRequestException("source not found");

        if (linkToIdentity.MpiLinkId == sourceIdentity.MpiLinkId)
            throw new HcaBadRequestException("sources are already linked");

        var response = await _muleSoftRequestExecuter.Execute<LinkClientIdentityResponse>(request);
        if (null != response && response.Success && null != response.Content?.LinkId)
        {
            await _clientIdentityRepository.UpdateMpiLinkId(sourceIdentity.SourceSystemName, sourceIdentity.SourceSystemId, response.Content.LinkId);
            return response;
        }

        throw new HcaBadRequestException("Error processing the request");
    }

    private async Task<BaseResponse> UnLinkIdentities(BaseRequest request)
    {
        var unLinkClientIdentityRequest = Cast<UnLinkClientIdentityRequest>(request);
        var unLinkingSources = unLinkClientIdentityRequest.Content;

        var unlinkFromIdentity = (await _clientIdentityRepository.GetBySourceAndId(unLinkingSources.UnlinkFromSource.Name, unLinkingSources.UnlinkFromSource.Id)).FirstOrDefault();
        var sourceIdentity = (await _clientIdentityRepository.GetBySourceAndId(unLinkingSources.Source.Name, unLinkingSources.Source.Id)).FirstOrDefault();

        if (null == unlinkFromIdentity)
            throw new HcaBadRequestException("Un link source not found");

        if (null == sourceIdentity)
            throw new HcaBadRequestException("source not found");

        //if (unlinkFromIdentity.MpiLinkId != sourceIdentity.MpiLinkId)
        //    throw new HcaBadRequestException("sources are not linked");

        var response = await _muleSoftRequestExecuter.Execute<UnLinkClientIdentityResponse>(request);
        if (null != response && response.Success && null != response.Content?.UnlinkedId)
        {
            await _clientIdentityRepository.UpdateMpiLinkId(sourceIdentity.SourceSystemName, sourceIdentity.SourceSystemId, response.Content.UnlinkedId);
            return response;
        }

        throw new HcaBadRequestException("Error processing the request");
    }

    private async Task<BaseResponse> MergeIdentities(BaseRequest request)
    {
        var mergeClientIdentityRequest = Cast<MergeClientIdentityRequest>(request);
        var mergingSources = mergeClientIdentityRequest.Content;

        var toSurviveIdentity = (await _clientIdentityRepository.GetBySourceAndId(mergingSources.ToSurviveSource.Name, mergingSources.ToSurviveSource.Id)).FirstOrDefault();
        var toRetireIdentity = (await _clientIdentityRepository.GetBySourceAndId(mergingSources.ToRetireSource.Name, mergingSources.ToRetireSource.Id)).FirstOrDefault();

        if (null == toSurviveIdentity)
            throw new HcaBadRequestException("To servive source not found");

        if (null == toRetireIdentity)
            throw new HcaBadRequestException("To retire source not found");

        var response = await _muleSoftRequestExecuter.Execute<MergeClientIdentityResponse>(request);

        if (null != response && response.Success && null != response.Content?.LinkId)
        {
            toRetireIdentity.IsActive = false;
            toRetireIdentity.IsDelete = true;
            //toRetireIdentity.MpiLinkId = response.Content.LinkId;
            await _clientIdentityRepository.Update(toRetireIdentity);
            return response;
        }

        throw new HcaBadRequestException("Error processing the request");
    }

    private async Task<BaseResponse> UnMergeIdentities(BaseRequest request)
    {
        var unMergeClientIdentityRequest = Cast<UnMergeClientIdentityRequest>(request);
        var unMergingSources = unMergeClientIdentityRequest.Content;

        var unmergeFromIdentity = (await _clientIdentityRepository.GetBySourceAndId(unMergingSources.UnmergeFromSource.Name, unMergingSources.UnmergeFromSource.Id)).FirstOrDefault();
        var unmergeSourceIdentity = (await _clientIdentityRepository.GetBySourceAndId(unMergingSources.UnmergeSource.Name, unMergingSources.UnmergeSource.Id)).FirstOrDefault();

        if (null == unmergeFromIdentity)
            throw new HcaBadRequestException("Un merge from source not found");

        if (null == unmergeSourceIdentity)
            throw new HcaBadRequestException("Un merge source not found");

        var response = await _muleSoftRequestExecuter.Execute<UnMergeClientIdentityResponse>(request);

        if (null != response && response.Success && null != response.Content?.UnmergedId)
        {
            unmergeSourceIdentity.IsActive = true;
            unmergeSourceIdentity.IsDelete = false;
            await _clientIdentityRepository.Update(unmergeSourceIdentity);
            return response;
        }

        throw new HcaBadRequestException("Error processing the request");
    }

    private async Task<BaseResponse> DemographicSearch(BaseRequest request)
    {
        var demographicSearchClientIdentityRequest = Cast<DemographicSearchClientIdentityRequest>(request);
        var response = await _muleSoftRequestExecuter.Execute<DemographicSearchClientIdentityResponse>(demographicSearchClientIdentityRequest);
        var listClientIdentities = new List<ClientIdentityModel>();

        if (null != response && response.Success && null != response.Content)
        {
            foreach (var groupedIdentity in response.Content)
            {
                foreach (var identity in groupedIdentity.IdentityGroupedBySource)
                {
                    if (identity.Names.Any())
                    {
                        foreach (var name in identity.Names)
                        {
                            var clientIdentities = await _clientIdentityRepository.Search(identity.Names.First().Name.First, null, null, null, null);
                            if (null != clientIdentities)
                            {
                                if (null == clientIdentities) continue;
                                
                                foreach (var clientIdentity in clientIdentities)
                                {
                                    var identityModel = ClientIdentityMapper.MapToClientIdentityModel(clientIdentity);
                                    listClientIdentities.Add(identityModel);
                                }
                            }
                        }
                    }
                }
            }
            response.Result = listClientIdentities;
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

public class ClientIdentityService : IClientIdentityService
{
    private readonly IUserRequestRepository _userRequestRepository;

    private readonly IClientIdentityRequestExecutor _clientIdentityRequestExecutor;

    private readonly IClientIdentityRepository _clientIdentityRepository;

    public ClientIdentityService(IUserRequestRepository userRequestRepository,
        IClientIdentityRequestExecutor clientIdentityRequestExecutor,
        IClientIdentityRepository clientIdentityRepository)
    {
        _userRequestRepository = userRequestRepository;
        _clientIdentityRequestExecutor = clientIdentityRequestExecutor;
        _clientIdentityRepository = clientIdentityRepository;
    }

    public async Task<PagenatedCollection<ClientIdentity>> GetAll(int pageNumber, int recordsPerPage)
    {
        var skip = pageNumber * recordsPerPage;
        var result = new List<ClientIdentity>();

        var clientIdentitiesCount = await _clientIdentityRepository.GetCount();
        var clientIdentityEntities = await _clientIdentityRepository.GetAll(skip, recordsPerPage);
        int i = 1;

        foreach (var clientIdentityEntity in clientIdentityEntities)
        {
            var identityModel = ClientIdentityMapper.MapToClientIdentityModel(clientIdentityEntity);
            var identities = ClientIdentityMapper.MapToClientIdentity(identityModel);

            foreach (var identity in identities)
            {
                identity.Id = i++;
                result.Add(identity);
            }
        }

        var paginatedCollection = new PagenatedCollection<ClientIdentity>
        {
            RecordsCount = clientIdentitiesCount,
            PageNumber = pageNumber,
            RecordsPerPage = recordsPerPage,
            Data = result
        };

        return paginatedCollection;
    }

    public async Task<LinkIdentitiesResponseContent?> LinkIdentities(LinkingSources linkingSources)
    {
        var userRequest = await CreateUserRequest(linkingSources);

        try
        {
            var trackingId = linkingSources.Source.GetTrackingId();
            var linkIdentityRequest = new LinkClientIdentityRequest(trackingId)
            {
                Content = linkingSources
            };
            var response = await _clientIdentityRequestExecutor.Execute<LinkClientIdentityResponse>(linkIdentityRequest);
            await UpdateProcessStatus(userRequest, RequestStatus.Success, "Request Processed Successfully");
            return response?.Content;
        }
        catch (HcaMuleSoftException e)
        {
            await UpdateProcessStatus(userRequest, RequestStatus.Failed, e.ToString());
            throw;
        }
    }

    public async Task<UnLinkIdentitiesResponseContent?> UnLinkIdentities(UnLinkingSources unLinkingSources)
    {
        var userRequest = await CreateUserRequest(unLinkingSources);

        try
        {
            var trackingId = unLinkingSources.Source.GetTrackingId();
            var unLinkClientIdentityRequest = new UnLinkClientIdentityRequest(trackingId)
            {
                Content = unLinkingSources
            };
            var response = await _clientIdentityRequestExecutor.Execute<UnLinkClientIdentityResponse>(unLinkClientIdentityRequest);
            await UpdateProcessStatus(userRequest, RequestStatus.Success, "Request Processed Successfully");
            return response?.Content;
        }
        catch (HcaMuleSoftException e)
        {
            await UpdateProcessStatus(userRequest, RequestStatus.Failed, e.ToString());
            throw;
        }
    }

    public async Task<MergeIdentitiesResponseContent?> MergeIdentites(MergingSources mergingSources)
    {
        var userRequest = await CreateUserRequest(mergingSources);

        try
        {
            var trackingId = mergingSources.ToSurviveSource.GetTrackingId();
            var mergeClientIdentityRequest = new MergeClientIdentityRequest(trackingId)
            {
                Content = mergingSources
            };
            var response = await _clientIdentityRequestExecutor.Execute<MergeClientIdentityResponse>(mergeClientIdentityRequest);
            await UpdateProcessStatus(userRequest, RequestStatus.Success, "Request Processed Successfully");
            return response?.Content;
        }
        catch (HcaMuleSoftException e)
        {
            await UpdateProcessStatus(userRequest, RequestStatus.Failed, e.ToString());
            throw;
        }
    }

    public async Task<UnMergeIdentitiesResponseContent?> UnMergeIdentities(UnMergingSources unMergingSources)
    {
        var userRequest = await CreateUserRequest(unMergingSources);

        try
        {
            var trackingId = unMergingSources.UnmergeSource.GetTrackingId();
            var unMergeClientIdentityRequest = new UnMergeClientIdentityRequest(trackingId)
            {
                Content = unMergingSources
            };
            var response = await _clientIdentityRequestExecutor.Execute<UnMergeClientIdentityResponse>(unMergeClientIdentityRequest);
            await UpdateProcessStatus(userRequest, RequestStatus.Success, "Request Processed Successfully");
            return response?.Content;
        }
        catch (HcaMuleSoftException e)
        {
            await UpdateProcessStatus(userRequest, RequestStatus.Failed, e.ToString());
            throw;
        }
    }

    public async Task<PagenatedCollection<ClientIdentity>> Search(int pageNumber, int recordsPerPage, IdentityFilter filter)
    {
        var userRequest = await CreateUserRequest(filter);

        try
        {
            var trackingId = Guid.NewGuid().ToString();
            var skip = pageNumber * recordsPerPage;
            var demographicSearhRequest = new DemographicSearchClientIdentityRequest(trackingId)
            {
                Content = filter
            };

            var response = await _clientIdentityRequestExecutor.Execute<DemographicSearchClientIdentityResponse>(demographicSearhRequest);
            await UpdateProcessStatus(userRequest, RequestStatus.Success, "Request Processed Successfully");
            var result = new List<ClientIdentity>();
            int i = 1;

            if (null != response)
            {
                foreach (var model in response.Result)
                {
                    var identities = ClientIdentityMapper.MapToClientIdentity(model);

                    foreach (var identity in identities)
                    {
                        identity.Id = i++;
                        result.Add(identity);
                    }
                }
            }

            var paginatedCollection = new PagenatedCollection<ClientIdentity>
            {
                RecordsCount = result.Count(),
                PageNumber = pageNumber,
                RecordsPerPage = recordsPerPage,
                Data = result.Skip(skip).Take(recordsPerPage)
            };

            return paginatedCollection;

        }
        catch (HcaMuleSoftException e)
        {
            await UpdateProcessStatus(userRequest, RequestStatus.Success, e.ToString());
            throw;
        }
    }

    private async Task<UserRequestEntity> CreateUserRequest<T>(T request)
    {
        var userRequest = new UserRequestEntity()
        {
            TrackingId = Guid.NewGuid().ToString(),
            UserId = 1,
            RequestJson = SerializationExtensions.Serialize(request),
            RequestDateTime = DateTime.Now,
            ProcessStartTime = DateTime.Now,
            Status = RequestStatus.Processing.GetStringValue(),
            Message = string.Empty,
            RetryCount = 0
        };

        await _userRequestRepository.Insert(userRequest);
        return userRequest;
    }

    private async Task UpdateProcessStatus(UserRequestEntity userRequest, RequestStatus status, string message)
    {
        userRequest.Status = status.GetStringValue();
        userRequest.Message = message;
        userRequest.ProcessEndTime = DateTime.Now;
        await _userRequestRepository.Update(userRequest);
    }
}

