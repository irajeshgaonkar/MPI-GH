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

        public async Task OnActionExecutionAsync( ActionExecutingContext context, ActionExecutionDelegate next )
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
                    context.Result = BuildOkObjectResultWith400Error( "sourceSystem validation failed. Input is missing sourceSystem field.");
                    return;
                }
                if (string.IsNullOrEmpty(ipAddress))
                {
                    context.Result = BuildOkObjectResultWith400Error( "ipAddress validation failed. Input is missing ipAddress value."  );
                    return;
                }

                bool isTrusted = await _iPConfigRepository.IsIPAddressTrustedAsync(sourceSystem, ipAddress);
                if (!isTrusted) 
                {
                    // TODO: swap this to a unauthorized error/result once systems are online
                    context.Result = BuildOkObjectResultWith400Error( "sourceSystem validation failed. ipAddress/sourceSystem mismatch." );
                    return;
                }
                await next();
            }
            catch (JsonException)
            {
                // TODO: swap this to a unauthorized error/result once systems are online
                context.Result = BuildOkObjectResultWith400Error( "sourceSystem validation failed. Unknown Error."  );
            }
        }

        private static OkObjectResult BuildOkObjectResultWith400Error( string message ) => new( new { errorCode = "400", Message = message } );
    }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}
