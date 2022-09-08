using HCA.Data.Entities;
using HCA.Models.Request;

namespace HCA.Core.Services;

public interface IFileRequestService
{
    Task<FileRequest?> GetByTrackingId(string trackingId);

    Task<FileRequest?> GetByFileName(string fileName);

    Task<FileRequestEntity?> UpdatefileRequestComplete(string requestId);
}