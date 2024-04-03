using HCA.Api.Constants;
using HCA.Api.Dto;
using HCA.Api.Filters;
using HCA.Api.Mapper;
using HCA.Core.Services;
using HCA.Infrastructure.Logger;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace HCA.Api.Controllers
{
    [Route("api/[controller]")]
    public class FilesController : Controller
    {
        private readonly IFileRequestService _fileRequestService;

        private readonly IAppLogger _appLogger;

        public FilesController(IFileRequestService fileRequestService, IAppLogger logger)
        {
            _fileRequestService = fileRequestService;
            _appLogger = logger;
        }

        [HcaAuthorize(Roles.ReadOnly)]
        [HttpGet("{trackingId}")]
        [SwaggerResponse(StatusCodes.Status200OK, "File Detailss", typeof(FileRequestDto))]
        public async Task<IActionResult> GetFileRequestStatusByTrackingId([FromRoute] string trackingId)
        {
            _appLogger.LogInformation($"Started Processing FilesController::GetFileRequestStatusByTrackingId for trackingId: {trackingId}");
            var result = await _fileRequestService.GetByTrackingId(trackingId);
            if (null == result) return NoContent();
            var fileRequestDto = FileRequestDtoMapper.GetDto(result);
            return Ok(fileRequestDto);
        }

        [HcaAuthorize(Roles.ReadOnly)]
        [HttpGet("GetByName/{fileName}")]
        [SwaggerResponse(StatusCodes.Status200OK, "File Detailss", typeof(FileRequestDto))]
        public async Task<IActionResult> GetFileRequestStatusByName([FromRoute] string fileName)
        {
            _appLogger.LogInformation($"Started Processing FilesController::GetFileRequestStatusByName for fileName: {fileName}");
            var result = await _fileRequestService.GetByFileName(fileName);
            if (null == result) return NoContent();
            var fileRequestDto = FileRequestDtoMapper.GetDto(result);
            return Ok(fileRequestDto);
        }
    }
}

