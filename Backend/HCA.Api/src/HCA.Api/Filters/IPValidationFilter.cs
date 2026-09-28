using System.Text.Json;
using HCA.Api.Extensions;
using HCA.Data.Repository;
using HCA.Infrastructure.Exceptions;
using HCA.Infrastructure.Logger;
using HCA.Models.Logging;
using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json.Linq;

namespace HCA.Api.Filters
{
    /// <summary>
    /// IP Validation Filter
    /// </summary>
    /// <param name="onboardedSystemRepository"></param>
    /// <param name="logger"></param>
    public class IPValidationFilter(IOnboardedSystemRepository onboardedSystemRepository, IAppLogger logger) : IAsyncActionFilter
    {
        private readonly IOnboardedSystemRepository _onboardedSystemRepository = onboardedSystemRepository;

        private readonly IAppLogger _logger = logger;

        public async Task OnActionExecutionAsync( ActionExecutingContext context, ActionExecutionDelegate next )
        {
            try
            {
                var requestBody = context.ActionArguments.FirstOrDefault();

                string body = JsonSerializer.Serialize(requestBody.Value);

                JObject jsonObjectRequestBody = JObject.Parse(body);

                string ipAddress = GetClientIpAddress(context, jsonObjectRequestBody);
                string trackingId = Convert.ToString(jsonObjectRequestBody["TrackingId"]) ?? "";

                //Add trackingId to context to consume and format the exception if any in down the line.
                if (!string.IsNullOrEmpty(trackingId))
                {
                    context.HttpContext.Items["TrackingId"] = trackingId;
                }

                if (string.IsNullOrEmpty(ipAddress))
                {
                    throw new IPValidationException($"ipAddress validation failed. Incoming request does not have an IP Address.");
                }

                var sourceSystems = await _onboardedSystemRepository.GetActiveSourceSystemsByIPAsync(ipAddress);

                // If there are no Source Systems for incoming IP - Block it.
                if(sourceSystems.Count == 0)
                {
                    throw new IPValidationException("sourceSystem validation failed. ipAddress/sourceSystem mismatch. Please contact MPI to resolve.");
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
                            throw new IPValidationException("Your Whitelisted IP Address is conflicting with another system. Please contact MPI to resolve");
                        }
                    }
                }               
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
                throw;
            }
        }

        /// <summary>
        /// Get incoming IP Address
        /// </summary>
        /// <param name="context"></param>
        /// <param name="jsonObjectRequestBody"></param>
        /// <returns></returns>
        private string GetClientIpAddress(ActionExecutingContext context, JObject jsonObjectRequestBody)
        {
            // Try to get IP from request body
            string? ipAddress = Convert.ToString(jsonObjectRequestBody?["IpAddress"]);

            _logger.LogInformation($"IPAddress from request Body: {ipAddress}");

            if (string.IsNullOrWhiteSpace(ipAddress))
            {
                // Fallback: check X-Forwarded-For header
                ipAddress = context?.HttpContext?.Request?.Headers?["X-Forwarded-For"].First();
                _logger.LogInformation($"IPAddress from Headers: {ipAddress}");

                if (!string.IsNullOrWhiteSpace(ipAddress))
                {
                    // X-Forwarded-For can contain multiple IPs; take the first one
                    ipAddress = ipAddress.Split(',').First().Trim();
                }
                else
                {
                    // Final fallback: remote IP address
                    ipAddress = context?.HttpContext.Connection.RemoteIpAddress?.ToString();
                    _logger.LogInformation($"IPAddress from RemoteIpAddress: {ipAddress}");
                }
            }

            return ipAddress ?? string.Empty;
        }
    }
}
