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

                if (string.IsNullOrEmpty(ipAddress))
                {
                    context.Result = BuildOkObjectResultWith400Error( "ipAddress validation failed. Input is missing ipAddress value." );
                    return;
                }

                if( string.IsNullOrEmpty( sourceSystem ) )
                {
                    sourceSystem = await _iPConfigRepository.GetSourceSystemFromIp( ipAddress );
                }
                else
                {
                    bool isTrusted = await _iPConfigRepository.IsIPAddressTrustedAsync(sourceSystem, ipAddress);
                    if( !isTrusted )
                    {
                        // TODO: swap this to a unauthorized error/result once systems are online
                        context.Result = BuildOkObjectResultWith400Error( "sourceSystem validation failed. ipAddress/sourceSystem mismatch." );
                        return;
                    }
                }

                await next();
            }
            catch (JsonException e)
            {
                // TODO: swap this to a unauthorized error/result once systems are online
                context.Result = BuildOkObjectResultWith400Error( "sourceSystem validation failed. JsonException Error: "+ e.Message );
            }
            catch (InvalidOperationException e) {
                context.Result = BuildOkObjectResultWith400Error( "sourceSystem validation failed. Likely database IP list error: "+ e.Message );
            }
            catch (Exception e ){ 
                context.Result = BuildOkObjectResultWith400Error( "sourceSystem validation failed. Unknown error: "+ e.Message );
            }
        }

        private static OkObjectResult BuildOkObjectResultWith400Error( string message ) => new( new { errorCode = "400", Message = message } );
    }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}
