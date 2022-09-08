using HCA.Data.Entities;
using HCA.Data.Repository;
using HCA.Infrastructure.Extensions;
using HCA.Models.Enums;
using HCA.Models.Request;
namespace HCA.Core.Processors;

public class UserRequestStatusUpdater : IRequestStatusUpdater
{
    private readonly IUserRequestRepository _userRequestRepository;

    private readonly IRequestProcessLogRepository _requestProcessLogRepository;

    public UserRequestStatusUpdater(IUserRequestRepository userRequestRepository, IRequestProcessLogRepository requestProcessLogRepository)
    {
        _userRequestRepository = userRequestRepository;
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

        var userRequest = await _userRequestRepository.GetSingleAsync(u => u.TrackingId == request.TrackingId);
        if (null == userRequest) return;
        userRequest.Status = status.GetStringValue();
        userRequest.Message = message;
        _userRequestRepository.Update(userRequest);
    }
}
