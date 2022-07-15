using System;
using HCA.Core.Mapper;
using HCA.Data;
using HCA.Data.Entities;
using HCA.Data.Repository;
using HCA.Infrastructure.Exceptions;
using HCA.Infrastructure.Extensions;
using HCA.Infrastructure.Extensions.ModelExtensions;
using HCA.Infrastructure.Logger;
using HCA.Models;
using HCA.Models.Request;
using HCA.Models.Response;
using HCA.MuleSoft;
using HCA.MuleSoft.Models.Response;
using HCA.MuleSoft.RequestBuilder;
using HCA.Infrastructure.Extensions;
using HCA.Models.MuleSoft;

namespace HCA.Core.Processors;

public interface IProcessor<T, U>
    where T : BaseRequest
    where U : BaseResponse
{
    Task<U> ProcessRequest(T request);
}

public class BaseProcessor
{
    protected readonly ILogger Logger;

    protected readonly IClientIdentityRequestRepository ClientIdentityRequestRepository;

    private readonly IMapper<ClientIdentityRequestEntity, ClientIdentityRequest> ClientIdentityRequestMapper;

    protected string LogPrefix;

    protected BaseProcessor(ILogger logger, IClientIdentityRequestRepository clientIdentityRequestRepository,
        IMapper<ClientIdentityRequestEntity, ClientIdentityRequest> clientIdentityRequestMapper)
    {
        Logger = logger;
        ClientIdentityRequestRepository = clientIdentityRequestRepository;
        ClientIdentityRequestMapper = clientIdentityRequestMapper;
    }

    protected bool HasErrors<T>(BaseResponse<T> response)
    {
        if (null != response.Errors && response.Errors.Count > 0) return true;
        if (null == response) return true;
        if (null == response.Content) return true;
        return false;
    }

    public async Task UpdateClientIdentityRequestStatus(IEnumerable<ClientIdentityRequest> requests, string? status, string? message, string? mpiLinkId = null)
    {
        var requestId = requests.First().RequestId;
        var trackingIds = requests.Select(r => r.TrackingId).ToList();
        var trackingIdsStr = trackingIds.JoinBy("|");
        if (null == trackingIds) return;
        LogInformation(requestId, $"{trackingIds}: Started incrementing retry count");
        await ClientIdentityRequestRepository.UpdateStatus(requestId, trackingIds, status, message, mpiLinkId);
        LogInformation(requestId, $"{trackingIds}: Completed incrementing retry count");
    }

    public async Task IncrementRetryCount(IEnumerable<ClientIdentityRequest> requests)
    {
        var entites = ClientIdentityRequestMapper.MapToEntityCollection(requests);
        foreach (var entity in entites)
        {
            entity.RetryCount++;
        }

        var requestId = requests.First().RequestId;
        var trackingIds = requests.Select(r => r.TrackingId).ToList();
        var trackingIdsStr = trackingIds.JoinBy(",");
        LogInformation(requestId, $"{trackingIdsStr}: Started incrementing retry count");
        await ClientIdentityRequestRepository.Update(entites);
        LogInformation(requestId, $"{trackingIdsStr}: Completed incrementing retry count");
    }

    private void LogInformation(Guid requestId, string message) => Logger.LogInformation($"{LogPrefix} Request Id: {requestId} => {message}");
}

public interface ILinkIdentityProcessor : IProcessor<LinkSourcesRequest, LinkSourcesResponse>
{

}

public interface IUnLinkIdentityProcessor : IProcessor<UnLinkSourcesRequest, UnLinkSourcesResponse>
{

}

public interface IMergeIdentityProcessor : IProcessor<MergingSourcesRequest, MergeSourcesResponse>
{

}

public interface IUnMergeIdentityProcessor : IProcessor<UnMergingSourcesRequest, UnMergeSourcesResponse>
{

}

public interface IDemographicSearchProcessor : IProcessor<DemographicSearchRequest, DemographicSearchResponse>
{

}


public class MergeIdentityProcessor : BaseProcessor, IMergeIdentityProcessor
{
    private readonly ILogger _logger;

    private readonly IMergeIdentitiesRequestBuilder _mergeIdentitiesRequestBuilder;

    private readonly IMuleSoftRepository _muleSoftRepository;

    private readonly IClientIdentityRepository _clientIdentityRepository;

    public MergeIdentityProcessor(IClientIdentityRequestRepository clientIdentityRequestRepository,
        IClientIdentityRepository clientIdentityRepository,
        IMergeIdentitiesRequestBuilder mergeIdentitiesRequestBuilder,
        IMuleSoftRepository muleSoftRepository, ILogger logger,
        IMapper<ClientIdentityRequestEntity, ClientIdentityRequest> clientIdentityRequestMapper
       ) : base(logger, clientIdentityRequestRepository, clientIdentityRequestMapper)
    {
        _clientIdentityRepository = clientIdentityRepository;
        _mergeIdentitiesRequestBuilder = mergeIdentitiesRequestBuilder;
        _muleSoftRepository = muleSoftRepository;
    }

    public async Task<MergeSourcesResponse> ProcessRequest(MergingSourcesRequest request)
    {
        LogPrefix = "Merge Identity Processor";

        var muleSoftRequest = _mergeIdentitiesRequestBuilder.Build(request);
        var mergeResponse = await _muleSoftRepository.MergeIdentities(muleSoftRequest);

        if (HasErrors(mergeResponse) || null == mergeResponse.Content.LinkId)
        {
            var errorMessage = mergeResponse.Errors?.JoinBy("|") ?? "Error occured while posting request to MuleSoft";
            throw new HcaMuleSoftException(errorMessage);
        }

        await _clientIdentityRepository.UpdateMpiLinkId(request.ToRetireSource.Name, request.ToRetireSource.Id, mergeResponse.Content.LinkId);

        return new MergeSourcesResponse()
        {
            LinkId = mergeResponse.Content.LinkId,
            Source = mergeResponse.Content.Source
        };
    }
}

public class UnMergeIdentityProcessor : BaseProcessor, IUnMergeIdentityProcessor
{
    private readonly ILogger _logger;

    private readonly IUnMergeIdentitiesRequestBuilder _unMergeIdentitiesRequestBuilder;

    private readonly IMuleSoftRepository _muleSoftRepository;

    private readonly IClientIdentityRepository _clientIdentityRepository;

    public UnMergeIdentityProcessor(IClientIdentityRequestRepository clientIdentityRequestRepository,
        IClientIdentityRepository clientIdentityRepository,
        IUnMergeIdentitiesRequestBuilder unMergeIdentitiesRequestBuilder,
        IMuleSoftRepository muleSoftRepository, ILogger logger,
        IMapper<ClientIdentityRequestEntity, ClientIdentityRequest> clientIdentityRequestMapper
       ) : base(logger, clientIdentityRequestRepository, clientIdentityRequestMapper)
    {
        _clientIdentityRepository = clientIdentityRepository;
        _unMergeIdentitiesRequestBuilder = unMergeIdentitiesRequestBuilder;
        _muleSoftRepository = muleSoftRepository;
    }

    public async Task<UnMergeSourcesResponse> ProcessRequest(UnMergingSourcesRequest request)
    {
        LogPrefix = "Un Merge Identity Processor";

        var muleSoftRequest = _unMergeIdentitiesRequestBuilder.Build(request);
        var unMergeIdentitiesResponse = await _muleSoftRepository.UnMergeIdentities(muleSoftRequest);

        if (HasErrors(unMergeIdentitiesResponse) || null == unMergeIdentitiesResponse.Content.UnmergedId)
        {
            var errorMessage = unMergeIdentitiesResponse.Errors?.JoinBy("|") ?? "Error occured while posting request to MuleSoft";
            throw new HcaMuleSoftException(errorMessage);
        }

        await _clientIdentityRepository.UpdateMpiLinkId(request.UnmergeSource.Name, request.UnmergeSource.Id, unMergeIdentitiesResponse.Content.UnmergedId);
        return new UnMergeSourcesResponse()
        {
            UnmergedId = unMergeIdentitiesResponse.Content.UnmergedId,
            UnmergedFromId = unMergeIdentitiesResponse.Content.UnmergedFromId,
            UnmergedFromSource = unMergeIdentitiesResponse.Content.UnmergedFromSource,
            UnmergedSource = unMergeIdentitiesResponse.Content.UnmergedSource
        };
    }
}

public class LinkIdentityProcessor : BaseProcessor, ILinkIdentityProcessor
{
    private readonly ILogger _logger;

    private readonly ILinkIdentitiesRequestBuilder _linkIdentitiesRequestBuilder;

    private readonly IMuleSoftRepository _muleSoftRepository;

    private readonly IClientIdentityRepository _clientIdentityRepository;

    public LinkIdentityProcessor(IClientIdentityRequestRepository clientIdentityRequestRepository,
        IClientIdentityRepository clientIdentityRepository,
        ILinkIdentitiesRequestBuilder linkIdentitiesRequestBuilder,
        IMuleSoftRepository muleSoftRepository, ILogger logger,
        IMapper<ClientIdentityRequestEntity, ClientIdentityRequest> clientIdentityRequestMapper
       ) : base(logger, clientIdentityRequestRepository, clientIdentityRequestMapper)
    {
        _clientIdentityRepository = clientIdentityRepository;
        _linkIdentitiesRequestBuilder = linkIdentitiesRequestBuilder;
        _muleSoftRepository = muleSoftRepository;
    }

    public async Task<LinkSourcesResponse> ProcessRequest(LinkSourcesRequest request)
    {
        LogPrefix = "Link Identity Processor";

        var muleSoftRequest = _linkIdentitiesRequestBuilder.Build(request);
        var linkIdentityResponse = await _muleSoftRepository.LinkIdentities(muleSoftRequest);

        if (HasErrors(linkIdentityResponse) || null == linkIdentityResponse.Content.LinkId)
        {
            var errorMessage = linkIdentityResponse.Errors?.JoinBy("|") ?? "Error occured while posting request to MuleSoft";
            throw new HcaMuleSoftException(errorMessage);
        }
        await _clientIdentityRepository.UpdateMpiLinkId(request.Source.Name, request.Source.Id, linkIdentityResponse.Content.LinkId);

        return new LinkSourcesResponse()
        {
            LinkId = linkIdentityResponse.Content.LinkId,
            LinkToSource = linkIdentityResponse.Content.LinkToSource
        };
    }
}

public class UnLinkIdentityProcessor : BaseProcessor, IUnLinkIdentityProcessor
{
    private readonly ILogger _logger;

    private readonly IUnLinkIdentitiesRequestBuilder _unLinkIdentitiesRequestBuilder;

    private readonly IMuleSoftRepository _muleSoftRepository;

    private readonly IClientIdentityRepository _clientIdentityRepository;

    public UnLinkIdentityProcessor(IClientIdentityRequestRepository clientIdentityRequestRepository,
        IClientIdentityRepository clientIdentityRepository,
        IUnLinkIdentitiesRequestBuilder unLinkIdentitiesRequestBuilder,
        IMuleSoftRepository muleSoftRepository, ILogger logger,
        IMapper<ClientIdentityRequestEntity, ClientIdentityRequest> clientIdentityRequestMapper
       ) : base(logger, clientIdentityRequestRepository, clientIdentityRequestMapper)
    {
        _clientIdentityRepository = clientIdentityRepository;
        _unLinkIdentitiesRequestBuilder = unLinkIdentitiesRequestBuilder;
        _muleSoftRepository = muleSoftRepository;
    }

    public async Task<UnLinkSourcesResponse> ProcessRequest(UnLinkSourcesRequest request)
    {
        LogPrefix = "Un Link Identity Processor";

        var muleSoftRequest = _unLinkIdentitiesRequestBuilder.Build(request);
        var unLinkResponse = await _muleSoftRepository.UnLinkIdentities(muleSoftRequest);

        if (HasErrors(unLinkResponse) || null == unLinkResponse.Content.UnlinkedId)
        {
            var errorMessage = unLinkResponse.Errors?.JoinBy("|") ?? "Error occured while posting request to MuleSoft";
            throw new HcaMuleSoftException(errorMessage);
        }

        await _clientIdentityRepository.UpdateMpiLinkId(request.Source.Name, request.Source.Id, unLinkResponse.Content.UnlinkedId);

        return new UnLinkSourcesResponse()
        {
            UnlinkedId = unLinkResponse.Content.UnlinkedId,
            UnlinkedSource = unLinkResponse.Content.UnlinkedSource,
            UnlinkedFromId = unLinkResponse.Content.UnlinkedFromId,
            UnlinkedFromSource = unLinkResponse.Content.UnlinkedFromSource,
        };
    }
}

public class DemographicSearchProcessor : BaseProcessor, IDemographicSearchProcessor
{
    private readonly ILogger _logger;

    private readonly IDemographicSearchRequestBuilder _demographicSearchRequestBuilder;

    private readonly IMuleSoftRepository _muleSoftRepository;

    private readonly IClientIdentityRepository _clientIdentityRepository;

    public DemographicSearchProcessor(IClientIdentityRequestRepository clientIdentityRequestRepository,
        IClientIdentityRepository clientIdentityRepository,
        IDemographicSearchRequestBuilder demographicSearchRequestBuilder,
        IMuleSoftRepository muleSoftRepository, ILogger logger,
        IMapper<ClientIdentityRequestEntity, ClientIdentityRequest> clientIdentityRequestMapper
       ) : base(logger, clientIdentityRequestRepository, clientIdentityRequestMapper)
    {
        _clientIdentityRepository = clientIdentityRepository;
        _demographicSearchRequestBuilder = demographicSearchRequestBuilder;
        _muleSoftRepository = muleSoftRepository;
    }

    public async Task<DemographicSearchResponse> ProcessRequest(DemographicSearchRequest request)
    {
        LogPrefix = "Demographic Identity Processor";
        var result = new List<ClientIdentity>();

        var muleSoftRequest = _demographicSearchRequestBuilder.Build(request);
        var searchResponse = await _muleSoftRepository.DemographicSearch(muleSoftRequest);

        if (HasErrors(searchResponse) || null == searchResponse.Content)
        {
            var errorMessage = searchResponse.Errors?.JoinBy("|") ?? "Error occured while posting request to MuleSoft";
            throw new HcaMuleSoftException(errorMessage);
        }

        var sources = new List<Source>();

        foreach(var groupedByIdentity in searchResponse.Content.SearchResults)
        {
            foreach(var identity in groupedByIdentity.IdentityGroupedBySource)
            {
                sources.AddRange(identity.Sources);
            }
        }

        var clientIdentites = await _clientIdentityRepository.GetBySources(sources);

        foreach (var clientIdentityEntity in clientIdentites)
        {
            var identityModel = ClientIdentityMapper.MapToClientIdentityModel(clientIdentityEntity);
            var identities = ClientIdentityMapper.MapToClientIdentity(identityModel);

            foreach (var identity in identities)
            {
                result.Add(identity);
            }
        }

        return new DemographicSearchResponse()
        {
            ClientIdentities = result
        };
    }
}

public interface IPostIdentityProcessor : IProcessor<ClientIdentityPostRequest, ClientIdentityPostResponse>
{

}

public class PostIdentityProcessor : BaseProcessor, IPostIdentityProcessor
{
    private readonly IPostIdentityRequestBuilder _postIdentityRequestBuilder;

    private readonly IMuleSoftRepository _muleSoftRepository;

    private readonly IClientIdentityRepository _clientIdentityRepository;

    public PostIdentityProcessor(IClientIdentityRequestRepository clientIdentityRequestRepository,
        IClientIdentityRepository clientIdentityRepository,
        IPostIdentityRequestBuilder postIdentityRequestBuilder,
        IMuleSoftRepository muleSoftRepository, ILogger logger,
        IMapper<ClientIdentityRequestEntity, ClientIdentityRequest> clientIdentityRequestMapper
       ) : base(logger, clientIdentityRequestRepository, clientIdentityRequestMapper)
    {
        _clientIdentityRepository = clientIdentityRepository;
        _muleSoftRepository = muleSoftRepository;
        _postIdentityRequestBuilder = postIdentityRequestBuilder;
    }

    public async Task<ClientIdentityPostResponse> ProcessRequest(ClientIdentityPostRequest request)
    {
        LogPrefix = "Post Identity Processor";
        await ProcessClientIdentity(request.ClientIdentity);
        return new ClientIdentityPostResponse();
    }

    private async Task ProcessClientIdentity(IEnumerable<ClientIdentityRequest> requests)
    {
        await UpdateClientIdentityRequestStatus(requests, DataConstants.Statuses.Processing, null);
        requests = await RemoveDuplicates(requests);
        var muleSoftRequest = _postIdentityRequestBuilder.Build(requests);
        var response = await _muleSoftRepository.PostIdentity(muleSoftRequest);
        var linkId = response!.Content.LinkId;

        if (HasErrors(response) || null == response.Content.LinkId)
        {
            var errorMessage = response.Errors?.JoinBy("|") ?? "Error occured while posting request to MuleSoft";
            await UpdateClientIdentityRequestStatus(requests, DataConstants.Statuses.Failed, errorMessage);
            return;
        }

        var entity = ClientIdentityMapper.MapFromRequestToEntity(linkId, DateTime.Now, requests);
        await _clientIdentityRepository.Upsert(entity);
        await UpdateClientIdentityRequestStatus(requests, DataConstants.Statuses.Succeded, "Successfully Procssed", linkId);
    }

    private async Task<IEnumerable<ClientIdentityRequest>> RemoveDuplicates(IEnumerable<ClientIdentityRequest> requests)
    {
        var (records, duplicateRecords) = requests.GetDuplicateRecords();
        if (null == duplicateRecords || duplicateRecords.Count() == 0)
            return records;

        await UpdateDuplicateRecordsStatus(duplicateRecords);
        return records;
    }

    private async Task UpdateDuplicateRecordsStatus(IEnumerable<ClientIdentityRequest> duplicateRequests)
    {
        var status = DataConstants.Statuses.Failed;
        var message = "Duplicate Record";
        await UpdateClientIdentityRequestStatus(duplicateRequests, status, message);
    }
  }
