using HCA.Core.Mapper;
using HCA.Core.Services;
using HCA.Data.Entities;
using HCA.Data.Repository;
using HCA.Data;
using HCA.Infrastructure.Extensions;
using HCA.Infrastructure.Logger;
using HCA.Infrastructure.Sqs;
using HCA.Models.Enums;
using HCA.Models.Logging;
using HCA.Models.Request;
using HCA.Models.SQS;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace HCA.Core.Processors;

public class ClientIdentitySQSPublisher : IClientIdentitySQSPublisher
{
    private readonly IAppLogger _logger;

    private readonly IFileRequestService _fileRequestService;

    private readonly IFileRequestRepository _fileRequestRepository;

    private readonly IClientIdentityRequestRepository _clientIdentityRequestRepository;

    private readonly IOnboardedSystemRepository _onboardedSystemRepository;

    private readonly IRequestProcessLogRepository _requestProcessLogRepository;

    private readonly IClientIdentityRequestMapper _clientIdentityRequestMapper;

    private readonly ISqsPublisher _sqsPublisher;

    private readonly ITenantContext _tenantContext;

    public ClientIdentitySQSPublisher(IAppLogger appLogger, IFileRequestService
        fileRequestService, IClientIdentityRequestRepository clientIdentityRequestRepository,
        IRequestProcessLogRepository requestProcessLogRepository, IClientIdentityRequestMapper clientIdentityRequestMapper,
        ISqsPublisher sqsPublisher, ITenantContext tenantContext, IFileRequestRepository fileRequestRepository,
        IOnboardedSystemRepository onboardedSystemRepository)
    {
        _logger = appLogger;
        _fileRequestService = fileRequestService;
        _fileRequestRepository = fileRequestRepository;
        _clientIdentityRequestRepository = clientIdentityRequestRepository;
        _onboardedSystemRepository = onboardedSystemRepository;
        _requestProcessLogRepository = requestProcessLogRepository;
        _clientIdentityRequestMapper = clientIdentityRequestMapper;
        _sqsPublisher = sqsPublisher;
        _tenantContext = tenantContext;
    }

    public async Task Publish(string requestId)
    {
        FileRequestEntity fileRequest = new();
        try
        {
            _logger.LogInformation($"Starting tenant database resolution for requestId: {requestId}");
            var tenantDatabase = await GetTenantDatabase(requestId);
            _tenantContext.SetTenantDatabase(tenantDatabase);
            _logger.LogInformation($"Resolved tenant database '{tenantDatabase.ToTenantValue()}' for requestId: {requestId}");

            fileRequest = await _fileRequestService.UpdatefileRequestStatus(requestId, RequestStatus.Processing.GetStringValue());

            if (null == fileRequest)
            {
                _logger.LogInformation($"Couldn't able to find the file request details for requestId: {requestId}, tenant: {tenantDatabase.ToTenantValue()}");
                return;
            }

            await ValidateSourceSystem(fileRequest, tenantDatabase);

            var requestEntities = await _clientIdentityRequestRepository.GetRequests(requestId);
            var allRequests = _clientIdentityRequestMapper.MapToModelCollection(requestEntities);

            var groupedRequests = allRequests.GroupBy(g => g.BatchNumber);
            //_logger.LogInformation($"Started Publishing records to SQS for requestId: {requestId}");
            int maxDegreeOfParallelism = 1;
            await groupedRequests.ParallelForEachAsync((requests) => PublishMessageToQueue(requests.ToList(), requests.Key, fileRequest.RequestId, fileRequest.ApiCallType, tenantDatabase), maxDegreeOfParallelism);
            _logger.LogInformation($"Completed Publishing records to SQS for requestId: {requestId}");
        }
        catch(Exception ex)
        {
            var exceptionCustomProperties = new ExceptionCustomProperties
            {
                User = "BatchProcess",
                Agency = fileRequest.SourceSystemAgency,
                FunctionName = nameof(Publish),
                ErrorMessage = ex.Message,
                ErrorCode = "500",
                StackTrace = ex.StackTrace
            };
            var errorLogItem = new LogItem()
            {
                Name = $"{Models.Logging.Constants.LogPrefix_Batch}{nameof(Publish)}-Failed",
                TrackingId = fileRequest.TrackingId,
                Layer = ServiceLayer.Batch.ToString(),
                ExceptionCustomProperties = exceptionCustomProperties
            };

            _logger.LogError(ex, JsonConvert.SerializeObject(errorLogItem));
            throw;
        }
    }

    private async Task PublishMessageToQueue(IEnumerable<ClientIdentityRequest> identityRequests, int batchNumber, string requestId, string apiCallType, TenantDatabaseKind tenantDatabase)
    {

        var batchProcessMessage = new BatchProcessMessage()
        {
            ApiCallType = apiCallType,
            TenantDatabase = tenantDatabase.ToTenantValue(),
            ClientIdentityRequests = identityRequests
        };

        var sqsMessage = new SqsMessage()
        {
            MessageType = MessageType.BatchProcess,
            Payload = SerializationExtensions.SerializeWithoutCasing(batchProcessMessage)
        };

        await _sqsPublisher.PublishMessage(sqsMessage);
        //UpdateProcessLog(requestId, $"Posted message on to SQS for RequestId: {requestId} and BatchNumber: {batchNumber}");
    }

    private async Task<TenantDatabaseKind> GetTenantDatabase(string requestId)
    {
        var tenantDatabaseByRequestId = await _fileRequestRepository.GetTenantDatabaseByRequestId(requestId);
        if (tenantDatabaseByRequestId.HasValue)
        {
            _logger.LogInformation($"Tenant database lookup found '{tenantDatabaseByRequestId.Value.ToTenantValue()}' for requestId: {requestId}");
            return tenantDatabaseByRequestId.Value;
        }

        _logger.LogInformation($"Tenant database lookup did not find requestId: {requestId}; defaulting to '{TenantDatabaseKind.Coalition.ToTenantValue()}'");
        return TenantDatabaseKind.Coalition;
    }

    private async Task ValidateSourceSystem(FileRequestEntity fileRequest, TenantDatabaseKind tenantDatabase)
    {
        var sourceSystemExists = await _onboardedSystemRepository.ActiveSourceSystemExistsAsync(fileRequest.SourceSystemName);
        if (sourceSystemExists)
        {
            return;
        }

        throw new InvalidOperationException(
            $"Source system '{fileRequest.SourceSystemName}' is not active in the {tenantDatabase} tenant database.");
    }

    private void UpdateProcessLog(string requestId, string message)
    {
        _logger.LogInformation(message);
        var requestProcessLogEntity = new RequestProcessLogEntity()
        {
            TrackingId = requestId,
            DateTime = DateTime.Now,
            Status = RequestStatus.Processing.GetStringValue(),
            Message = message
        };

        _requestProcessLogRepository.AddAsync(requestProcessLogEntity);
    }
}
