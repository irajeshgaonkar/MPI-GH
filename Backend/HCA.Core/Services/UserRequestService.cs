using HCA.Core.Mapper;
using HCA.Data.Repository;
using HCA.Infrastructure.Logger;
using HCA.Models.Request;

namespace HCA.Core.Services;

public class UserRequestService : IUserRequestService
{
    private readonly IUserRequestRepository _userRequestRepository;

    private readonly IAppLogger _appLogger;

    private readonly IUserRequestMapper _userRequestMapper;

    public UserRequestService(IUserRequestRepository userRequestRepository, IAppLogger appLogger,
        IUserRequestMapper userRequestMapper)
    {
        _userRequestRepository = userRequestRepository;
        _appLogger = appLogger;
        _userRequestMapper = userRequestMapper;
    }

    public async Task<UserRequest?> GetByTrackingId(string trackingId)
    {
        _appLogger.LogInformation($"Started processing GetByTrackingId::GetByFileName for TrackingId {trackingId}");
        var userRequestEntity = await _userRequestRepository.GetSingleAsync(f => f.TrackingId == trackingId);
        if (userRequestEntity == null) return null;
        var userRequest = _userRequestMapper.MapToModel(userRequestEntity);
        _appLogger.LogInformation($"Completed processing FileRequestService::GetByTrackingId for TrackingId {trackingId}");
        return userRequest;
    }
}

