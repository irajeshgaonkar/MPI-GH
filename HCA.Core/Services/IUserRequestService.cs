using HCA.Models.Request;

namespace HCA.Core.Services;

public interface IUserRequestService
{
    Task<UserRequest?> GetByTrackingId(string trackingId);
}

