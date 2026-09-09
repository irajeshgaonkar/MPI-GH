using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using HCA.AdminMetrics.Api.Constants;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace HCA.AdminMetrics.Api.Filters;

/// <summary>
/// Enforces JWT-based authorization and optional role checks for controller actions.
/// </summary>
/// <param name="roles">The roles allowed to access the target endpoint.</param>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class HcaAuthorizeAttribute( params string[] roles ) : Attribute, IAuthorizationFilter
{
    /// <summary>
    /// Performs authorization for the current request.
    /// </summary>
    /// <param name="context">The authorization filter context.</param>
    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var user = context.HttpContext.User;
        var authorizationHeader = context.HttpContext.Request.Headers[RequestHeaders.Authorization].ToString();

        if (string.IsNullOrEmpty(authorizationHeader) || user == null)
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

            if (!string.IsNullOrWhiteSpace(clientId))
            {
                return;
            }

            var appId = jwt.Claims.FirstOrDefault(c => c.Type == "appid")?.Value;
            if (!string.IsNullOrWhiteSpace(appId))
            {
                return;
            }

            SetUnAuthorized(context);
            return;
        }

        if (roles.Length == 0)
        {
            return;
        }

        foreach (var role in roles)
        {
            if (user.IsInRole(role))
            {
                return;
            }
        }

        SetForbidden(context);
    }

    private static void SetUnAuthorized(AuthorizationFilterContext context)
    {
        context.Result = new UnauthorizedResult();
    }

    private static void SetForbidden(AuthorizationFilterContext context)
    {
        context.Result = new ForbidResult();
    }
}
