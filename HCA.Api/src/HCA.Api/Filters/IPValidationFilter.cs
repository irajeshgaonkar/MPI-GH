using HCA.Api.Extensions;
using HCA.Data.Repository;
using HCA.Infrastructure.Logger;
using HCA.Models.Logging;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json.Linq;
using System.Text.Json;

namespace HCA.Api.Filters
{
    // TODO: add documentation
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public class IPValidationFilter : IAsyncActionFilter
    {
        private readonly IOnboardedSystemRepository _onboardedSystemRepository;

        private readonly IAppLogger _logger;

        public IPValidationFilter(IOnboardedSystemRepository onboardedSystemRepository, IAppLogger logger)
        {
            _onboardedSystemRepository = onboardedSystemRepository;
            _logger = logger;
        }

        public async Task OnActionExecutionAsync( ActionExecutingContext context, ActionExecutionDelegate next )
        {
            // TODO: limit try/catch wrapping
            try
            {
                var requestBody = context.ActionArguments.FirstOrDefault();

                string body = JsonSerializer.Serialize(requestBody.Value);

                JObject jsonObjectRequestBody = JObject.Parse(body);

                string ipAddress = Convert.ToString(jsonObjectRequestBody["IpAddress"]) ?? "";
                string trackingId = Convert.ToString(jsonObjectRequestBody["TrackingId"]) ?? "";

                if (string.IsNullOrEmpty(ipAddress))
                {
                    context.Result = BuildOkObjectResultWith400Error( "ipAddress validation failed. Input is missing ipAddress value." , trackingId);
                    return;
                }

                var sourceSystems = await _onboardedSystemRepository.GetActiveSourceSystemsByIPAsync(ipAddress);

                // If there are no Source Systems for incoming IP - Block it.
                if(sourceSystems.Count == 0)
                {
                    context.Result = BuildOkObjectResultWith400Error("sourceSystem validation failed. ipAddress/sourceSystem mismatch.", trackingId);
                    return;
                }
                //If there is one matching source system for incoming IP - Allow
                if(sourceSystems.Count == 1)
                {
                    context.HttpContext.Items["SourceSystem"] = sourceSystems.First();
                    await next();
                    return;
                }

                //If there are duplicate whitelisting for same source and IP combination- Consider it as one source system
                if(sourceSystems.Distinct(StringComparer.OrdinalIgnoreCase).Count() == 1)
                {
                    context.HttpContext.Items["SourceSystem"] = sourceSystems.First();
                    await next();
                    return;
                }
                //If there are multiple source systems for incoming IP
                else
                {
                    //Check if incoming source system header exists and matches one of the whitelisted systems for the incoming Ip Address
                    var sourceSystemFromRequest = jsonObjectRequestBody["SourceSystem"]?.ToString();

                    if(!string.IsNullOrEmpty(sourceSystemFromRequest)
                        && sourceSystems.Any(source => string.Equals(source, sourceSystemFromRequest, StringComparison.OrdinalIgnoreCase)))
                    {
                        context.HttpContext.Items["SourceSystem"] = sourceSystemFromRequest;
                        await next();
                        return;
                    }
                    else
                    {
                        //Get the parent systems of sub systems
                        var parentSourceSystem = sourceSystems.Select(source => source.Split('.')[0]).ToList();

                        //If the sub systems are for same parent system - Allow parent system
                        if (parentSourceSystem.Distinct(StringComparer.OrdinalIgnoreCase).Count() == 1)
                        {
                            //Check if request header has optional source system name
                            context.HttpContext.Items["SourceSystem"] = parentSourceSystem.First();
                            await next();
                            return;
                        }
                        //If same IP whitelisted for multiple parent systems - Block it as it is ambiguous
                        //This can happen when whitelisting systems manually
                        else
                        {
                            context.Result = BuildOkObjectResultWith400Error("sourceSystem validation failed. ipAddress/sourceSystem mismatch.", trackingId);
                            return;
                        }
                    }
                }               
            }
            catch (JsonException e)
            {
                // TODO: swap this to a unauthorized error/result once systems are online
                var exceptionCustomProperties = new ExceptionCustomProperties
                {
                    User = context.HttpContext.GetCurrentUser(),
                    Agency = "",
                    Role = context.HttpContext.GetUserRoles(),
                    FunctionName = nameof(IPValidationFilter),
                    ErrorMessage = e.Message,
                    StackTrace = e.StackTrace,
                    ErrorCode = "400"
                };
                var errorLogItem = new LogItem()
                {
                    Name = $"{HCA.Models.Logging.Constants.LogPrefix_API}-{nameof(IPValidationFilter)}-Failed",
                    TrackingId = "",
                    Layer = ServiceLayer.API.ToString(),
                    ExceptionCustomProperties = exceptionCustomProperties
                };

                _logger.LogError(e, JsonSerializer.Serialize(errorLogItem));
                context.Result = BuildOkObjectResultWith400Error( "sourceSystem validation failed. JsonException Error: "+ e.Message );
            }
            catch (InvalidOperationException e) {
                var exceptionCustomProperties = new ExceptionCustomProperties
                {
                    User = context.HttpContext.GetCurrentUser(),
                    Agency = "",
                    Role = context.HttpContext.GetUserRoles(),
                    FunctionName = nameof(IPValidationFilter),
                    ErrorMessage = e.Message,
                    StackTrace = e.StackTrace,
                    ErrorCode = "400"
                };
                var errorLogItem = new LogItem()
                {
                    Name = $"{HCA.Models.Logging.Constants.LogPrefix_API}-{nameof(IPValidationFilter)}-Failed",
                    TrackingId = "",
                    Layer = ServiceLayer.API.ToString(),
                    ExceptionCustomProperties = exceptionCustomProperties
                };

                _logger.LogError(e, JsonSerializer.Serialize(errorLogItem));
                context.Result = BuildOkObjectResultWith400Error( "sourceSystem validation failed. Likely database IP list error: "+ e.Message );
            }
            catch (Exception e ){
                var exceptionCustomProperties = new ExceptionCustomProperties
                {
                    User = context.HttpContext.GetCurrentUser(),
                    Agency = "",
                    Role = context.HttpContext.GetUserRoles(),
                    FunctionName = nameof(IPValidationFilter),
                    ErrorMessage = e.Message,
                    StackTrace = e.StackTrace,
                    ErrorCode = "400"
                };
                var errorLogItem = new LogItem()
                {
                    Name = $"{HCA.Models.Logging.Constants.LogPrefix_API}-{nameof(IPValidationFilter)}-Failed",
                    TrackingId = "",
                    Layer = ServiceLayer.API.ToString(),
                    ExceptionCustomProperties = exceptionCustomProperties
                };

                _logger.LogError(e, JsonSerializer.Serialize(errorLogItem));
                context.Result = BuildOkObjectResultWith400Error( "sourceSystem validation failed. Unknown error: "+ e.Message );
            }
        }

        private static OkObjectResult BuildOkObjectResultWith400Error( string message, string? trackingid = "" ) => new( new { errorCode = "400", Message = message, Success = false, TrackingId = trackingid } );
    }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}
