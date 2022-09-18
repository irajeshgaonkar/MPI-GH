using HCA.Api.Constants;
using System.Security.Claims;

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
}
