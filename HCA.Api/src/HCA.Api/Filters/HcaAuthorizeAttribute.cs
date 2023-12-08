using HCA.Api.Constants;
using HCA.Core.Services;
using HCA.Infrastructure.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace HCA.Api.Filters;

public interface ISession
{
    string SourceSystemName { get; set; }

    string UserName { get; set; }
}

public class Session : ISession
{
    public string SourceSystemName { get; set; }

    public string UserName { get; set; }
}

public interface ISourceSystemValidator
{
    Task<bool> ValidateSourceSystem(HttpContext context, IEnumerable<string> sourceSystems);
}

public class SourceSystemValidator : ISourceSystemValidator
{
    private readonly ISessionService _sessionService;

    private readonly IServiceAccountService _serviceAccountService;

    public SourceSystemValidator(IServiceAccountService serviceAccountService, ISessionService sessionService)
    {
        _sessionService = sessionService;
        _serviceAccountService = serviceAccountService;
    }

    public async Task<bool> ValidateSourceSystem(HttpContext context, IEnumerable<string> sourceSystems)
    {
        var appId = await _sessionService.GetAppId(context);

        if (string.IsNullOrWhiteSpace(appId))
            return true;

        var serviceAccount = await _serviceAccountService.GetServiceAccount(appId);

        if (serviceAccount == null)
            return false;

        var sourceSystemName = serviceAccount.SourceSystemName;

        foreach (var sourceSystem in sourceSystems)
        {
            if (sourceSystemName.ToLower() != sourceSystem.ToLower())
                return false;
        }

        return true; ;
    }
}

public interface ISessionService
{
    Task<string> GetAppId(HttpContext context);
}

public class SessionService : ISessionService
{
    public SessionService()
    {

    }

    public async Task<string> GetAppId(HttpContext context)
    {
        var user = context.User;
        var authorizationHeader = context.Request.Headers[RequestHeaders.Authorization].ToString();
        if (string.IsNullOrEmpty(authorizationHeader))
        {
            return await Task.FromResult(string.Empty);
        }

        if (user == null)
        {
            return await Task.FromResult(string.Empty);
        }

        var name = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (name == null)
        {
            var token = authorizationHeader.Split("Bearer ");

            if (token.Length < 2)
            {
                return await Task.FromResult(string.Empty);
            }

            var jwtToken = token[1];
            var jwt = new JwtSecurityTokenHandler().ReadJwtToken(jwtToken);
            string appId = jwt.Claims.First(c => c.Type == "appid").Value;
            return appId;
        }

        return string.Empty;
    }
}


[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class SourceSystemAuthorizeAttribute : Attribute, IAuthorizationFilter
{
    public async void OnAuthorization(AuthorizationFilterContext context)
    {
       // this code moved to HttpContextExtensions GetRequestbody

            //var requestData = SerializationExtensions.DeSerializeWithoutCasing<dynamic>(requestBody);

        //foreach (var item in requestData)
        //{
        //    Console.WriteLine(item);
        //}
        //sourceSystemNames.Add(sourceSystemName);

        // get all the source system names
    }
}

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class HcaAuthorizeAttribute : Attribute, IAuthorizationFilter
{
    private readonly string[] _roles;

    public HcaAuthorizeAttribute(params string[] roles)
    {
        _roles = roles;
    }

    public async void OnAuthorization(AuthorizationFilterContext context)
    {
        var user = context.HttpContext.User;
        var authorizationHeader = context.HttpContext.Request.Headers[RequestHeaders.Authorization].ToString();

        if (string.IsNullOrEmpty(authorizationHeader))
        {
            SetUnAuthorized(context);
            return;
        }

        if (user == null)
        {
            SetUnAuthorized(context);
            return;

        }
        var name = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (name == null)
        {
            var token = authorizationHeader.Split("Bearer ");

            if (token.Length < 2)
            {
                SetUnAuthorized(context);
                return;
            }

            var jwtToken = token[1];
            var jwt = new JwtSecurityTokenHandler().ReadJwtToken(jwtToken);

            var clientId = jwt.Claims.FirstOrDefault(c => c.Type == "client_id")?.Value;
            if(!string.IsNullOrWhiteSpace(clientId))
            {
                return;
            }

            string appId = jwt.Claims.First(c => c.Type == "appid").Value;
            return;
        }

        if (_roles != null)
        {
            foreach (var role in _roles)
            {
                var isInRole = user.IsInRole(role);
                if (isInRole) return;
            }

            SetForbidden(context);
        }
    }

    private void SetUnAuthorized(AuthorizationFilterContext context)
    {
        context.Result = new UnauthorizedResult();
    }

    private void SetForbidden(AuthorizationFilterContext context)
    {
        context.Result = new ForbidResult();
    }
}
