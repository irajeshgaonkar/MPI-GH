using HCA.Models.Request;

namespace HCA.Core.Services
{
    public interface IMuleSoftRequestExecuter
    {
        Task<T?> Execute<T>(BaseRequest request) where T : BaseResponse;
    }
}

