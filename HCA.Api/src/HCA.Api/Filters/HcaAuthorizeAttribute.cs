using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;
using System.Security.Principal;

namespace HCA.Api.Filters;

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

        if (user == null)
        {
            SetUnAuthorized(context);
            return;
        }

        var name = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (name == null)
        {
            SetUnAuthorized(context);
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
