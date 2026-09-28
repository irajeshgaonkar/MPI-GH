using HCA.Core.Mapper;
using HCA.Data.Entities;
using HCA.Data.Repository;
using HCA.Infrastructure.Extensions;
using HCA.Infrastructure.Logger;
using HCA.Models.Enums;
using HCA.Models.Request;

namespace HCA.Core.Services;

public class FileRequestService : IFileRequestService
{
    private readonly IFileRequestRepository _fileRequestRepository;

    private readonly IAppLogger _appLogger;

    private readonly IFileRequestMapper _fileRequestMapper;

    public FileRequestService(IFileRequestRepository fileRequestRepository, IAppLogger appLogger,
        IFileRequestMapper fileRequestMapper)
    {
        _fileRequestRepository = fileRequestRepository;
        _appLogger = appLogger;
        _fileRequestMapper = fileRequestMapper;
    }

    public async Task<FileRequest?> GetByFileName(string fileName)
    {
        _appLogger.LogInformation($"Started processing FileRequestService::GetByFileName for FileName {fileName}");
        var fileRequestEntity = await _fileRequestRepository.GetSingleAsync(f => f.FileName == fileName);
        if (fileRequestEntity == null) return null;
        var fileRequest = _fileRequestMapper.MapToModel(fileRequestEntity);
        _appLogger.LogInformation($"Completed processing FileRequestService::GetByFileName for FileName {fileName}");
        return fileRequest;
    }

    public async Task<FileRequest?> GetByTrackingId(string trackingId)
    {
        _appLogger.LogInformation($"Started processing GetByTrackingId::GetByFileName for TrackingId {trackingId}");
        var fileRequestEntity = await _fileRequestRepository.GetSingleAsync(f => f.TrackingId == trackingId);
        if (fileRequestEntity == null) return null;
        var fileRequest = _fileRequestMapper.MapToModel(fileRequestEntity);
        _appLogger.LogInformation($"Completed processing FileRequestService::GetByTrackingId for TrackingId {trackingId}");
        return fileRequest;
    }

    public async Task<FileRequestEntity?> UpdatefileRequestComplete(string requestId)
    {
        _appLogger.LogInformation($"Started processing GetByTrackingId::GetByFileName for RequestId {requestId}");
        var fileRequestEntity = await _fileRequestRepository.GetSingleAsync(f => f.RequestId == requestId);
        if (fileRequestEntity == null) return null;
        fileRequestEntity.ProcessEndTime = DateTime.Now;
        fileRequestEntity.Status = RequestStatus.Success.GetStringValue();
        _fileRequestRepository.Update(fileRequestEntity);
        return fileRequestEntity;
    }

    public async Task<FileRequestEntity?> UpdatefileRequestStatus(string requestId, string status)
    {
        _appLogger.LogInformation($"Started processing GetByTrackingId::GetByFileName for RequestId {requestId}");
        var fileRequestEntity = await _fileRequestRepository.GetSingleAsync(f => f.RequestId == requestId);
        if (fileRequestEntity == null) return null;
        fileRequestEntity.ProcessEndTime = DateTime.Now;
        fileRequestEntity.Status = status;
        _fileRequestRepository.Update(fileRequestEntity);
        return fileRequestEntity;
    }
}   