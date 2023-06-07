using HCA.Api.Constants;
using HCA.Api.Dto;
using HCA.Api.Extensions;
using HCA.Api.Filters;
using HCA.Api.Mapper;
using HCA.Core.Services;
using HCA.Data.Entities;
using HCA.Infrastructure.Logger;
using HCA.Models.Request;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace HCA.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomDataMappingController : ControllerBase
    {
        private readonly ICustomDataMappingService _customDataMappingService;

        private readonly IAppLogger _appLogger;

        public CustomDataMappingController(ICustomDataMappingService customDataMappingService, IAppLogger logger)
        {
            _customDataMappingService = customDataMappingService;
            _appLogger = logger;
        }

        //[HcaAuthorize(Roles.ReadOnly)]
        [HttpGet("GetCustomDataMappingBySourceSystems/{sourceSystemName}")]
        [SwaggerResponse(StatusCodes.Status200OK, "CustomDataMapping Details", typeof(CustomDataMappingDto))]
        public async Task<IActionResult> GetCustomDataMappingBySourceSystem([FromRoute] string sourceSystemName)
        {
            _appLogger.LogInformation($"Started Processing CustomDataMappingController::GetCustomDataMapping by Id:{sourceSystemName}");
            var result = await _customDataMappingService.GetCustomDataMappingBySourceSystem(sourceSystemName);
            if (null == result) return NoContent();
            var customDataMappingsDto = CustomDataMappingDtoMapper.GetListDto(result);
            return Ok(customDataMappingsDto);
        }

        //[HcaAuthorize(Roles.ReadOnly)]
        [HttpGet("GetCustomDataMappings")]
        [SwaggerResponse(StatusCodes.Status200OK, "CustomDataMappings Details", typeof(CustomDataMappingDto))]
        public async Task<IActionResult> GetCustomDataMappings()
        {
            _appLogger.LogInformation($"Started Processing CustomDataMappingController::GetCustomDataMappings for all records:{new DateTime()}");
            var result = await _customDataMappingService.GetCustomDataMappings();
            if (null == result) return NoContent();
            var customDataMappingsDto = CustomDataMappingDtoMapper.GetListDto(result);
            return Ok(customDataMappingsDto);
        }

        //[HcaAuthorize(Roles.ReadOnly)]
        [HttpPost("AddCustomDataMapping")]
        [SwaggerResponse(StatusCodes.Status200OK, "Added CustomDataMapping", typeof(CustomDataMappingDto))]
        public async Task<IActionResult> AddCustomDataMapping([FromBody] CustomDataMappingDto fileResponseDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var CustomDataMappingModel = CustomDataMappingDtoMapper.MapDtoToEntity(fileResponseDto);
            var result = await _customDataMappingService.AddCustomDataMapping(CustomDataMappingModel);
            if (null == result) return BadRequest("Invalid Input");
            return Ok(result);
        }

        //[HcaAuthorize(Roles.ReadOnly)]
        [HttpPut("UpdateCustomDataMapping")]
        [SwaggerResponse(StatusCodes.Status200OK, "Updated CustomDataMapping", typeof(CustomDataMappingDto))]
        public async Task<IActionResult> UpdateCustomDataMapping([FromBody] CustomDataMappingDto fileResponseDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var CustomDataMappingModel = CustomDataMappingDtoMapper.MapDtoToEntity(fileResponseDto);
            var result = await _customDataMappingService.UpdateCustomDataMapping(CustomDataMappingModel);
            if (null == result) return BadRequest("Invalid Input");
            return Ok(result);
        }

        [HttpPost("RemoveCustomDataMapping/{id}")]
        public async Task<IActionResult> RemoveCustomDataMapping([FromRoute] int id)
        {
            await _customDataMappingService.RemoveCustomDataMapping(id);
            return Ok();
        }
    }
}
