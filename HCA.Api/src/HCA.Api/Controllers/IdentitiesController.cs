using HCA.Api.Dto;
using HCA.Api.Mapper;
using HCA.Core.Services;
using HCA.Data.Entities;
using HCA.Infrastructure.Extensions;
using HCA.Infrastructure.Logger;
using HCA.Models.Enums;
using HCA.Models.MuleSoft;
using HCA.Models.SQS;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HCA.Api.Controllers
{
    [Route("api/[controller]")]
    public class IdentitiesController : Controller
    {
        private readonly IAppLogger _logger;

        private readonly IClientIdentityService _clientIdentityService;

        public IdentitiesController(IClientIdentityService clientIdentityService, IAppLogger appLogger)
        {
            _clientIdentityService = clientIdentityService;
            _logger = appLogger;
        }

        [HttpPost("dashboardData")]
        public async Task<IActionResult> GetDashboardData([FromBody] Identity filter, [FromQuery] string? currentUser, [FromQuery] int pagNumber = 0, [FromQuery] int recordsPerPage = 20)
        {
            var (searchBy, searchValue) = GetSearchFilter(filter);
            var (count, records) = await _clientIdentityService.GetAll(currentUser ?? "User Request", searchBy ?? "", searchValue ?? "", pagNumber, recordsPerPage);
            var identities = ClientIdentityDtoMapper.GetDto(records);

            var result = new PagenatedCollection<ClientIdentityDto>
            {
                RecordsCount = count,
                PageNumber = pagNumber,
                RecordsPerPage = recordsPerPage,
                Data = identities
            };

            return Ok(result);
        }


        [HttpPost("demographicSearch")]
        public async Task<IActionResult> DemographicSearch([FromBody] Identity filter, [FromQuery] string? currentUser, [FromQuery] int pagNumber = 0, [FromQuery] int recordsPerPage = 20, [FromQuery] string? processingOptions = null)
        {
            var (processType, notificationOptions) = GetProcessingOptions(processingOptions);
            var searchResult = await _clientIdentityService.DemographicSearch(filter, currentUser ?? "User Request", processType, notificationOptions);
            if (searchResult == null) return NoContent();
            return Ok(searchResult);
        }

        [HttpPut("link")]
        public async Task<IActionResult> Link([FromBody] LinkingSources value, [FromQuery] string? currentUser, [FromQuery] string? processingOptions = null)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var (processType, notificationOptions) = GetProcessingOptions(processingOptions);
            var result = await _clientIdentityService.LinkIdentities(value, currentUser ?? "User Request", processType, notificationOptions);
            if (null == result) return BadRequest("Invalid Input");
            return Ok(result);
        }

        [HttpPut("unlink")]
        public async Task<IActionResult> UnLink([FromBody] UnLinkingSources value, [FromQuery] string? currentUser, [FromQuery] string? processingOptions = null)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var (processType, notificationOptions) = GetProcessingOptions(processingOptions);
            var result = await _clientIdentityService.UnLinkIdentities(value, currentUser ?? "User Request", processType, notificationOptions);
            if (result == null) return BadRequest("Invalid Input");
            return Ok(result);
        }

        [HttpPut("merge")]
        public async Task<IActionResult> Merge([FromBody] MergingSources value, [FromQuery] string? currentUser, [FromQuery] string? processingOptions = null)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var (processType, notificationOptions) = GetProcessingOptions(processingOptions);
            var result = await _clientIdentityService.MergeIdentities(value, currentUser ?? "User Request", processType, notificationOptions);
            if (result == null) return BadRequest("Invalid Input");
            return Ok(result);
        }

        [HttpPut("unmerge")]
        public async Task<IActionResult> UnMerge([FromBody] UnMergingSources value, [FromQuery] string? currentUser, [FromQuery] string? processingOptions = null)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var (processType, notificationOptions) = GetProcessingOptions(processingOptions);
            var result = await _clientIdentityService.UnMergeIdentities(value, currentUser ?? "User Request", processType, notificationOptions);
            if (result == null) return BadRequest("Invalid Input");
            return Ok(result);
        }

        private static (ProcessType, NotificationOptions?) GetProcessingOptions(string? processingOptions)
        {
            if (processingOptions.IsEmpty()) return (ProcessType.Sync, null);
            var options = processingOptions?.SplitByChar('|');
            if (options == null) return (ProcessType.Sync, null);
            Enum.TryParse(typeof(ProcessType), options[0], true, out var processTypeObj);
            if (processTypeObj == null) return (ProcessType.Sync, null);
            var processType = (ProcessType)processTypeObj;
            if (options.Length < 3 || processType == ProcessType.Sync) return (ProcessType.Sync, null);
            var appName = options[1];
            var messageGroupId = options[2];
            return (ProcessType.Async, new NotificationOptions() { AppName = appName, MessageGroup = messageGroupId });
        }

        private static (string?, string?) GetSearchFilter(Identity filter)
        {
            var name = filter.Names.FirstOrDefault();

            if (name != null)
            {
                if (name.First.IsNotEmpty()) return ("FName", name.First);
                if (name.Last.IsNotEmpty()) return ("LName", name.Last);
            }

            var ssn = filter.Ssns.FirstOrDefault();
            if (ssn.IsNotEmpty()) return ("Ssn", ssn);

            var email = filter.Emails.FirstOrDefault();
            if (email.IsNotEmpty()) return ("Email", email);

            return (null, null);
        }
    }
}

