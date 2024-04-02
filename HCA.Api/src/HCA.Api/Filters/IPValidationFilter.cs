using HCA.Data.Repository;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace HCA.Api.Filters
{
    // TODO: add documentation
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public class IPValidationFilter : IAsyncActionFilter
    {
        private readonly IIPConfigRepository _iPConfigRepository;

        public IPValidationFilter(IIPConfigRepository iPConfigRepository)
        {
            _iPConfigRepository = iPConfigRepository;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            // TODO: limit try/catch wrapping
            try
            {
                var requestBody = context.ActionArguments.FirstOrDefault();


                string body = JsonConvert.SerializeObject(requestBody.Value);

                JObject jsonObjectRequestBody = JObject.Parse(body);

                string sourceSystem = Convert.ToString(jsonObjectRequestBody["SourceSystem"]) ?? "";
                string ipAddress = Convert.ToString(jsonObjectRequestBody["IpAddress"]) ?? "";

                if (string.IsNullOrEmpty(sourceSystem) || string.IsNullOrEmpty(ipAddress))
                {
                    context.Result = new BadRequestObjectResult("Both sourceSystem and ipAddress are required fields");
                    return;
                }

                bool isTrusted = await _iPConfigRepository.IsIPAddressTrustedAsync(sourceSystem, ipAddress);
                if (!isTrusted)
                {
                    string errorMessage = "IP Validation failed.The IP address is not trusted.";
                    context.Result = new ObjectResult(errorMessage)
                    {
                        StatusCode = StatusCodes.Status403Forbidden
                    };
                    return;
                }
                await next();
            }
            catch (JsonException)
            {
                context.Result = new BadRequestResult();
            }

        }
    }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member

}
