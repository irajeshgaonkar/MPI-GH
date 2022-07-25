using HCA.Core.Mapper;
using HCA.Core.Services;
using HCA.Data;
using HCA.Data.Entities;
using HCA.Data.Repository;
using HCA.FileProcessor.FileReaders;
using HCA.Infrastructure.Exceptions;
using HCA.Infrastructure.Extensions;
using HCA.Infrastructure.Extensions.ModelExtensions;
using HCA.Infrastructure.Logger;
using HCA.Models.Enums;
using HCA.Models.MuleSoft;
using HCA.Models.Request;

namespace HCA.Core.Processors;

public class CsvFileWriter
{
    public MemoryStream WriteLine(List<string> streamReader)
    {
        return new MemoryStream();
    }

    public List<string> GetCsvFileLines(IEnumerable<ClientIdentityRequest> requests)
    {
        string[] _fieldNames = new string[] { "MPI Link ID", "Source System ID",
            "First Name", "Middle Name", "Last Name","Suffix", "Birth Date", "Gender", "SSN", "Address Type", "Address Line 1",
            "Address Line 2", "Address Line 3", "City", "State", "Zip Code", "Zip Plus Four", "Phone type", "Phone number",
            "Email type", "Email Address", "Protectec Population Flag", "Protected Population Type" };
        var lines = new List<string>();
        foreach (var request in requests)
        {
            var line = $"{request.MpiLinkId},{request.SourceSystemId},{request.SourceSystemUpdated},{request.FirstName},";
            line += $"{request.MiddleName},{request.LastName},{request.NameSuffix},{request.Dob},";
            line += $"{request.Gender},{request.Ssn},{request.AddressType},{request.AddressLine1},{request.AddressLine2},";
            line += $"{request.AddressLine3},{request.City},{request.State},{request.ZipCode},";
            line += $"{request.ZipFour},{request.PhoneType},{request.PhoneNumber},";
            line += $"{request.EmailType},{request.EmailAddress},{request.ProtectedPopulationFlag},";
            line += $"{request.ProtectedPopulationType},";
            line += $"{request.Status},{request.Message}";
            lines.Add(line);
        }
        return lines;
    }
}

public interface IFileWriter
{
    Task<MemoryStream> WriteFile(int requestId, StreamReader streamReader);
}

public class FileWriter : IFileWriter
{
    private readonly IFileReader _fileReader;

    private readonly IFileRequestRepository _fileRequestRepository;

    private readonly IClientIdentityRequestRepository _clientIdentityRequestRepository;

    private readonly IClientIdentityRequestMapper _clientIdentityRequestMapper;

    public FileWriter(IFileRequestRepository fileRequestRepository, IClientIdentityRequestRepository clientIdentityRequestRepository,
        IClientIdentityRequestMapper clientIdentityRequestMapper,
        IFileReader fileReader)
    {
        _fileRequestRepository = fileRequestRepository;
        _clientIdentityRequestRepository = clientIdentityRequestRepository;
        _clientIdentityRequestMapper = clientIdentityRequestMapper;
        _fileReader = fileReader;
    }

    public async Task<MemoryStream> WriteFile(int requestId, StreamReader streamReader)
    {
        var fileRequest = await _fileRequestRepository.GetRequest(requestId);
        var requests = await _clientIdentityRequestRepository.GetRequests(requestId);
        var models = _clientIdentityRequestMapper.MapToModelCollection(requests);

        var inputLines = _fileReader.ReadLines(streamReader);
        var headerLines = new List<string>();
        headerLines.Add(inputLines[0]);
        headerLines.Add(inputLines[1]);
        headerLines.Add("MPI Link ID,Source System ID,Source System Last Update ,First Name,Middle Name,Last Name,Suffix,Birth Date,Gender,SSN,Address Type,Address Line 1 ,Address Line 2,Address Line 3,City ,State,Zip Code,Zip Plus Four,Phone type,Phone number,Email type,Email Address,Protectec Population Flag,Protected Population Type,Status,Message,,,,,");
        var lines = new CsvFileWriter().GetCsvFileLines(models);


        MemoryStream streamToReturn = new MemoryStream();
        var writer = new StreamWriter(streamToReturn);

        foreach (var line in headerLines)
        {
            writer.WriteLine(line);
        }

        foreach (var line in lines)
        {
            writer.WriteLine(line + ",,,,,");
        }

        return streamToReturn;
    }
}

public interface IRequestUpdater
{
    Task UpdateRequestStatus(BaseRequest request, RequestStatus status, String message);
}

public class RequestUpdater : IRequestUpdater
{
    public async Task UpdateRequestStatus(BaseRequest request, RequestStatus status, string message)
    {
        await Task.Delay(1);
    }
}

public interface IFileRequestProcessor
{
    Task<RequestStatus> ProcessRequest(int requestId);
}

public class FileRequestProcessor : IFileRequestProcessor
{
    private IDictionary<ApiCallType, Func<FileRequestEntity, Task<(RequestStatus, string)>>> _requestProcessors;
    private readonly IAppLogger _logger;
    private readonly IFileRequestRepository _fileRequestRepository;
    private readonly IClientIdentityRequestRepository _clientIdentityRequestRepository;
    private readonly IClientIdentityRequestMapper _clientIdentityRequestMapper;
    private readonly IClientIdentityRequestExecutor _clientIdentityRequestExecutor;

    public FileRequestProcessor(IAppLogger logger, IFileRequestRepository fileRequestRepository,
        IClientIdentityRequestRepository clientIdentityRequestRepository, IClientIdentityRequestMapper clientIdentityRequestMapper,
        IClientIdentityRequestExecutor clientIdentityRequestExecutor)
    {
        _logger = logger;
        _fileRequestRepository = fileRequestRepository;
        _clientIdentityRequestRepository = clientIdentityRequestRepository;
        _clientIdentityRequestMapper = clientIdentityRequestMapper;
        _clientIdentityRequestExecutor = clientIdentityRequestExecutor;
    }

    public async Task<RequestStatus> ProcessRequest(int requestId)
    {
        var fileRequest = await _fileRequestRepository.GetRequest(requestId);

        if (null == fileRequest)
        {
            _logger.LogInformation("Couldn't able to find the file request details");
            return RequestStatus.Failed;
        }

        try
        {
            await UpdateProcessStart(fileRequest);
            await ProcessPostIdentityRequest(fileRequest);
            await UpdateProcessComplete(fileRequest, RequestStatus.Success, "");
            return RequestStatus.Success;
        }
        catch (Exception e)
        {
            if (null != fileRequest)
                await UpdateStatus(fileRequest, RequestStatus.Failed, e.ToString());

            _logger.LogInformation("Error processing the file request");
            _logger.LogError(e);
            return RequestStatus.Failed;
        }
    }

    private async Task ProcessLinkIdentityRequest(FileRequestEntity fileRequestEntity)
    {
        var requests = await GetClientIdentityRequests(fileRequestEntity);
        requests = await RemoveDuplicates(requests);

        if (!requests.Any())
            return;

        if (requests.Count < 2)
        {
            await Update(requests, string.Empty, RequestStatus.Failed, "Record cannot be linked, as there is only one record", null);
        }

        var linkToSourceIdentity = requests.First();
        var linkToSource = new Source(linkToSourceIdentity.SourceSystemName, linkToSourceIdentity.SourceSystemId);

        var trackingId = ClientIdentityRequestExtension.GetTrackingId(linkToSourceIdentity.SourceSystemName, linkToSourceIdentity.SourceSystemId);
        await Update(requests, trackingId, RequestStatus.Processing, "Processing", null);

        for (int i = 1; i < requests.Count; ++i)
        {
            var source = new Source(requests[i].SourceSystemName, requests[i].SourceSystemId);
            var linkingSources = new LinkingSources(linkToSource, source);
            var linkClientIdentityRequest = new LinkClientIdentityRequest(trackingId)
            {
                Content = linkingSources
            };
            var response = await _clientIdentityRequestExecutor.Execute<PostClientIdentityResponse>(linkClientIdentityRequest);
        }
    }

    private async Task ProcessPostIdentityRequest(FileRequestEntity requestEntity)
    {
        int maxDegreeOfParallelism = 1;
        var groupedRequests = await GetGroupedRequests(requestEntity);
        await groupedRequests.ParallelForEachAsync((requests) => ProcessPostIdentityRequests(requestEntity, requests), maxDegreeOfParallelism);
    }

    private async Task ProcessPostIdentityRequests(FileRequestEntity fileRequestEntity, IEnumerable<Models.Request.ClientIdentityRequest> requests)
    {
        requests = await RemoveDuplicates(requests);

        if (!requests.Any())
            return;

        var firstRequest = requests.First();

        var trackingId = ClientIdentityRequestExtension.GetTrackingId(firstRequest.SourceSystemName, firstRequest.SourceSystemId);
        await Update(requests, trackingId, RequestStatus.Processing, "Processing", null);

        var postIdentityRequest = new PostClientIdentityRequest(trackingId)
        {
            Content = requests.ToList()
        };

        try
        {
            var response = await _clientIdentityRequestExecutor.Execute<PostClientIdentityResponse>(postIdentityRequest);

            if (null == response || response.Success == false)
            {
                await Update(requests, trackingId, RequestStatus.Failed, response?.Errors.JoinBy("|") ?? "", null);
                return;
            }

            await Update(requests, trackingId, RequestStatus.Success, "", null);
        }
        catch(HcaMuleSoftException e)
        {
            await Update(requests, trackingId, RequestStatus.Failed, e.Message, null);
        }
    }

    private async Task<FileRequestEntity> UpdateProcessStart(FileRequestEntity requestEntity)
    {
        requestEntity.ProcessStartTime = DateTime.Now;
        return await UpdateStatus(requestEntity, RequestStatus.Processing, "Started Processing");
    }

    private async Task<FileRequestEntity> UpdateProcessComplete(FileRequestEntity requestEntity, RequestStatus status, string message)
    {
        requestEntity.ProcessEndTime = DateTime.Now;
        return await UpdateStatus(requestEntity, status, message);
    }

    private async Task<FileRequestEntity> UpdateStatus(FileRequestEntity requestEntity, RequestStatus status, string message)
    {
        requestEntity.Status = status.GetStringValue();
        requestEntity.Message = message;
        await _fileRequestRepository.Update(requestEntity);
        return requestEntity;
    }

    private async Task Update(IEnumerable<ClientIdentityRequest> requests, string? trackingId, RequestStatus? status, string? message, string? mpiLinkId)
    {
        if (null != trackingId)
            foreach (var request in requests)
                request.TrackingId = trackingId;

        var ids = requests.Select(r => r.Id).ToList();
        await _clientIdentityRequestRepository.Update(ids, status?.GetStringValue(), message, trackingId, mpiLinkId);
    }

    private async Task<IEnumerable<IEnumerable<Models.Request.ClientIdentityRequest>>> GetGroupedRequests(FileRequestEntity requestEntity)
    {
        var models = await GetClientIdentityRequests(requestEntity);
        var groupedRequests = models.GroupBySourceNameAndId();
        return groupedRequests;
    }

    private async Task<IList<ClientIdentityRequest>> GetClientIdentityRequests(FileRequestEntity requestEntity)
    {
        var fileRequestId = requestEntity.Id;
        _logger.LogInformation($"RequestId {fileRequestId}: Started getting the request records");
        var requests = (await _clientIdentityRequestRepository.GetRequests(fileRequestId, RequestStatus.NotStarted.GetStringValue())).ToList();
        var models = _clientIdentityRequestMapper.MapToModelCollection(requests).ToList();
        _logger.LogInformation($"RequestId {fileRequestId}: Completed getting the request, records Count: {requests.Count}");
        return models;
    }

    private async Task<IList<ClientIdentityRequest>> RemoveDuplicates(IEnumerable<Models.Request.ClientIdentityRequest> requests)
    {
        var (records, duplicateRecords) = requests.GetDuplicateRecords();
        if (null == duplicateRecords || duplicateRecords.Count() == 0)
            return records.ToList();

        await Update(duplicateRecords, string.Empty, RequestStatus.Failed, "Duplicate Record", null);
        return records.ToList();
    }
}