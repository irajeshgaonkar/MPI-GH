using HCA.Models.Enums;
using HCA.Models.Request;
namespace HCA.Core.Processors;

public interface IRequestStatusUpdater
{
    Task UpdateStatus(BaseRequest request, RequestStatus status, string message);
}
