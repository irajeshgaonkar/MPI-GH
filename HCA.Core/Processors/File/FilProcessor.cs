using HCA.Core.Mapper;
using HCA.Data.Entities;
using HCA.Data.Repository;
using HCA.FileProcessor.FileReaders;
using HCA.FileProcessor.Models;
using HCA.FileProcessor.Parsers;
using HCA.Infrastructure.Extensions;
using HCA.Infrastructure.Logger;
using HCA.Models;
using HCA.Models.Enums;

namespace HCA.Core.Processors.File;

public class FilProcessor : IFileProcessor
{
    private readonly IAppLogger _logger;

    private readonly IFileReader _fileReader;

    private readonly LineParser<FileHeaderModel> _headerParser;

    private readonly LineParser<FileClientIdentity> _clientIdentityParser;

    private readonly IFileRequestRepository _fileRequestRepository;

    private readonly IClientIdentityRequestRepository _clientIdentityRequestRepository;

    private readonly IFileClientIdentityMapper _fileClientIdentityMapper;

    public FilProcessor(IAppLogger logger, IFileReader fileReader, IFileRequestRepository requestRepository,
        IFileClientIdentityMapper fileClientIdentityMapper, IClientIdentityRequestRepository clientIdentityRequestRepository)
    {
        _logger = logger;
        _fileReader = fileReader;
        _fileRequestRepository = requestRepository;
        _fileClientIdentityMapper = fileClientIdentityMapper;
        _headerParser = new LineParser<FileHeaderModel>();
        _clientIdentityParser = new LineParser<FileClientIdentity>();
        _clientIdentityRequestRepository = clientIdentityRequestRepository;
    }

    public async Task<int> ProcessFile(string fileName, StreamReader streamReader)
    {
        _logger.LogInformation($"Started Processing File {fileName}");
        var request = await CreateFileRequest(fileName);
        var lines = _fileReader.ReadLines(streamReader);
        var headerData = _headerParser.Parse(lines[0], out string headerErrorMessage);
        request.TrackingId = null != headerData && headerData.TrackingId.IsNotEmpty()
                            ? headerData!.TrackingId
                            : Guid.NewGuid().ToString();
        //request.RequestDateTime = NEW ??Todo: update to date and time

        if (headerErrorMessage.IsEmpty() || null == headerData)
        {
            await UpdateStatus(request, RequestStatus.Failed, headerErrorMessage);
            _logger.LogInformation($"Invalid file header data: {headerErrorMessage}");
            return request.Id;
        }

        request.ApiCallType = headerData.ApiCallType.GetStringValue();
        request.SourceSystemAgency = headerData.SourceSystemAgency;
        request.SourceSystemName = headerData.SourceSystemName;
        await UpdateStatus(request, RequestStatus.Parsing, string.Empty);
        _logger.LogInformation($"Started parsing records for File {fileName}, Tracking Id {request.TrackingId}");

        var clientIdentities = ParseClientIdentities(lines);
        var entities = MapToEntitie(request, clientIdentities);
        await _clientIdentityRequestRepository.InsertBulk(entities);

        request.RecordsCount = entities.Count;
        await UpdateStatus(request, RequestStatus.DataLoaded, string.Empty);
        _logger.LogInformation($"ompleted parsing records for File {fileName}, Tracking Id {request.TrackingId}");

        _logger.LogInformation($"Completed Processing File {fileName}, Tracking Id {request.TrackingId}");
        return request.Id;
    }

    private IList<FileClientIdentity> ParseClientIdentities(List<string> lines)
    {
        List<FileClientIdentity> clientIdentities = new();

        for (int i = 3; i < lines.Count; ++i)
        {
            var values = _clientIdentityParser.ParseToValues(lines[i]);
            if (null == values || _clientIdentityParser.IsEmptyLine(values)) break;
            var clientIdentity = _clientIdentityParser.Parse(values, out string errorMessage);

            if (errorMessage.IsNotEmpty())
            {
                clientIdentity.Status = RequestStatus.Failed.GetStringValue();
                clientIdentity.Message = errorMessage;
            }
            else
            {
                clientIdentity.Status = RequestStatus.NotStarted.GetStringValue();
            }

            clientIdentities.Add(clientIdentity);
        }

        return clientIdentities;
    }

    private IList<ClientIdentityRequestEntity> MapToEntitie(FileRequestEntity fileRequestEntity, IList<FileClientIdentity> models)
    {
        var entities = _fileClientIdentityMapper.MapToEntityCollection(models);
        foreach (var entity in entities)
        {
            entity.TrackingId = string.Empty;
            entity.RequestId = fileRequestEntity.Id;
            entity.SourceSystemAgency = fileRequestEntity.SourceSystemAgency;
            entity.SourceSystemName = fileRequestEntity.SourceSystemName;
        }

        return entities.ToList();
    }

    private async Task<FileRequestEntity> CreateFileRequest(string fileName)
    {
        FileRequestEntity fileRequest = new()
        {
            FileName = fileName,
            TrackingId = string.Empty,
            SourceSystemAgency = string.Empty,
            SourceSystemName = string.Empty,
            Trailer = String.Empty,
            RecordsCount = 0,
            ApiCallType = string.Empty,
            RequestDateTime = DateTime.Now,
            FileCreatedDateTime = DateTime.Now,
            Status = RequestStatus.NotStarted.GetStringValue(),
            Message = string.Empty
        };

        await _fileRequestRepository.Insert(fileRequest);
        return fileRequest;
    }

    private async Task<FileRequestEntity> UpdateStatus(FileRequestEntity request, RequestStatus status, string message)
    {
        request.Status = status.GetStringValue();
        request.Message = message;

        //?Todo: Update the file request status history

        return await UpdateRequest(request);
    }

    private async Task<FileRequestEntity> UpdateRequest(FileRequestEntity fileRequestEntity)
    {
        await _fileRequestRepository.Update(fileRequestEntity);
        return fileRequestEntity;
    }
}



