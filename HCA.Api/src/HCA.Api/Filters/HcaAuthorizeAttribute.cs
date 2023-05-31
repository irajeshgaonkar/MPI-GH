using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Principal;

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

    public SourceSystemValidator(ISessionService sessionService)
    {
        _sessionService = sessionService;
    }

    public async Task<bool> ValidateSourceSystem(HttpContext context, IEnumerable<string> sourceSystems)
    {
        var sourceSystemName = await _sessionService.GetSourceSystemName(context);

        if (string.IsNullOrWhiteSpace(sourceSystemName))
            return true;

        foreach(var sourceSystem in sourceSystems)
        {
            if (sourceSystemName.ToLower() != sourceSystem.ToLower())
                return false;
        }

        return true; ;
    }
}

public interface ISessionService
{
    Task<string> GetSourceSystemName(HttpContext context);
}

public class SessionService : ISessionService
{
    public SessionService()
    {

    }

    public async Task<string> GetSourceSystemName(HttpContext context)
    {
        var user = context.User;
        var authorizationHeader = context.Request.Headers["Authorization"].ToString();
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
public class HcaAuthorizeAttribute : Attribute, IAuthorizationFilter
{
    private readonly string[] _roles;

    public HcaAuthorizeAttribute(params string[] roles)
    {
        _roles = roles;
    }

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var user = context.HttpContext.User;
        var authorizationHeader = context.HttpContext.Request.Headers["Authorization"].ToString();

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
