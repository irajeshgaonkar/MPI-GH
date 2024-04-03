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

                if (string.IsNullOrEmpty(sourceSystem))
                {
                    context.Result = new OkObjectResult(new { errorCode = "400" ,message ="sourceSystem Validation  is failed , There is no sourcesystem field is present in the input"});
                    return;
                }
                if (string.IsNullOrEmpty(ipAddress))
                {
                    context.Result = new OkObjectResult(new { errorCode = "400", message = "ipAddress Validation is failed , There is no ipAddress value present in the input"});
                    return;
                }

                bool isTrusted = await _iPConfigRepository.IsIPAddressTrustedAsync(sourceSystem, ipAddress);
                if (!isTrusted)
                {
                    context.Result = new OkObjectResult(new { errorCode = "400", Message = "IPAddress/SourceSytem name Validation failed. Received IPAddress/SourceSystem names from input are not matched." });
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
