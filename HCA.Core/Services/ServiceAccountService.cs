using HCA.Core.Mapper;
using HCA.Data.Repository;
using HCA.Infrastructure.Logger;
using HCA.Models.Request;

namespace HCA.Core.Services
{
    public class ServiceAccountService : IServiceAccountService
    {
        private readonly IServiceAccountRepository _serviceAccountRepository;

        private readonly IAppLogger _appLogger;

        private readonly IServiceAccountMapper _serviceAccountMapper;

        public ServiceAccountService(IServiceAccountRepository serviceAccountRepository, IAppLogger appLogger,
            IServiceAccountMapper serviceAccountMapper)
        {
            _serviceAccountRepository = serviceAccountRepository;
            _appLogger = appLogger;
            _serviceAccountMapper = serviceAccountMapper;
        }
        public async Task<ServiceAccount?> GetServiceAccount(string appId)
        {
            _appLogger.LogInformation($"Started processing FileRequestService::GetByFileName for FileName {appId}");
            var fileRequestEntity = await _serviceAccountRepository.GetServiceAccount(appId);
            if (fileRequestEntity == null) return null;
            var fileRequest = _serviceAccountMapper.MapToModel(fileRequestEntity);
            _appLogger.LogInformation($"Completed processing FileRequestService::GetByFileName for FileName {appId}");
            return fileRequest;
        }

        public async Task<IEnumerable<ServiceAccount?>> GetServiceAccounts()
        {
            _appLogger.LogInformation($"Started processing FileRequestService::GetByFileName for FileName {new DateTime()}");
            var fileRequestEntity = await _serviceAccountRepository.GetServiceAccounts();
            if (fileRequestEntity == null) return null;
            var fileRequest = _serviceAccountMapper.MapToModelCollection(fileRequestEntity);
            _appLogger.LogInformation($"Completed processing FileRequestService::GetByFileName for FileName {new DateTime()}");
            return fileRequest;
        }
    }
}
