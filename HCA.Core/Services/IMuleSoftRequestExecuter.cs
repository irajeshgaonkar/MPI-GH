using HCA.Core.Processors;
using HCA.Models.Request;
using HCA.Models.Response;

namespace HCA.Core.Services
{
    public interface IMuleSoftRequestExecuter
    {
        Task<T?> Execute<T>(BaseRequest request, IRequestStatusUpdater statusUpdater) where T : BaseResponse;
    }
}

