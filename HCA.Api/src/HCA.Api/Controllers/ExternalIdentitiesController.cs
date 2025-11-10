using HCA.Api.Dto;
using HCA.Api.Extensions;
using HCA.Api.Filters;
using HCA.Core.Services;
using HCA.Infrastructure.Extensions;
using HCA.Infrastructure.Logger;
using HCA.Models.Enums;
using HCA.Models.Logging;
using HCA.Models.Request;
using HCA.Models.Request.DOH;
using HCA.Models.Response;
using HCA.Models.SQS;
using HCA.Models.Verato;
using HCA.Models.Verato.Response;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Newtonsoft.Json;
using Swashbuckle.AspNetCore.Annotations;

namespace HCA.Api.Controllers
{
    [Route("api/v1")]
    [Consumes("application/json")]
    [Produces("application/json")]
    public class ExternalIdentitiesController(IAppLogger logger, IClientIdentityService clientIdentityService) : Controller
    {
        private readonly IAppLogger _logger = logger;

        private readonly IClientIdentityService _clientIdentityService = clientIdentityService;

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
        [ServiceFilter(typeof(IPValidationFilter))]
        [ServiceFilter(typeof(ValidateIdentityFilter))]
        [HttpPost("demographicsQuery")]
        public async Task<IActionResult> DemographicsQueryAsync([FromBody] DOH_DemographicQueryRequest filter, [FromQuery] string? processingOptions = null)
        {
            if (!ModelState.IsValid)
            {
                var errorMessage = GetErrorMessages(ModelState);

                var exceptionCustomProperties = new ExceptionCustomProperties
                {
                    User = HttpContext.GetCurrentUser(),
                    Agency = filter.Agency,
                    Role = HttpContext.GetUserRoles(),
                    FunctionName = nameof(DemographicsQueryAsync),
                    ErrorMessage = String.Join(",", errorMessage),
                    ErrorCode = "400"
                };
                var errorLogItem = new LogItem()
                {
                    Name = $"{Models.Logging.Constants.LogPrefix_API}{nameof(DemographicsQueryAsync)}-Failed",
                    TrackingId = filter.Trackingid,
                    Layer = ServiceLayer.API.ToString(),
                    ExceptionCustomProperties = exceptionCustomProperties
                };

                _logger.LogCritical(JsonConvert.SerializeObject(errorLogItem));

                return BuildOkObjectResultWith400Error(errorMessage, filter.Trackingid);
            }
            if (HttpContext.Items["SourceSystem"] != null)
            {
                filter.SourceSystem = HttpContext?.Items["SourceSystem"]?.ToString();
            }

            var (processType, notificationOptions) = GetProcessingOptions(processingOptions);
            //var responseIdentityFormatNames = filter.content.responseIdentityFormatNames.ToString();
            var searchResult = await _clientIdentityService.DOH_DemographicQuery(filter, HttpContext?.GetCurrentUser() ?? string.Empty, processType, notificationOptions);
            //    if (searchResult == null) return NoContent();
            return Ok(searchResult);
        }

        /// <summary>
        /// Asynchronously checks if an identity exists based on the provided request data.
        /// and configured allowed systems with data sharing
        /// It then returns an appropriate response based on the result.
        /// </summary>
        /// <param name="request">The request object containing the identity check details (from the request body)</param>
        /// <param name="processingOptions"></param>
        /// <returns></returns>
        [SwaggerResponse(StatusCodes.Status200OK, "Get identity exists response", typeof(IdentityExistsResponse))]
        [SwaggerResponse(StatusCodes.Status401Unauthorized)]
        [SwaggerResponse(StatusCodes.Status403Forbidden)]
        [SwaggerResponse(StatusCodes.Status500InternalServerError)]
        [ServiceFilter(typeof(IPValidationFilter))]
        [ServiceFilter(typeof(ValidateIdentityFilter))]
        [HttpPost("identityExists")]
        public async Task<IActionResult> IdentityExistsAsync([FromBody] IdentityExistsRequest request, [FromQuery] string? processingOptions = null)
        {
            if (!ModelState.IsValid)
            {
                var errorMessage = GetErrorMessages(ModelState);

                var exceptionCustomProperties = new ExceptionCustomProperties
                {
                    User = HttpContext.GetCurrentUser(),
                    Agency = request.Agency,
                    Role = HttpContext.GetUserRoles(),
                    FunctionName = nameof(IdentityExistsAsync),
                    ErrorMessage = String.Join(",", errorMessage),
                    ErrorCode = "400"
                };
                var errorLogItem = new LogItem()
                {
                    Name = $"{Models.Logging.Constants.LogPrefix_API}{nameof(IdentityExistsAsync)}-Failed",
                    TrackingId = request.TrackingId,
                    Layer = ServiceLayer.API.ToString(),
                    ExceptionCustomProperties = exceptionCustomProperties
                };

                _logger.LogCritical(JsonConvert.SerializeObject(errorLogItem));

                return BuildOkObjectResultWith400Error(errorMessage, request.TrackingId);
            }
            if (HttpContext.Items["SourceSystem"] != null)
            {
                request.SourceSystem = HttpContext.Items["SourceSystem"]?.ToString();
            }

            var (processType, notificationOptions) = GetProcessingOptions(processingOptions);

            var identityExistsResult = await _clientIdentityService.IdentityExistsAsync(request, HttpContext.GetCurrentUser(), notificationOptions);

            return Ok(identityExistsResult);
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
        [ServiceFilter(typeof(IPValidationFilter))]
        [ServiceFilter(typeof(ValidateIdentityFilter))]
        [HttpPost("demographicsSearch")]
        public async Task<IActionResult> DemographicsSearchAsync([FromBody] DOH_DemographicsSearchRequest filter, [FromQuery] string? processingOptions = null)    //[FromQuery] int pagNumber = 0, [FromQuery] int recordsPerPage = 20,
        {
            if (!ModelState.IsValid)
            {
                var errorMessage = GetErrorMessages(ModelState);

                var exceptionCustomProperties = new ExceptionCustomProperties
                {
                    User = HttpContext.GetCurrentUser(),
                    Agency = filter.Agency,
                    Role = HttpContext.GetUserRoles(),
                    FunctionName = nameof(DemographicsSearchAsync),
                    ErrorMessage = String.Join(",", errorMessage),
                    ErrorCode = "400"
                };
                var errorLogItem = new LogItem()
                {
                    Name = $"{Models.Logging.Constants.LogPrefix_API}{nameof(DemographicsSearchAsync)}-Failed",
                    TrackingId = filter.Trackingid,
                    Layer = ServiceLayer.API.ToString(),
                    ExceptionCustomProperties = exceptionCustomProperties
                };

                _logger.LogCritical(JsonConvert.SerializeObject(errorLogItem));

                return BuildOkObjectResultWith400Error(errorMessage, filter.Trackingid);
            }
            if (HttpContext.Items["SourceSystem"] != null)
            {
                filter.SourceSystem = HttpContext?.Items["SourceSystem"]?.ToString();
            }
            var (processType, notificationOptions) = GetProcessingOptions(processingOptions);
            var searchResult = await _clientIdentityService.DOH_DemographicSearch(filter, HttpContext?.GetCurrentUser() ?? string.Empty, processType, notificationOptions);
            //if (searchResult == null) return NoContent();
            return Ok(searchResult);
        }


        /// <summary>
        /// PostIdentity web service adds or updates your customer records into identity provider (Verato)
        /// </summary>
        /// <param name="request"></param>
        /// <param name="processingOptions"></param>
        /// <returns></returns>
        [SwaggerResponse(StatusCodes.Status200OK, "Post client identity response", typeof(DOH_DemographicQueryRequest))]
        [SwaggerResponse(StatusCodes.Status400BadRequest)]
        [SwaggerResponse(StatusCodes.Status401Unauthorized)]
        [SwaggerResponse(StatusCodes.Status403Forbidden)]
        [SwaggerResponse(StatusCodes.Status500InternalServerError)]
        [ServiceFilter(typeof(IPValidationFilter))]
        [ServiceFilter(typeof(ValidateIdentityFilter))]
        [HttpPost("postIdentity")]
        public async Task<IActionResult> PostIdentityAsync([FromBody] DOH_PostClientIdentityRequest request, [FromQuery] string? processingOptions = null)
        {
            //As not going with Asyn logic removed this and passing nulls -- Naresh 2024-02-01
            //var (processType, notificationOptions) = GetProcessingOptions(processingOptions);
            if (!ModelState.IsValid)
            {
                var errorMessage = GetErrorMessages(ModelState);
                var exceptionCustomProperties = new ExceptionCustomProperties
                {
                    User = HttpContext.GetCurrentUser(),
                    Agency = request.Agency,
                    Role = HttpContext.GetUserRoles(),
                    FunctionName = nameof(PostIdentityAsync),
                    ErrorMessage = String.Join(",", errorMessage),
                    ErrorCode = "400"
                };
                var errorLogItem = new LogItem()
                {
                    Name = $"{Models.Logging.Constants.LogPrefix_API}{nameof(PostIdentityAsync)}-Failed",
                    TrackingId = request.TrackingId,
                    Layer = ServiceLayer.API.ToString(),
                    ExceptionCustomProperties = exceptionCustomProperties
                };

                _logger.LogCritical(JsonConvert.SerializeObject(errorLogItem));

                return BuildOkObjectResultWith400Error(errorMessage, request.TrackingId);
            }
            if (HttpContext.Items["SourceSystem"] != null)
            {
                request.SourceSystem = HttpContext?.Items["SourceSystem"]?.ToString();
            }
            //if (request == null) return BadRequest("Invalid Request");

            var searchResult = await _clientIdentityService.DOH_PostIdentities(request, HttpContext?.GetCurrentUser() ?? String.Empty, ProcessType.Sync, null);
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
        [ServiceFilter(typeof(IPValidationFilter))]
        [HttpPost("linkIdentities")]
        public async Task<IActionResult> LinkIdentitiesAsync([FromBody] DOH_LinkingSources value, [FromQuery] string? processingOptions = null)
        {
            if (!ModelState.IsValid)
            {
                var errorMessage = GetErrorMessages(ModelState);

                var exceptionCustomProperties = new ExceptionCustomProperties
                {
                    User = HttpContext.GetCurrentUser(),
                    Agency = value.Agency,
                    Role = HttpContext.GetUserRoles(),
                    FunctionName = nameof(LinkIdentitiesAsync),
                    ErrorMessage = String.Join(",", errorMessage),
                    ErrorCode = "400"
                };
                var errorLogItem = new LogItem()
                {
                    Name = $"{Models.Logging.Constants.LogPrefix_API}{nameof(LinkIdentitiesAsync)}-Failed",
                    TrackingId = value.TrackingId,
                    Layer = ServiceLayer.API.ToString(),
                    ExceptionCustomProperties = exceptionCustomProperties
                };

                _logger.LogCritical(JsonConvert.SerializeObject(errorLogItem));

                return BuildOkObjectResultWith400Error(errorMessage, value.TrackingId);
            }
            if (HttpContext.Items["SourceSystem"] != null)
            {
                value.SourceSystem = HttpContext?.Items["SourceSystem"]?.ToString();
            }
            var (processType, notificationOptions) = GetProcessingOptions(processingOptions);
            var result = await _clientIdentityService.DOH_LinkIdentities(value, HttpContext?.GetCurrentUser() ?? string.Empty, processType, notificationOptions);
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
        [ServiceFilter(typeof(IPValidationFilter))]
        [HttpPost("unlinkIdentities")]
        public async Task<IActionResult> UnlinkIdentitiesAsync([FromBody] DOH_UnLinkingSources value, [FromQuery] string? processingOptions = null)
        {
            if (!ModelState.IsValid)
            {
                var errorMessage = GetErrorMessages(ModelState);

                var exceptionCustomProperties = new ExceptionCustomProperties
                {
                    User = HttpContext.GetCurrentUser(),
                    Agency = value.Agency,
                    Role = HttpContext.GetUserRoles(),
                    FunctionName = nameof(UnlinkIdentitiesAsync),
                    ErrorMessage = String.Join(",", errorMessage),
                    ErrorCode = "400"
                };
                var errorLogItem = new LogItem()
                {
                    Name = $"{Models.Logging.Constants.LogPrefix_API}{nameof(UnlinkIdentitiesAsync)}-Failed",
                    TrackingId = value.TrackingId,
                    Layer = ServiceLayer.API.ToString(),
                    ExceptionCustomProperties = exceptionCustomProperties
                };

                _logger.LogCritical(JsonConvert.SerializeObject(errorLogItem));

                return BuildOkObjectResultWith400Error(errorMessage, value.TrackingId);
            }
            if (HttpContext.Items["SourceSystem"] != null)
            {
                value.SourceSystem = HttpContext?.Items["SourceSystem"]?.ToString();
            }
            var (processType, notificationOptions) = GetProcessingOptions(processingOptions);
            var result = await _clientIdentityService.DOH_UnLinkIdentities(value, HttpContext?.GetCurrentUser() ?? string.Empty, processType, notificationOptions);
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
        [ServiceFilter(typeof(IPValidationFilter))]
        [HttpPost("mergeIdentities")]
        public async Task<IActionResult> MergeIdentitiesAsync([FromBody] DOH_MergingSources value, [FromQuery] string? processingOptions = null)
        {
            if (!ModelState.IsValid)
            {
                var errorMessage = GetErrorMessages(ModelState);

                var exceptionCustomProperties = new ExceptionCustomProperties
                {
                    User = HttpContext.GetCurrentUser(),
                    Agency = value.Agency,
                    Role = HttpContext.GetUserRoles(),
                    FunctionName = nameof(MergeIdentitiesAsync),
                    ErrorMessage = String.Join(",", errorMessage),
                    ErrorCode = "400"
                };
                var errorLogItem = new LogItem()
                {
                    Name = $"{Models.Logging.Constants.LogPrefix_API}{nameof(MergeIdentitiesAsync)}-Failed",
                    TrackingId = value.TrackingId,
                    Layer = ServiceLayer.API.ToString(),
                    ExceptionCustomProperties = exceptionCustomProperties
                };

                _logger.LogCritical(JsonConvert.SerializeObject(errorLogItem));

                return BuildOkObjectResultWith400Error(errorMessage, value.TrackingId);
            }
            if (HttpContext.Items["SourceSystem"] != null)
            {
                value.SourceSystem = HttpContext?.Items["SourceSystem"]?.ToString();
            }
            var (processType, notificationOptions) = GetProcessingOptions(processingOptions);
            var result = await _clientIdentityService.DOH_MergeIdentities(value, HttpContext?.GetCurrentUser() ?? string.Empty, processType, notificationOptions);
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
        [ServiceFilter(typeof(IPValidationFilter))]
        [HttpPost("unMergeIdentities")]
        public async Task<IActionResult> UnmergeIdentitiesAsync([FromBody] DOH_UnMergingSources value, [FromQuery] string? processingOptions = null)
        {
            if (!ModelState.IsValid)
            {
                var errorMessage = GetErrorMessages(ModelState);

                var exceptionCustomProperties = new ExceptionCustomProperties
                {
                    User = HttpContext.GetCurrentUser(),
                    Agency = value.Agency,
                    Role = HttpContext.GetUserRoles(),
                    FunctionName = nameof(UnmergeIdentitiesAsync),
                    ErrorMessage = String.Join(",", errorMessage),
                    ErrorCode = "400"
                };
                var errorLogItem = new LogItem()
                {
                    Name = $"{Models.Logging.Constants.LogPrefix_API}{nameof(UnmergeIdentitiesAsync)}-Failed",
                    TrackingId = value.TrackingId,
                    Layer = ServiceLayer.API.ToString(),
                    ExceptionCustomProperties = exceptionCustomProperties
                };

                _logger.LogCritical(JsonConvert.SerializeObject(errorLogItem));

                return BuildOkObjectResultWith400Error(errorMessage, value.TrackingId);
            }
            if (HttpContext.Items["SourceSystem"] != null)
            {
                value.SourceSystem = HttpContext?.Items["SourceSystem"]?.ToString();
            }
            var (processType, notificationOptions) = GetProcessingOptions(processingOptions);
            var result = await _clientIdentityService.DOH_UnMergeIdentities(value, HttpContext?.GetCurrentUser() ?? string.Empty, processType, notificationOptions);
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
        [ServiceFilter(typeof(IPValidationFilter))]
        [HttpPost("deleteSourceIdentity")]
        public async Task<IActionResult> DeleteSourceIdentityAsync([FromBody] DOH_DeleteClientIdentityRequest value, [FromQuery] string? processingOptions = null)
        {
            if (!ModelState.IsValid)
            {
                var errorMessage = GetErrorMessages(ModelState);

                var exceptionCustomProperties = new ExceptionCustomProperties
                {
                    User = HttpContext.GetCurrentUser(),
                    Agency = value.Agency,
                    Role = HttpContext.GetUserRoles(),
                    FunctionName = nameof(DeleteSourceIdentityAsync),
                    ErrorMessage = String.Join(",", errorMessage),
                    ErrorCode = "400"
                };
                var errorLogItem = new LogItem()
                {
                    Name = $"{Models.Logging.Constants.LogPrefix_API}{nameof(DeleteSourceIdentityAsync)}-Failed",
                    TrackingId = value.TrackingId,
                    Layer = ServiceLayer.API.ToString(),
                    ExceptionCustomProperties = exceptionCustomProperties
                };

                _logger.LogCritical(JsonConvert.SerializeObject(errorLogItem));

                return BuildOkObjectResultWith400Error(errorMessage, value.TrackingId);
            }
            if (HttpContext.Items["SourceSystem"] != null)
            {
                value.SourceSystem = HttpContext?.Items["SourceSystem"]?.ToString();
            }
            var (processType, notificationOptions) = GetProcessingOptions(processingOptions);
            var result = await _clientIdentityService.DOH_DeleteSourceIdentity(value, HttpContext?.GetCurrentUser() ?? string.Empty, processType, notificationOptions);
            //if (result == null) return BadRequest("Invalid Input");
            return Ok(result);
        }

        /// <summary>
        /// Enrich Demographic query appends third-party detailed demographic and lifesytle attribute that matches the demographic data provided in the web service request.
        /// </summary>
        /// <param name="filter">Filter condition for search</param>
        /// <param name="processingOptions"></param>
        /// <returns></returns>
        [SwaggerResponse(StatusCodes.Status200OK, "Get client identity response", typeof(DOH_EnrichDemographicQueryRequest))]
        [SwaggerResponse(StatusCodes.Status401Unauthorized)]
        [SwaggerResponse(StatusCodes.Status403Forbidden)]
        [SwaggerResponse(StatusCodes.Status500InternalServerError)]
        [ServiceFilter(typeof(IPValidationFilter))]
        [HttpPost("enrichDemographicsQuery")]
        public async Task<IActionResult> EnrichDemographicsQueryAsync([FromBody] DOH_EnrichDemographicQueryRequest filter, [FromQuery] string? processingOptions = null)
        {
            if (!ModelState.IsValid)
            {
                var errorMessage = GetErrorMessages(ModelState);

                var exceptionCustomProperties = new ExceptionCustomProperties
                {
                    User = HttpContext.GetCurrentUser(),
                    Agency = filter?.Agency,
                    Role = HttpContext.GetUserRoles(),
                    FunctionName = nameof(EnrichDemographicsQueryAsync),
                    ErrorMessage = String.Join(",", errorMessage),
                    ErrorCode = "400"
                };
                var errorLogItem = new LogItem()
                {
                    Name = $"{Models.Logging.Constants.LogPrefix_API}{nameof(EnrichDemographicsQueryAsync)}-Failed",
                    TrackingId = filter?.TrackingId,
                    Layer = ServiceLayer.API.ToString(),
                    ExceptionCustomProperties = exceptionCustomProperties
                };

                _logger.LogCritical(JsonConvert.SerializeObject(errorLogItem));

                return BuildOkObjectResultWith400Error(errorMessage, filter?.TrackingId);
            }
            if (HttpContext.Items["SourceSystem"] != null)
            {
                filter.SourceSystem = HttpContext.Items?["SourceSystem"]?.ToString();
            }

            var (processType, notificationOptions) = GetProcessingOptions(processingOptions);
            var enrichQueryResult = await _clientIdentityService.DOH_EnrichDemographicQuery(filter, HttpContext.GetCurrentUser(), processType, notificationOptions);
            return Ok(enrichQueryResult);
        }

        private static List<string> GetErrorMessages(ModelStateDictionary modelState) => modelState.Values.SelectMany(e => e.Errors).Select(em => em.ErrorMessage).ToList();

        private static OkObjectResult BuildOkObjectResultWith400Error(List<string> message, string? TrackingId = "") => new(new { errorCode = "400", Message = String.Join(",", message), Success = false, TrackingId = TrackingId });

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
    }
}
