using HCA.Api.Constants;
using HCA.Api.Dto;
using HCA.Api.Extensions;
using HCA.Api.Filters;
using HCA.Api.Mapper;
using HCA.Core.Services;
using HCA.Data.Entities;
using HCA.Infrastructure.Extensions;
using HCA.Infrastructure.Logger;
using HCA.Models.Enums;
using HCA.Models.MuleSoft;
using HCA.Models.MuleSoft.Response;
using HCA.Models.SQS;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Net;
using System.Security.Claims;

namespace HCA.Api.Controllers
{

    /// <summary>
    /// Provides methods for operations on client identities
    /// </summary>
    [Route("api/[controller]")]
    [Consumes("application/json")]
    [Produces("application/json")]
    public class ReportsController : Controller
    {
        private readonly IAppLogger _logger;

        private readonly IReportsService _reportsService;


        /// <summary>
        /// <see cref="IdentitiesController"/>
        /// </summary>
        /// <param name="reportsService">Reports service <see cref="IReportsService"/></param>
        /// <param name="appLogger">Applicaiton logger <see cref="IAppLogger"/></param>
        public ReportsController(IReportsService reportsService, IAppLogger appLogger)
        {
            _reportsService = reportsService;
            _logger = appLogger;
        }

        /// <summary>
        /// Fetches the Dashboard data based on the filter condition
        /// </summary>
        /// <param name="linkId">Link Id</param>
        /// <param name="pagNumber">current page number</param>
        /// <param name="recordsPerPage">Records per page</param>
        /// <param name="orderBy">Order by column nmae</param>
        /// <returns>Paginated collection of client identities <see cref="PagenatedCollection{ClientIdentityDto}"/></returns>
        [SwaggerResponse(StatusCodes.Status200OK, "List of client identities", typeof(PagenatedCollection<ClientIdentityDto>))]
        [SwaggerResponse(StatusCodes.Status401Unauthorized)]
        [SwaggerResponse(StatusCodes.Status403Forbidden)]
        [SwaggerResponse(StatusCodes.Status500InternalServerError)]
        //[HcaAuthorize(Roles.ReadOnly, Roles.Admin)]
        [HttpGet("matchingLinkIds")]
        public async Task<IActionResult> SameLinkIdWithDifferentSourceIds([FromQuery] string linkId = "", [FromQuery] int pagNumber = 0, [FromQuery] int recordsPerPage = 20, [FromQuery] string orderBy = "")
        {
            var (count, records) = await _reportsService.GetClientIdentityGroupedByLinkId(linkId, pagNumber, recordsPerPage, orderBy);

            var result = new Dictionary<string, IEnumerable<ClientIdentityDto>>();
            
            foreach (var record in records)
            {
                result.Add(record.Key, ClientIdentityDtoMapper.MapToReportDto(record.Value));
            }


            var returnValue = new PagenatedDictionary<Dictionary<string, IEnumerable<ClientIdentityDto>>>
            {
                RecordsCount = count,
                PageNumber = pagNumber,
                RecordsPerPage = recordsPerPage,
                Data = result
            };

            return Ok(returnValue);
        }

        /// <summary>
        /// Fetches the Dashboard data based on the filter condition
        /// </summary>
        /// <param name="pagNumber">current page number</param>
        /// <param name="recordsPerPage">Records per page</param>
        /// <param name="userNameFilter">User name filter</param>
        /// <param name="orderBy">Order by column nmae</param>
        /// <returns>Paginated collection of client identities <see cref="PagenatedCollection{ClientIdentityDto}"/></returns>
        [SwaggerResponse(StatusCodes.Status200OK, "List of client identities", typeof(PagenatedCollection<ClientIdentityDto>))]
        [SwaggerResponse(StatusCodes.Status401Unauthorized)]
        [SwaggerResponse(StatusCodes.Status403Forbidden)]
        [SwaggerResponse(StatusCodes.Status500InternalServerError)]
        [HcaAuthorize(Roles.ReadOnly, Roles.Admin)]
        [HttpGet("userActionsReports")]
        public async Task<IActionResult> GetUserActionsReport([FromQuery] string userNameFilter = "", [FromQuery] int pagNumber = 0, [FromQuery] int recordsPerPage = 20, [FromQuery] string orderBy = "")
        {
            var user = HttpContext.User;
            var name = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var isAdmin = user.IsInRole(Roles.Admin);
            if (!isAdmin) userNameFilter = name ?? "";
            var (count, records) = await _reportsService.GetUserRequests(userNameFilter, pagNumber, recordsPerPage, orderBy);
            var userActions = UserRequestDtoMapper.GetDto(records);

            var result = new PagenatedCollection<UserRequestDto>
            {
                RecordsCount = count,
                PageNumber = pagNumber,
                RecordsPerPage = recordsPerPage,
                Data = userActions
            };

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

        private static Dictionary<string, string> GetSearchFilter(DashboardFillter filter)
        {
            var searchFilter = new Dictionary<string, string>();
            if (filter.FirstName.IsNotEmpty()) searchFilter.Add("FName", filter.FirstName);
            if (filter.LastName.IsNotEmpty()) searchFilter.Add("LName", filter.LastName);
            if (filter.Ssn.IsNotEmpty()) searchFilter.Add("Ssn", filter.Ssn);
            if (filter.Email.IsNotEmpty()) searchFilter.Add("Email", filter.Email);
            if (filter.LinkId.IsNotEmpty()) searchFilter.Add("MpiLinkId", filter.LinkId);
            if (filter.SourceSystemId.IsNotEmpty()) searchFilter.Add("SourceId", filter.SourceSystemId);
            return searchFilter;
        }
    }
}

