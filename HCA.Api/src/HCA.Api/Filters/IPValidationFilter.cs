using HCA.Data.Repository;
using HCA.Data.Repository.Impl;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace HCA.Api.Filters
{
    public class IPValidationFilter : IAsyncActionFilter
    {
        private readonly IIPConfigRepository _iPConfigRepository;

        public IPValidationFilter(IIPConfigRepository iPConfigRepository)
        {
            _iPConfigRepository = iPConfigRepository;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            try
            {
                var requestBody = context.ActionArguments.FirstOrDefault();


                string body = JsonConvert.SerializeObject(requestBody.Value);

                JObject jsonObject = JObject.Parse(body);

                string sourceSystem = (string)jsonObject["SourceSystem"];
                string ipAddress = (string)jsonObject["IpAddress"];

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
}
