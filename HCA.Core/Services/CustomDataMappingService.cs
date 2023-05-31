using HCA.Core.Mapper;
using HCA.Data.Entities;
using HCA.Data.Repository;
using HCA.Infrastructure.Logger;
using HCA.Models.Enums;
using HCA.Models.Request;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HCA.Core.Services
{
    public class CustomDataMappingService : ICustomDataMappingService
    {
        private readonly ICustomDataMappingRepository _customDataMappingRepository;

        private readonly IAppLogger _appLogger;

        private readonly ICustomDataMappingMapper _customDataMappingMapper;

        public CustomDataMappingService(ICustomDataMappingRepository customDataMappingRepository, IAppLogger appLogger,
            ICustomDataMappingMapper customDataMappingMapper)
        {
            _customDataMappingRepository = customDataMappingRepository;
            _appLogger = appLogger;
            _customDataMappingMapper = customDataMappingMapper;
        }

        public async Task<CustomDataMapping?> AddCustomDataMapping(CustomDataMapping customData)
        {
            _appLogger.LogInformation($"Started processing CustomDataMappingService::Add CustomDataMapping {customData}");
            var fileResponseEntity = _customDataMappingMapper.MapToEntity(customData);
            var fileResponseModel = await _customDataMappingRepository.AddCustomDataMapping(fileResponseEntity);
           var fileResponseData = _customDataMappingMapper.MapToModel(fileResponseModel);
            if (fileResponseData == null) return null;
            return fileResponseData;
        }

        public async Task<CustomDataMapping?> GetCustomDataMapping(int id)
        {
            _appLogger.LogInformation($"Started processing CustomDataMappingService::GetCustomDataMapping for Id {id}");
            var fileResponseEntity = await _customDataMappingRepository.GetCustomDataMapping(id);
            if (fileResponseEntity == null) return null;
            var fileResponse = _customDataMappingMapper.MapToModel(fileResponseEntity);
            _appLogger.LogInformation($"Completed processing CustomDataMappingService::GetCustomDataMapping for Id {id}");
            return fileResponse;
        }

        public async Task<IEnumerable<CustomDataMapping?>> GetCustomDataMappings()
        {
            _appLogger.LogInformation($"Started processing CustomDataMappingService::GetCustomDataMappings for Id {new DateTime()}");
            var fileResponseEntity = await _customDataMappingRepository.GetCustomDataMappings();
            if (fileResponseEntity == null) return null;
            var fileResponse = _customDataMappingMapper.MapToModelCollection(fileResponseEntity);
            _appLogger.LogInformation($"Completed processing CustomDataMappingService::GetById for Id {new DateTime()}");
            return fileResponse;
        }

        public async Task RemoveCustomDataMapping(int id)
        {
            var entity = await _customDataMappingRepository.GetSingleAsync(m => m.Id == id);

            if (entity != null)
            {
                _customDataMappingRepository.Delete(entity);
            }
        }

        public async Task<CustomDataMapping?> UpdateCustomDataMapping(CustomDataMapping fileResponse)
        {
            _appLogger.LogInformation($"Started processing CustomDataMappingService::Update CustomDataMapping {fileResponse}");
            var fileResponseEntity = _customDataMappingMapper.MapToEntity(fileResponse);
            var fileResponseModel = await _customDataMappingRepository.UpdateCustomDataMapping(fileResponseEntity);
            var fileResponseData = _customDataMappingMapper.MapToModel(fileResponseModel);
            if (fileResponseData == null) return null;
            return fileResponseData;
        }
    }
}
