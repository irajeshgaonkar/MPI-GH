using HCA.Core.Mapper;
using HCA.Data;
using HCA.Data.Entities;
using HCA.Data.Repository;
using HCA.Infrastructure.Extensions;
using HCA.Infrastructure.Extensions.ModelExtensions;
using HCA.Infrastructure.Logger;
using HCA.Models;
using HCA.Models.Request;

namespace HCA.Core.Processors;

public class FileRequestProcessor : RequestProcessorBase
{
    private readonly IProcessorProvider _processorProvider;

    private readonly IFileRequestRepository _fileRequestRepository;

    private readonly IClientIdentityRequestRepository _clientIdentityRequestRepository;

    private readonly IMapper<ClientIdentityRequestEntity, ClientIdentityRequest> _clientIdentityRequestMapper;

    private FileRequestEntity _fileRequest;

    public FileRequestProcessor(ILogger logger, IProcessorProvider processorProvider, IFileRequestRepository fileRequestRepository,
        IRequestProcessLogRepository requestProcessLogRepository, IClientIdentityRequestRepository clientIdentityRequestRepository,
        IMapper<ClientIdentityRequestEntity, ClientIdentityRequest> mapper)
        : base(logger, requestProcessLogRepository)
    {
        _processorProvider = processorProvider;
        _fileRequestRepository = fileRequestRepository;
        _clientIdentityRequestRepository = clientIdentityRequestRepository;
        _clientIdentityRequestMapper = mapper;
    }

    public async Task ProcessRequest(Guid requestId)
    {
        try
        {
            LogTextPrefix = $"File Request Processor";
            var fileRequest = await _fileRequestRepository.GetRequest(requestId);

            if (null == fileRequest)
            {
                await LogInDatabase("Couldn't able to find the file request details");
                return;
            }

            _fileRequest = fileRequest;
            RequestId = _fileRequest.RequestId;
            await UpdatFileRequestProcessStart();
            await LogInDatabase("Completed fetching request details from databse");
            await ProcessPostIdentity();
            await UpdatFileRequestProcessStatus(DataConstants.Statuses.Succeded, "success", DateTime.Now);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex);
            await LogInDatabase(ex.ToString());

            if (null != _fileRequest)
            {
                await UpdatFileRequestProcessStatus(DataConstants.Statuses.Failed, ex.ToString());
            }
        }
    }

    private async Task ProcessPostIdentity()
    {
        int maxDegreeOfParallelism = 1;
        var processor = _processorProvider.PostIdentityProcessor;
        var groupedRequests = await GetGroupedRequests();
        await groupedRequests.ParallelForEachAsync((requests) => ProcessClientIdentity(processor, requests), maxDegreeOfParallelism);

    }

    private async Task ProcessClientIdentity(IPostIdentityProcessor processor, IEnumerable<ClientIdentityRequest> requests)
    {
        var requestId = requests.First().RequestId;
        ClientIdentityPostRequest request = new ClientIdentityPostRequest(requestId, requestId.ToString())
        {
            ClientIdentity = requests.ToList()
        };

        request.RequestId = RequestId;
        await processor.ProcessRequest(request);
    }

    private async Task UpdatFileRequestProcessStart()
    {
        _fileRequest!.Status = DataConstants.Statuses.Processing;
        _fileRequest!.Message = "";
        _fileRequest!.ProcessStartTime = DateTime.Now;
        await _fileRequestRepository.Update(_fileRequest);
    }

    private async Task UpdatFileRequestProcessStatus(string status, string message, DateTime? completedDate = null)
    {
        _fileRequest.Status = status;
        _fileRequest.Message = message;
        if (null != completedDate)
        {
            _fileRequest.ProcessEndTime = completedDate;
        }
        await _fileRequestRepository.Update(_fileRequest);
    }

    private async Task<IEnumerable<IEnumerable<ClientIdentityRequest>>> GetGroupedRequests()
    {
        Guid requestId = _fileRequest.RequestId;
        LogInformation($"RequestId {requestId}: Started getting the request records");
        var requests = (await _clientIdentityRequestRepository.GetRequests(requestId)).Take(3).ToList();
        var models = _clientIdentityRequestMapper.MapToModelCollection(requests);
        LogInformation($"RequestId {requestId}: Completed getting the request, records Count: {requests.Count}");
        var groupedRequests = models.GroupBySourceNameAndId();
        return groupedRequests;
    }
}