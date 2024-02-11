using HCA.Api.Constants;
using System.Security.Claims;
using System.Text;

namespace HCA.Api.Extensions;

public static class HttpContextExtensions
{
    public static bool CanShowSensitiveData(this HttpContext context)
    {
        return context.User.IsInRole(Roles.Admin);
    }

    public static string GetCurrentUser(this HttpContext context)
   {
        var currentUser = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return currentUser ?? string.Empty;
    }

    /// <summary>
    /// get current user source system name from request headers
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    public static string GetCurrentUserSourceSystem(this HttpContext context)
    {
        var currentUser = context.User.FindFirst(Claims.SourceSystemName)?.Value;
        return currentUser ?? string.Empty;
    }

    /// <summary>
    /// get current autherization from request headers
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    public static string? GetAuthorizationHeader(this HttpContext context)
    {
        return context.Request.Headers[RequestHeaders.Authorization].FirstOrDefault();
    }

    /// <summary>
    /// get current request type return 
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    public static string GetRequestType(this HttpContext context)
    {
        return context.Request.Method;
    }

    /// <summary>
    /// get check rquest types which contains payload
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    public static bool ContainsRequestBody(this HttpContext context)
    {
        var requestType = context.Request.Method;
        var types = new string[] { RequestType.Post, RequestType.Put, RequestType.Patch };
        return types.Contains(requestType.ToLower());
    }

    /// <summary>
    /// get AbsoluteUrl from http header
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    public static string GetAbsoluteUrl(this HttpContext context)
    {
        var request = context.Request;
        var absoluteUri = $"{request.Scheme}://{request.Host}{request.PathBase}{request.Path}{request.QueryString}";
        return absoluteUri;
    }

    /// <summary>
    /// get request body from request header
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    public static async Task<string> GetRequestBody(this HttpContext context)
    {
        if (ContainsRequestBody(context))
        {
            using (StreamReader reader = new StreamReader(context.Request.Body, Encoding.UTF8))
            {
                string requestBody = await reader.ReadToEndAsync();
                return requestBody;
            }
        }

        return string.Empty;
    }
}
