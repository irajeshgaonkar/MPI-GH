using HCA.Api.Mapper;
using HCA.Core.Services;
using HCA.Infrastructure.Logger;
using Microsoft.AspNetCore.Mvc;


namespace HCA.Api.Controllers
{
    [Route("api/[controller]")]
    public class UserController : Controller
    {
        private readonly IUserRequestService _userRequestService;

        private readonly IUserModifyRecordsService _userModifyRecordsService;

        private readonly IAppLogger _appLogger;

        public UserController(IUserRequestService userRequestService, IUserModifyRecordsService userModifyRecordsService,
            IAppLogger logger)
        {
            _userModifyRecordsService = userModifyRecordsService;
            _userRequestService = userRequestService;
            _appLogger = logger;
        }

        [HttpGet("{trackingId}")]
        public async Task<IActionResult> GetFileRequestStatusByTrackingId([FromRoute] string trackingId)
        {
            _appLogger.LogInformation($"Started Processing FilesController::GetFileRequestStatusByTrackingId for trackingId: {trackingId}");
            var result = await _userRequestService.GetByTrackingId(trackingId);
            if (null == result) return NoContent();
            var fileRequestDto = UserRequestDtoMapper.GetDto(result);
            return Ok(fileRequestDto);
        }


        //[HttpPost("moveToModify/{userName}/{id}")]
        //public async Task<IActionResult> MoveToModify([FromRoute] int id, [FromRoute] string userName)
        //{
        //    await _userModifyRecordsService.MoveToModify(userName, id);
        //    return Ok();
        //}

        //[HttpPost("removeModify/{userName}/{id}")]
        //public async Task<IActionResult> RemoveFromModify([FromRoute] int id, [FromRoute] string userName)
        //{
        //    await _userModifyRecordsService.RemoveModify(userName, id);
        //    return Ok();
        //}

        //[HttpGet("modifyData/{currentUser}")]
        //public async Task<IActionResult> GetModifyData([FromRoute] string currentUser)
        //{
        //    var records = await _userModifyRecordsService.GetUserRecords(currentUser);
        //    var identities = ClientIdentityDtoMapper.GetDto(records);
        //    return Ok(identities);
        //}
    }
}

