using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;

namespace HCA.Api.Filters;

public class UserFilter : Attribute, IActionFilter
{
    public void OnActionExecuted(ActionExecutedContext context)
    {

    }

    public void OnActionExecuting(ActionExecutingContext context)
    {
        var currentUser = context.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (currentUser == null) return;
        context.ActionArguments["currentUser"] = currentUser;
    }
}
