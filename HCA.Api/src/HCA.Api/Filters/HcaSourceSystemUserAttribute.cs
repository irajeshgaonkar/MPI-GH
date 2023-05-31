using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;
using System.Security.Principal;

namespace HCA.Api.Filters;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class HcaSourceSystemUserAttribute : Attribute, IActionFilter
{
    private readonly string[] _roles;

    public HcaSourceSystemUserAttribute(params string[] roles)
    {
        _roles = roles;
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
        throw new NotImplementedException();
    }

    public void OnActionExecuting(ActionExecutingContext context)
    {
        throw new NotImplementedException();
    }
}
