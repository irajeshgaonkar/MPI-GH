using HCA.Core.Processors;
using HCA.Models.Request;
using HCA.Models.Response;

namespace HCA.Core.Services;

public interface IClientIdentityRequestExecutor
{
    Task<T?> Execute<T>(BaseRequest request, IRequestStatusUpdater requestStatusUpdater)
        where T : BaseResponse;
}

