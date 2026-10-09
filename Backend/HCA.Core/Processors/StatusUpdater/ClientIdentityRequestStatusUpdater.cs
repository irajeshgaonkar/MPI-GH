using HCA.Data.Entities;
using HCA.Data.Repository;
using HCA.Infrastructure.Extensions;
using HCA.Models.Enums;
using HCA.Models.Request;
namespace HCA.Core.Processors;

public class ClientIdentityRequestStatusUpdater : IRequestStatusUpdater
{
    private readonly IClientIdentityRequestRepository _clientIdentityRequestRepository;

    private readonly IRequestProcessLogRepository _requestProcessLogRepository;

    public ClientIdentityRequestStatusUpdater(IClientIdentityRequestRepository clientIdentityRequestRepository,
        IRequestProcessLogRepository requestProcessLogRepository)
    {
        _clientIdentityRequestRepository = clientIdentityRequestRepository;
        _requestProcessLogRepository = requestProcessLogRepository;
    }

    public async Task UpdateStatus(BaseRequest request, RequestStatus status, string message)
    {
        var requestProcessLogEntity = new RequestProcessLogEntity()
        {
            TrackingId = request.TrackingId,
            DateTime = DateTime.Now,
            Status = status.GetStringValue(),
            Message = message
        };

        _requestProcessLogRepository.AddAsync(requestProcessLogEntity);

        var clientIdentityRequests = (await _clientIdentityRequestRepository.GetAllAsync(u => u.TrackingId == request.TrackingId)).ToList();
        if (null == clientIdentityRequests) return;

        foreach (var clientIdentityRequest in clientIdentityRequests)
        {
            clientIdentityRequest.Status = status.GetStringValue();
            clientIdentityRequest.Message = message;
            _clientIdentityRequestRepository.Update(clientIdentityRequest);
        }
    }
}
