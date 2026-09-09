using HCA.Api.Dto;
using HCA.Models.Request;

namespace HCA.Api.Mapper;

public static class UserRequestDtoMapper
{
    public static UserRequestDto GetDto(UserRequest userRequest)
    {
        return new UserRequestDto()
        {
            TrackingId = userRequest.TrackingId,
            ApiCallType = userRequest.ApiCallType,
            UserName = userRequest.UserName,
            RequestJson = userRequest.RequestJson,
            ResponseJson = userRequest.ResponseJson,
            NotificationOptions = userRequest.NotificationOptions,
            RequestDateTime = userRequest.RequestDateTime,
            ProcessStartTime = userRequest.ProcessStartTime,
            ProcessEndTime = userRequest.ProcessEndTime,
            Status = userRequest.Status,
            Message = userRequest.Message
        };
    }

    public static IEnumerable<UserRequestDto> GetDto(IEnumerable<UserRequest> userRequests)
    {
        var result = new List<UserRequestDto>();

        foreach(var request in userRequests)
        {
            result.Add(GetDto(request));    
        }
        return result;
    }
}

