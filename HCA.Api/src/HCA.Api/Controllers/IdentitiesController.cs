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

namespace HCA.Api.Controllers
{

    /// <summary>
    /// Provides methods for operations on client identities
    /// </summary>
    [Route("api/[controller]")]
    [Consumes("application/json")]
    [Produces("application/json")]
    public class IdentitiesController : Controller
    {
        private readonly IAppLogger _logger;

        private readonly IClientIdentityService _clientIdentityService;

        /// <summary>
        /// <see cref="IdentitiesController"/>
        /// </summary>
        /// <param name="clientIdentityService">Client identity service <see cref="IClientIdentityService"/></param>
        /// <param name="appLogger">Applicaiton logger <see cref="IAppLogger"/></param>
        public IdentitiesController(IClientIdentityService clientIdentityService, IAppLogger appLogger)
        {
            _clientIdentityService = clientIdentityService;
            _logger = appLogger;
        }

        /// <summary>
        /// Fetches the Dashboard data based on the filter condition
        /// </summary>
        /// <param name="filter">Filter condition for the data</param>
        /// <param name="pagNumber">current page number</param>
        /// <param name="recordsPerPage">Records per page</param>
        /// <param name="orderBy">Order by column nmae</param>
        /// <returns>Paginated collection of client identities <see cref="PagenatedCollection{ClientIdentityDto}"/></returns>
        [SwaggerResponse(StatusCodes.Status200OK, "List of client identities", typeof(PagenatedCollection<ClientIdentityDto>))]
        [SwaggerResponse(StatusCodes.Status401Unauthorized)]
        [SwaggerResponse(StatusCodes.Status403Forbidden)]
        [SwaggerResponse(StatusCodes.Status500InternalServerError)]
        [HcaAuthorize(Roles.ReadOnly, Roles.Admin)]
        [HttpPost("dashboardData")]
        public async Task<IActionResult> GetDashboardData([FromBody] DashboardFillter filter, [FromQuery] int pagNumber = 0, [FromQuery] int recordsPerPage = 20, [FromQuery] string orderBy = "")
        {
            var searchFilter = GetSearchFilter(filter);
            var (count, records) = await _clientIdentityService.GetAll(HttpContext.GetCurrentUser(), searchFilter, pagNumber, recordsPerPage, orderBy);
            var showSensitiveData = HttpContext.CanShowSensitiveData();
            var identities = ClientIdentityDtoMapper.GetDto(records, showSensitiveData);
            if (identities.Count > count) count = identities.Count;

            var result = new PagenatedCollection<ClientIdentityDto>
            {
                RecordsCount = count,
                PageNumber = pagNumber,
                RecordsPerPage = recordsPerPage,
                Data = identities
            };

            return Ok(result);
        }

        /// <summary>
        /// Demographic search for the client identities - calls the identity store demographic search api and returns the search results from identity provider (Verato)
        /// </summary>
        /// <param name="filter">Filter condition for search</param>
        /// <param name="pagNumber"></param>
        /// <param name="recordsPerPage"></param>
        /// <param name="processingOptions"></param>
        /// <returns></returns>
        [SwaggerResponse(StatusCodes.Status200OK, "List of client identities", typeof(PagenatedCollection<ClientIdentityDto>))]
        [SwaggerResponse(StatusCodes.Status401Unauthorized)]
        [SwaggerResponse(StatusCodes.Status403Forbidden)]
        [SwaggerResponse(StatusCodes.Status500InternalServerError)]
        [HcaAuthorize(Roles.ReadOnly, Roles.Admin)]
        [HttpPost("demographicSearch")]
        public async Task<IActionResult> DemographicSearch([FromBody] Identity filter, [FromQuery] int pagNumber = 0, [FromQuery] int recordsPerPage = 20, [FromQuery] string? processingOptions = null)
        {
            var (processType, notificationOptions) = GetProcessingOptions(processingOptions);
            var searchResult = await _clientIdentityService.DemographicSearch(filter, HttpContext.GetCurrentUser(), processType, notificationOptions);
            if (searchResult == null) return NoContent();
            return Ok(searchResult);
        }

        /// <summary>
        /// Links the 2 client identities
        /// </summary>
        /// <param name="value">Linking sources <see cref="LinkingSources" /></param>
        /// <param name="processingOptions">Processing options - indicates whether synchronous or asynchronous execution of the apis</param>
        /// <returns></returns>

        [SwaggerResponse(StatusCodes.Status200OK, "Request id for asynchronous call of the api", typeof(string))]
        [SwaggerResponse(StatusCodes.Status200OK, "Link identities response", typeof(LinkIdentitiesResponseContent))]
        [SwaggerResponse(StatusCodes.Status400BadRequest)]
        [SwaggerResponse(StatusCodes.Status401Unauthorized)]
        [SwaggerResponse(StatusCodes.Status403Forbidden)]
        [SwaggerResponse(StatusCodes.Status500InternalServerError)]
        [HcaAuthorize(Roles.Admin)]
        [HttpPut("link")]
        public async Task<IActionResult> Link([FromBody] LinkingSources value, [FromQuery] string? processingOptions = null)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var (processType, notificationOptions) = GetProcessingOptions(processingOptions);
            var result = await _clientIdentityService.LinkIdentities(value, HttpContext.GetCurrentUser(), processType, notificationOptions);
            if (null == result) return BadRequest("Invalid Input");
            return Ok(result);
        }

        /// <summary>
        /// Unlink client identiities
        /// </summary>
        /// <param name="value">Un linking sources <see cref="UnLinkingSources"/></param>
        /// <param name="processingOptions">Processing options - indicates whether synchronous or asynchronous execution of the apis</param>
        /// <returns></returns>
        [SwaggerResponse(StatusCodes.Status200OK, "Request id for asynchronous call of the api", typeof(string))]
        [SwaggerResponse(StatusCodes.Status200OK, "Un link identities response", typeof(UnLinkIdentitiesResponseContent))]
        [SwaggerResponse(StatusCodes.Status400BadRequest)]
        [SwaggerResponse(StatusCodes.Status401Unauthorized)]
        [SwaggerResponse(StatusCodes.Status403Forbidden)]
        [SwaggerResponse(StatusCodes.Status500InternalServerError)]
        [HcaAuthorize(Roles.Admin)]
        [HttpPut("unlink")]
        public async Task<IActionResult> UnLink([FromBody] UnLinkingSources value, [FromQuery] string? processingOptions = null)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var (processType, notificationOptions) = GetProcessingOptions(processingOptions);
            var result = await _clientIdentityService.UnLinkIdentities(value, HttpContext.GetCurrentUser(), processType, notificationOptions);
            if (result == null) return BadRequest("Invalid Input");
            return Ok(result);
        }

        /// <summary>
        /// Merge client identities
        /// </summary>
        /// <param name="value">Merge Sources <see cref="MergingSources"/></param>
        /// <param name="processingOptions">Processing options - indicates whether synchronous or asynchronous execution of the apis</param>
        /// <returns></returns>
        [SwaggerResponse(StatusCodes.Status200OK, "Request id for asynchronous call of the api", typeof(string))]
        [SwaggerResponse(StatusCodes.Status200OK, "Merge identities response", typeof(MergeIdentitiesResponseContent))]
        [SwaggerResponse(StatusCodes.Status400BadRequest)]
        [SwaggerResponse(StatusCodes.Status401Unauthorized)]
        [SwaggerResponse(StatusCodes.Status403Forbidden)]
        [SwaggerResponse(StatusCodes.Status500InternalServerError)]
        [HcaAuthorize(Roles.Admin)]
        [HttpPut("merge")]
        public async Task<IActionResult> Merge([FromBody] MergingSources value, [FromQuery] string? processingOptions = null)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var (processType, notificationOptions) = GetProcessingOptions(processingOptions);
            var result = await _clientIdentityService.MergeIdentities(value, HttpContext.GetCurrentUser(), processType, notificationOptions);
            if (result == null) return BadRequest("Invalid Input");
            return Ok(result);
        }

        /// <summary>
        /// Un merge client identitiess
        /// </summary>
        /// <param name="value">Un merge Sources <see cref="UnMergingSources"/></param>
        /// <param name="processingOptions">Processing options - indicates whether synchronous or asynchronous execution of the apis</param>
        /// <returns></returns>
        [SwaggerResponse(StatusCodes.Status200OK, "Request id for asynchronous call of the api", typeof(string))]
        [SwaggerResponse(StatusCodes.Status200OK, "Un Merge identities response", typeof(UnMergeIdentitiesResponseContent))]
        [SwaggerResponse(StatusCodes.Status400BadRequest)]
        [SwaggerResponse(StatusCodes.Status401Unauthorized)]
        [SwaggerResponse(StatusCodes.Status403Forbidden)]
        [SwaggerResponse(StatusCodes.Status500InternalServerError)]
        [HcaAuthorize(Roles.Admin)]
        [HttpPut("unmerge")]
        public async Task<IActionResult> UnMerge([FromBody] UnMergingSources value, [FromQuery] string? processingOptions = null)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var (processType, notificationOptions) = GetProcessingOptions(processingOptions);
            var result = await _clientIdentityService.UnMergeIdentities(value, HttpContext.GetCurrentUser(), processType, notificationOptions);
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
    /// <summary>
    /// Dashboard filter
    /// </summary>
    public class DashboardFillter
    {
        /// <summary>
        /// First Name
        /// </summary>
        /// <example></example>
        public string FirstName { get; set; }

        /// <summary>
        /// Last Name
        /// </summary>
        /// <example></example>
        public string LastName { get; set; }

        /// <summary>
        /// Email
        /// </summary>
        /// <example></example>
        public string Email { get; set; }

        /// <summary>
        /// Social Security Number
        /// </summary>
        /// <example></example>
        public string Ssn { get; set; }

        /// <summary>
        /// Email
        /// </summary>
        /// <example></example>
        public string LinkId { get; set; }

        /// <summary>
        /// Source System Ide
        /// </summary>
        /// <example></example>
        public string SourceSystemId { get; set; }
    }
}

