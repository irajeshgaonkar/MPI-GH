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
                string agency = Convert.ToString(jsonObjectRequestBody["Agency"]) ?? "";
                string trackingId = Convert.ToString(jsonObjectRequestBody["TrackingId"]) ?? "";

                // TODO: we can load source system from the DB
                //if (string.IsNullOrEmpty(sourceSystem))
                //{
                //    context.Result = BuildOkObjectResultWith400Error( "sourceSystem validation failed. Input is missing sourceSystem field." , trackingId);
                //    return;
                //}
                if (string.IsNullOrEmpty(ipAddress))
                {
                    context.Result = BuildOkObjectResultWith400Error( "ipAddress validation failed. Input is missing ipAddress value." , trackingId);
                    return;
                }

                sourceSystem = await _iPConfigRepository.GetSourceSystemFromIp( ipAddress );
                if( string.IsNullOrEmpty( sourceSystem ) )
                {
                    context.Result = BuildOkObjectResultWith400Error( "sourceSystem validation failed. ipAddress/sourceSystem mapping failed.", trackingId );
                    return;
                }
               
                // TODO: URGENT/current: why are we requiring agency?
                //if (string.IsNullOrEmpty(agency))
                //{
                //    context.Result = BuildOkObjectResultWith400Error("Agency validation failed. Input is missing agency value.", trackingId);
                //    return;
                //}

                // TODO: no need to check this given we get source system from ip
                //bool isTrusted = await _iPConfigRepository.IsIPAddressTrustedAsync(sourceSystem, ipAddress);
                //if (!isTrusted) 
                //{
                //    // TODO: swap this to a unauthorized error/result once systems are online
                //    context.Result = BuildOkObjectResultWith400Error( "sourceSystem validation failed. ipAddress/sourceSystem mismatch." ,trackingId);
                //    return;
                //}
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

        private static OkObjectResult BuildOkObjectResultWith400Error( string message, string? trackingid = "" ) => new( new { errorCode = "400", Message = message, Success = false, TrackingId = trackingid } );
    }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}
