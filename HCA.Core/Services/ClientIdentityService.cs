using System;
using HCA.Core.Mapper;
using HCA.Core.Processors;
using HCA.Data;
using HCA.Data.Entities;
using HCA.Data.Repository;
using HCA.Infrastructure.Exceptions;
using HCA.Infrastructure.Extensions;
using HCA.Models;
using HCA.Models.MuleSoft;
using HCA.MuleSoft.Models.Request.Link;
using HCA.MuleSoft.Models.Response.Link;
using HCA.MuleSoft.Models.Response.Merge;

namespace HCA.Core.Services;

public interface IClientIdentityService
{
    Task<PagenatedCollection<ClientIdentity>> GetAll(int pageNumber, int recordsPerPage);

    Task<PagenatedCollection<ClientIdentity>> Search(int pageNumber, int recordsPerPage, IdentityFilter filter);

    Task<LinkIdentitiesResponseContent?> LinkIdentities(LinkingSources linkingSources);

    Task<UnLinkIdentitiesResponseContent?> UnLinkIdentities(UnLinkingSources unLinkingSources);

    Task<MergeIdentitiesResponseContent?> MergeIdentites(MergingSources mergingSources);

    Task<UnMergeIdentitiesResponseContent?> UnMergeIdentities(UnMergingSources unMergingSources);

    //Task<bool?> DeleteIdentity(Source source);
}

public class ClientIdentityService : IClientIdentityService
{
    private readonly IUserRequestRepository _userRequestRepository;

    private readonly IClientIdentityRepository _clientIdentityRepository;

    private readonly ILinkIdentityProcessor _linkIdentityProcessor;

    private readonly IUnLinkIdentityProcessor _unLinkIdentityProcessor;

    private readonly IMergeIdentityProcessor _mergeIdentityProcessor;

    private readonly IUnMergeIdentityProcessor _unMergeIdentityProcessor;


    private readonly IDemographicSearchProcessor _demographicSearchProcessor;

    public ClientIdentityService(IClientIdentityRepository clientIdentityRepository, ILinkIdentityProcessor linkIdentityProcessor,
        IUnLinkIdentityProcessor unLinkIdentityProcessor, IMergeIdentityProcessor mergeIdentityProcessor,
        IUnMergeIdentityProcessor unMergeIdentityProcessor, IUserRequestRepository userRequestRepository,
        IDemographicSearchProcessor demographicSearchProcessor)
    {
        _clientIdentityRepository = clientIdentityRepository;
        _linkIdentityProcessor = linkIdentityProcessor;
        _unLinkIdentityProcessor = unLinkIdentityProcessor;
        _mergeIdentityProcessor = mergeIdentityProcessor;
        _unMergeIdentityProcessor = unMergeIdentityProcessor;
        _userRequestRepository = userRequestRepository;
        _demographicSearchProcessor = demographicSearchProcessor;
    }

    public async Task<PagenatedCollection<ClientIdentity>> GetAll(int pageNumber, int recordsPerPage)
    {
        var skip = pageNumber * recordsPerPage;
        var result = new List<ClientIdentity>();

        var clientIdentitiesCount = await _clientIdentityRepository.GetCount();
        var clientIdentityEntities = await _clientIdentityRepository.GetAll(skip, recordsPerPage);

        var count = clientIdentityEntities.ToList().Count();
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
            var linkIdentityRequest = new LinkSourcesRequest(userRequest.RequestId, userRequest.RequestId.ToString(),
                linkingSources.LinkToSource, linkingSources.Source);
            var response = await _linkIdentityProcessor.ProcessRequest(linkIdentityRequest);
            await UpdateProcessStatus(userRequest, DataConstants.Statuses.Succeded, "Request Processed Successfully");

            return new LinkIdentitiesResponseContent()
            {
                LinkId = response.LinkId,
                LinkToSource = linkingSources.LinkToSource
            };
        }
        catch (HcaMuleSoftException e)
        {
            await UpdateProcessStatus(userRequest, DataConstants.Statuses.Failed, e.ToString());
            throw;
        }
    }

    public async Task<UnLinkIdentitiesResponseContent?> UnLinkIdentities(UnLinkingSources unLinkingSources)
    {
        var userRequest = await CreateUserRequest(unLinkingSources);

        try
        {
            var unLinkIdentityRequest = new UnLinkSourcesRequest(userRequest.RequestId, userRequest.RequestId.ToString(), unLinkingSources.UnlinkFromSource, unLinkingSources.Source); ;
            var response = await _unLinkIdentityProcessor.ProcessRequest(unLinkIdentityRequest);
            await UpdateProcessStatus(userRequest, DataConstants.Statuses.Succeded, "Request Processed Successfully");

            return new UnLinkIdentitiesResponseContent()
            {
                UnlinkedId = response.UnlinkedId,
                UnlinkedSource = response.UnlinkedSource,
                UnlinkedFromId = response.UnlinkedFromId,
                UnlinkedFromSource = response.UnlinkedFromSource
            };
        }
        catch (HcaMuleSoftException e)
        {
            await UpdateProcessStatus(userRequest, DataConstants.Statuses.Failed, e.ToString());
            throw;
        }
    }

    public async Task<MergeIdentitiesResponseContent?> MergeIdentites(MergingSources mergingSources)
    {
        var userRequest = await CreateUserRequest(mergingSources);

        try
        {
            var mergeIdentitiesRequest = new MergingSourcesRequest(userRequest.RequestId, userRequest.RequestId.ToString(), mergingSources.ToSurviveSource, mergingSources.ToRetireSource);
            var response = await _mergeIdentityProcessor.ProcessRequest(mergeIdentitiesRequest);
            await UpdateProcessStatus(userRequest, DataConstants.Statuses.Succeded, "Request Processed Successfully");

            return new MergeIdentitiesResponseContent()
            {
                LinkId = response.LinkId,
                Source = mergingSources.ToSurviveSource
            };
        }
        catch (HcaMuleSoftException e)
        {
            await UpdateProcessStatus(userRequest, DataConstants.Statuses.Failed, e.ToString());
            throw;
        }
    }

    public async Task<UnMergeIdentitiesResponseContent?> UnMergeIdentities(UnMergingSources unMergingSources)
    {
        var userRequest = await CreateUserRequest(unMergingSources);

        try
        {
            var unMergeIdentitiesRequest = new UnMergingSourcesRequest(userRequest.RequestId, userRequest.RequestId.ToString(), unMergingSources.UnmergeFromSource, unMergingSources.UnmergeSource);
            var response = await _unMergeIdentityProcessor.ProcessRequest(unMergeIdentitiesRequest);
            await UpdateProcessStatus(userRequest, DataConstants.Statuses.Succeded, "Request Processed Successfully");

            return new UnMergeIdentitiesResponseContent()
            {
                UnmergedId = response.UnmergedId,
                UnmergedFromId = response.UnmergedFromId,
                UnmergedFromSource = response.UnmergedFromSource,
                UnmergedSource = response.UnmergedSource
            };
        }
        catch (HcaMuleSoftException e)
        {
            await UpdateProcessStatus(userRequest, DataConstants.Statuses.Failed, e.ToString());
            throw;
        }
    }

    public async Task<PagenatedCollection<ClientIdentity>> Search(int pageNumber, int recordsPerPage, IdentityFilter filter)
    {
        var userRequest = await CreateUserRequest(filter);

        try
        {
            var skip = pageNumber * recordsPerPage;
            var searchRequest = new DemographicSearchRequest(userRequest.RequestId, userRequest.RequestId.ToString(), filter);
            var result = await _demographicSearchProcessor.ProcessRequest(searchRequest);

            var paginatedCollection = new PagenatedCollection<ClientIdentity>
            {
                RecordsCount = result.ClientIdentities.Count(),
                PageNumber = pageNumber,
                RecordsPerPage = recordsPerPage,
                Data = result.ClientIdentities.Skip(skip).Take(pageNumber)
            };

            return paginatedCollection;

        }
        catch (HcaMuleSoftException e)
        {
            await UpdateProcessStatus(userRequest, DataConstants.Statuses.Failed, e.ToString());
            throw;
        }
    }

    private async Task<UserRequestEntity> CreateUserRequest<T>(T request)
    {
        var userRequest = new UserRequestEntity()
        {
            RequestId = Guid.NewGuid(),
            UserId = 1,
            RequestJson = request.Serialize(),
            RequestDateTime = DateTime.Now,
            ProcessStartTime = DateTime.Now,
            Status = DataConstants.Statuses.Processing,
            Message = string.Empty,
            RetryCount = 0
        };

        await _userRequestRepository.Insert(userRequest);
        return userRequest;
    }

    private async Task UpdateProcessStatus(UserRequestEntity userRequest, string status, string message)
    {
        userRequest.Status = status;
        userRequest.Message = message;
        userRequest.ProcessEndTime = DateTime.Now;
        await _userRequestRepository.Update(userRequest);
    }
}

