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
using HCA.Models.Request;
using HCA.Models.Request.DOH;
using HCA.Models.SQS;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Net;
using System.Linq;
using Newtonsoft.Json;

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

        //private readonly ISourceSystemValidator _sourceSystemValidator;


        /// <summary>
        /// <see cref="IdentitiesController"/>
        /// </summary>
        /// <param name="clientIdentityService">Client identity service <see cref="IClientIdentityService"/></param>
        /// <param name="appLogger">Applicaiton logger <see cref="IAppLogger"/></param>
        public IdentitiesController(IClientIdentityService clientIdentityService, IAppLogger appLogger)
        {
            _clientIdentityService = clientIdentityService;
            //_sourceSystemValidator = sourceSystemValidator;
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
        /// Demographics Search web service is intended to retrieve one or more identities from identity provider (Verato) that are potential matches to the search input criteria.
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
        /// Demographic query  retrieves the single matching identity from identity provider (Verato) that matches the demographic data provided in the web service request.
        /// </summary>
        /// <param name="filter">Filter condition for search</param>
        /// <param name="processingOptions"></param>
        /// <returns></returns>
        [SwaggerResponse(StatusCodes.Status200OK, "Get client identity response", typeof(ClientIdentityDto))]
        [SwaggerResponse(StatusCodes.Status401Unauthorized)]
        [SwaggerResponse(StatusCodes.Status403Forbidden)]
        [SwaggerResponse(StatusCodes.Status500InternalServerError)]
        [HcaAuthorize(Roles.ReadOnly, Roles.Admin)]
        [HttpPost("DemographicQuery")]
        public async Task<IActionResult> DemographicQuery([FromBody] Identity filter, [FromQuery] string? processingOptions = null)
        {
            var (processType, notificationOptions) = GetProcessingOptions(processingOptions);
            var searchResult = await _clientIdentityService.DemographicQuery(filter, HttpContext.GetCurrentUser(), processType, notificationOptions);
            if (searchResult == null) return NoContent();
            return Ok(searchResult);
        }

        /// <summary>
        /// PostIdentity web service adds or updates your customer records into identity provider (Verato)
        /// </summary>
        /// <param name="filter">Filter condition for search</param>
        /// <param name="processingOptions"></param>
        /// <returns></returns>
        [SwaggerResponse(StatusCodes.Status200OK, "Post client identity response", typeof(ClientIdentityDto))]
        [SwaggerResponse(StatusCodes.Status401Unauthorized)]
        [SwaggerResponse(StatusCodes.Status403Forbidden)]
        [SwaggerResponse(StatusCodes.Status500InternalServerError)]
        [HcaAuthorize(Roles.ReadOnly, Roles.Admin)]
        [HttpPost("post")]
        public async Task<IActionResult> PostIdentity([FromBody] IEnumerable<ClientIdentityRequest> filter, [FromQuery] string? processingOptions = null)
        {
            var (processType, notificationOptions) = GetProcessingOptions(processingOptions);

            var sourceSystemNames = filter.Select(c => c.SourceSystemName).ToList();
            //var isValid = await _sourceSystemValidator.ValidateSourceSystem(HttpContext, sourceSystemNames);

            //if (!isValid) return BadRequest("Cannot update data for the provided source system");

            var searchResult = await _clientIdentityService.PostIdentities(filter, HttpContext.GetCurrentUser() ?? String.Empty, processType, notificationOptions);
            if (searchResult == null) return NoContent();
            return Ok(searchResult);
        }

        /// <summary>
        /// LinkIdentities web service is used to force two customer source records (each identified by its Source + Native ID) to be linked together in a common Link ID.
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
        /// UnlinkIdentities web service is used to force apart two customer source records (each identified by its Source + Native ID) that had been matched into the same Link ID.
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
        /// MergeIdentities web service is used to force two customer source records (each identified by its Source + Native ID) to be linked together in a common Link ID and convert one of the customer source records into a retired/merged state.
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
        /// UnmergeIdentities web service is used to re-activate a previously-merged source record and force it apart from its current Link ID.
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

        #region 'DOH_Related_Code'


        /// <summary>
        /// Demographic query  retrieves the single matching identity from identity provider (Verato) that matches the demographic data provided in the web service request.
        /// </summary>
        /// <param name="filter">Filter condition for search</param>
        /// <param name="processingOptions"></param>
        /// <returns></returns>
        [SwaggerResponse(StatusCodes.Status200OK, "Get client identity response", typeof(DOH_DemographicQueryRequest))]
        [SwaggerResponse(StatusCodes.Status401Unauthorized)]
        [SwaggerResponse(StatusCodes.Status403Forbidden)]
        [SwaggerResponse(StatusCodes.Status500InternalServerError)]
        [HcaAuthorize(Roles.ReadOnly, Roles.Admin)]
        [HttpPost("DOH-demographicQuery")]
        public async Task<IActionResult> DOH_DemographicQuery([FromBody] DOH_DemographicQueryRequest filter, [FromQuery] string? processingOptions = null)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var (processType, notificationOptions) = GetProcessingOptions(processingOptions);
            //var responseIdentityFormatNames = filter.content.responseIdentityFormatNames.ToString();
            var searchResult = await _clientIdentityService.DOH_DemographicQuery(filter, HttpContext.GetCurrentUser(), processType, notificationOptions);
            //    if (searchResult == null) return NoContent();
            return Ok(searchResult);
        }

        /// <summary>
        /// Demographics Search web service is intended to retrieve one or more identities from identity provider (Verato) that are potential matches to the search input criteria.
        /// </summary>
        /// <param name="filter">Filter condition for search</param>
        /// <param name="processingOptions"></param>
        /// <returns></returns>
        [SwaggerResponse(StatusCodes.Status200OK, "List of client identities", typeof(PagenatedCollection<DOH_DemographicsSearchRequest>))]
        [SwaggerResponse(StatusCodes.Status400BadRequest)]
        [SwaggerResponse(StatusCodes.Status401Unauthorized)]
        [SwaggerResponse(StatusCodes.Status403Forbidden)]
        [SwaggerResponse(StatusCodes.Status500InternalServerError)]
        [HcaAuthorize(Roles.ReadOnly, Roles.Admin)]
        [HttpPost("DOH-demographicSearch")]
        public async Task<IActionResult> DOH_DemographicSearch([FromBody] DOH_DemographicsSearchRequest filter,  [FromQuery] string? processingOptions = null)    //[FromQuery] int pagNumber = 0, [FromQuery] int recordsPerPage = 20,
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var (processType, notificationOptions) = GetProcessingOptions(processingOptions);
            var searchResult = await _clientIdentityService.DOH_DemographicSearch(filter, HttpContext.GetCurrentUser(), processType, notificationOptions);
            //if (searchResult == null) return NoContent();
            return Ok(searchResult);
        }


        /// <summary>
        /// PostIdentity web service adds or updates your customer records into identity provider (Verato)
        /// </summary>
        /// <param name="filter">Filter condition for search</param>
        /// <param name="processingOptions"></param>
        /// <returns></returns>
        [SwaggerResponse(StatusCodes.Status200OK, "Post client identity response", typeof(DOH_DemographicQueryRequest))]
        [SwaggerResponse(StatusCodes.Status400BadRequest)]
        [SwaggerResponse(StatusCodes.Status401Unauthorized)]
        [SwaggerResponse(StatusCodes.Status403Forbidden)]
        [SwaggerResponse(StatusCodes.Status500InternalServerError)]
        [HcaAuthorize(Roles.ReadOnly, Roles.Admin)]
        [HttpPost("DOH-post")]
        public async Task<IActionResult> DOH_PostIdentity([FromBody] DOH_PostClientIdentityRequest request, [FromQuery] string? processingOptions = null)
        {
            //As not going with Asyn logic removed this and passing nulls -- Naresh 2024-02-01
            //var (processType, notificationOptions) = GetProcessingOptions(processingOptions);
            if (!ModelState.IsValid) return BadRequest(ModelState);
            //if (request == null) return BadRequest("Invalid Request");

            var searchResult = await _clientIdentityService.DOH_PostIdentities(request, HttpContext.GetCurrentUser() ?? String.Empty, ProcessType.Sync, null);
            //if (searchResult == null) return NoContent();
            return Ok(searchResult);
        }

        /// <summary>
        /// LinkIdentities web service is used to force two customer source records (each identified by its Source + Native ID) to be linked together in a common Link ID.
        /// </summary>
        /// <param name="value">"DOH_LinkingSources"</param>
        /// <param name="processingOptions"></param>
        /// <returns></returns>
        [SwaggerResponse(StatusCodes.Status200OK, "Request id for asynchronous call of the api", typeof(string))]
        [SwaggerResponse(StatusCodes.Status200OK, "Link identities response", typeof(LinkIdentitiesResponseContent))]
        [SwaggerResponse(StatusCodes.Status400BadRequest)]
        [SwaggerResponse(StatusCodes.Status401Unauthorized)]
        [SwaggerResponse(StatusCodes.Status403Forbidden)]
        [SwaggerResponse(StatusCodes.Status500InternalServerError)]
        [HcaAuthorize(Roles.Admin)]
        [HttpPut("DOH-link")]
        public async Task<IActionResult> DOH_Link([FromBody] DOH_LinkingSources value, [FromQuery] string? processingOptions = null)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var (processType, notificationOptions) = GetProcessingOptions(processingOptions);
            var result = await _clientIdentityService.DOH_LinkIdentities(value, HttpContext.GetCurrentUser(), processType, notificationOptions);
            //if (null == result) return BadRequest("Invalid Input");
            return Ok(result);
        }

        /// <summary>
        /// UnlinkIdentities web service is used to force apart two customer source records (each identified by its Source + Native ID) that had been matched into the same Link ID.
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
        [HttpPut("DOH-unlink")]
        public async Task<IActionResult> DOH_UnLink([FromBody] DOH_UnLinkingSources value, [FromQuery] string? processingOptions = null)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var (processType, notificationOptions) = GetProcessingOptions(processingOptions);
            var result = await _clientIdentityService.DOH_UnLinkIdentities(value, HttpContext.GetCurrentUser(), processType, notificationOptions);
            //if (result == null) return BadRequest("Invalid Input");
            return Ok(result);
        }

        /// <summary>
        /// MergeIdentities web service is used to force two customer source records (each identified by its Source + Native ID) to be linked together in a common Link ID and convert one of the customer source records into a retired/merged state.
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
        [HttpPut("DOH-merge")]
        public async Task<IActionResult> DOH_Merge([FromBody] DOH_MergingSources value, [FromQuery] string? processingOptions = null)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var (processType, notificationOptions) = GetProcessingOptions(processingOptions);
            var result = await _clientIdentityService.DOH_MergeIdentities(value, HttpContext.GetCurrentUser(), processType, notificationOptions);
            //if (result == null) return BadRequest("Invalid Input");
            return Ok(result);
        }

        /// <summary>
        /// UnmergeIdentities web service is used to re-activate a previously-merged source record and force it apart from its current Link ID.
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
        [HttpPut("DOH-unmerge")]
        public async Task<IActionResult> DOH_UnMerge([FromBody] DOH_UnMergingSources value, [FromQuery] string? processingOptions = null)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var (processType, notificationOptions) = GetProcessingOptions(processingOptions);
            var result = await _clientIdentityService.DOH_UnMergeIdentities(value, HttpContext.GetCurrentUser(), processType, notificationOptions);
            //if (result == null) return BadRequest("Invalid Input");
            return Ok(result);
        }


        /// <summary>
        /// DeleteSourceIdentity web service is used to physically delete one of your source records from your instance of identity provider (Verato)
        /// </summary>
        /// <param name="value">Delete Sources <see cref="DOH_DeleteClientIdentityRequest"/></param>
        /// <param name="processingOptions">Processing options - indicates whether synchronous or asynchronous execution of the apis</param>
        /// <returns></returns>
        [SwaggerResponse(StatusCodes.Status200OK, "Request id for asynchronous call of the api", typeof(string))]
        [SwaggerResponse(StatusCodes.Status200OK, "Delete identities response", typeof(MergeIdentitiesResponseContent))]
        [SwaggerResponse(StatusCodes.Status400BadRequest)]
        [SwaggerResponse(StatusCodes.Status401Unauthorized)]
        [SwaggerResponse(StatusCodes.Status403Forbidden)]
        [SwaggerResponse(StatusCodes.Status500InternalServerError)]
        [HcaAuthorize(Roles.Admin)]
        [HttpDelete("DOH-delete")]
        public async Task<IActionResult> DOH_Delete([FromBody] DOH_DeleteClientIdentityRequest value, [FromQuery] string? processingOptions = null)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var (processType, notificationOptions) = GetProcessingOptions(processingOptions);
            var result = await _clientIdentityService.DOH_DeleteSourceIdentity(value, HttpContext.GetCurrentUser(), processType, notificationOptions);
            //if (result == null) return BadRequest("Invalid Input");
            return Ok(result);
        }


        #endregion
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

