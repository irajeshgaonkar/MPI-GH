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
            var customDataMappingEntity = _customDataMappingMapper.MapToEntity(customData);
            _customDataMappingRepository.AddAsync(customDataMappingEntity);
            return await Task.FromResult(customData);
        }

        public async Task<IEnumerable<CustomDataMapping?>> GetCustomDataMappingBySourceSystem(string sourceSystemName)
        {
            _appLogger.LogInformation($"Started processing CustomDataMappingService::GetCustomDataMapping for Id {sourceSystemName}");
            var customDataMappingEntities = await _customDataMappingRepository.GetCustomDataMappingBySourceSystem(sourceSystemName);
            if (customDataMappingEntities == null) 
                return null;
            var customDataMappings = _customDataMappingMapper.MapToModelCollection(customDataMappingEntities);
            _appLogger.LogInformation($"Completed processing CustomDataMappingService::GetCustomDataMapping for Id {sourceSystemName}");
            return customDataMappings;
        }

        public async Task<IEnumerable<CustomDataMapping?>> GetCustomDataMappings()
        {

            _appLogger.LogInformation($"Started processing CustomDataMappingService::GetCustomDataMappings for Id {new DateTime()}");
            var fileResponseEntity = await _customDataMappingRepository.GetAllAsync();
            if (fileResponseEntity == null) return null;
            var fileResponse = _customDataMappingMapper.MapToModelCollection(fileResponseEntity);
            _appLogger.LogInformation($"Completed processing CustomDataMappingService::GetById for Id {new DateTime()}");
            return fileResponse;
        }

        public async Task RemoveCustomDataMapping(int id)
        {
            //var entity = await _customDataMappingRepository.GetSingleAsync(m => m.Id == id);

            //if (entity != null)
            //{
            //    _customDataMappingRepository.Delete(entity);
            //}
            //return null;
        }

        public async Task<CustomDataMapping?> UpdateCustomDataMapping(CustomDataMapping fileResponse)
        {
            //_appLogger.LogInformation($"Started processing CustomDataMappingService::Update CustomDataMapping {fileResponse}");
            //var fileResponseEntity = _customDataMappingMapper.MapToEntity(fileResponse);
            //var fileResponseModel = await _customDataMappingRepository.UpdateCustomDataMapping(fileResponseEntity);
            //var fileResponseData = _customDataMappingMapper.MapToModel(fileResponseModel);
            //if (fileResponseData == null) return null;
            //return fileResponseData;
            return null;
        }
    }
}
