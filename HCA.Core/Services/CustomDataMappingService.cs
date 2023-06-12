using HCA.Core.Mapper;
using HCA.Data.Entities;
using HCA.Data.Repository;
using HCA.Infrastructure.JObjectHelper;
using HCA.Infrastructure.Logger;
using HCA.Models.Enums;
using HCA.Models.Request;
using Newtonsoft.Json.Linq;
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

        private readonly IJObjectCreator _iJObjectCreator;

        public CustomDataMappingService(ICustomDataMappingRepository customDataMappingRepository, IAppLogger appLogger,
            ICustomDataMappingMapper customDataMappingMapper, IJObjectCreator iJObjectCreator)
        {
            _customDataMappingRepository = customDataMappingRepository;
            _appLogger = appLogger;
            _customDataMappingMapper = customDataMappingMapper;
            _iJObjectCreator = iJObjectCreator;
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
            _appLogger.LogInformation($"Started processing CustomDataMappingService::GetCustomDataMappingBySourceSystem for sourceSystemName {sourceSystemName}");
            var customDataMappingEntities = await _customDataMappingRepository.GetCustomDataMappingBySourceSystem(sourceSystemName);
            if (customDataMappingEntities == null) return null;
            var customDataMappings = _customDataMappingMapper.MapToModelCollection(customDataMappingEntities);
            _appLogger.LogInformation($"Completed processing CustomDataMappingService::GetCustomDataMapping for Id {sourceSystemName}");
            return customDataMappings;
        }

        public async Task<IEnumerable<CustomDataMapping?>> GetCustomDataMappings()
        {
            _appLogger.LogInformation($"Started processing CustomDataMappingService::GetCustomDataMappings At DateTime {new DateTime()}");
            var CustomDataMappingEntity = await _customDataMappingRepository.GetAllAsync();
            if (CustomDataMappingEntity == null) return null;
            var CustomDataMappingModel = _customDataMappingMapper.MapToModelCollection(CustomDataMappingEntity);
            _appLogger.LogInformation($"Completed processing CustomDataMappingService::GetAll At DateTime {new DateTime()}");
            return CustomDataMappingModel;
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
            var customDataMappingModel = _customDataMappingMapper.MapToEntity(fileResponse);
            var customDataMappingEntity = _customDataMappingRepository.Update(customDataMappingModel);
            var UpdateCustomDataMappingModel = _customDataMappingMapper.MapToModel(customDataMappingEntity);
            if (UpdateCustomDataMappingModel == null) return null;
            return UpdateCustomDataMappingModel;
        }

        public JObject MapCustomJson(IEnumerable<CustomDataMapping?> customDataMappings, Dictionary<string, object> customData)
        {
            var result = new JObject();

            foreach (var customDataMapping in customDataMappings.OrderBy(c => c.InputIndex))
            {
                if(customDataMapping == null) continue;

                var key = customDataMapping.InputIndex.ToString();

                if (customData.ContainsKey(key))
                {
                    //parsedCustomJson.Add(customDataMapping.InputColumnName, customData[key]);
                    result.Merge(_iJObjectCreator.GetJObject("{" + $"'{customDataMapping.VeratoRequestPath}': '{customData[key]}'" + "}"));
                }
            }

            return result;
        }
    }
}
