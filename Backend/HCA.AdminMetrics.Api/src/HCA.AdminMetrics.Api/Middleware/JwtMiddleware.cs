using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Principal;
using HCA.AdminMetrics.Api.Constants;
using HCA.AdminMetrics.Api.Extensions;
using HCA.Infrastructure.Configurations;

namespace HCA.AdminMetrics.Api.Middleware;

public class JwtMiddleware( RequestDelegate next, AppSettings appSettings )
{
    public async Task Invoke(HttpContext context)
    {
        var authorizationHeader = context.GetAuthorizationHeader();
        var token = authorizationHeader?.Split(" ").Last();

        if (token != null)
        {
            AttachUserToContext(context, token);
        }

        await next(context);
    }

    private void AttachUserToContext(HttpContext context, string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(token);
            var email = jwtToken.Claims.FirstOrDefault(x => x.Type == Claims.Email)?.Value;
            var customGroups = jwtToken.Claims.FirstOrDefault(x => x.Type == "custom:groups")?.Value;
            var groups = ParseUserGroups(customGroups);
            var roles = new List<string> { Roles.ReadOnly };
            var isAdmin = groups?.Any(g => g == appSettings.SecurityOptions?.AdminGroupName);
            if (isAdmin == true)
            {
                roles.Add(Roles.Admin);
            }

            var identity = new ClaimsIdentity(Claims.Token);
            var claims = new List<Claim>();

            if (!string.IsNullOrWhiteSpace(email))
            {
                claims.Add(new Claim(ClaimTypes.NameIdentifier, email));
            }

            if (groups != null)
            {
                foreach (var group in groups)
                {
                    claims.Add(new Claim(ClaimTypes.GroupSid, group));
                }
            }

            identity.AddClaims(claims);
            var user = new GenericPrincipal(identity, [.. roles]);
            context.User = user;
        }
        catch
        {
            // Leave the request unauthenticated and let the authorize filter reject it.
        }
    }

    private static string[]? ParseUserGroups(string? customGroups)
    {
        if (string.IsNullOrWhiteSpace(customGroups))
        {
            return null;
        }

        var groups = customGroups.Split(',');

        for (var i = 0; i < groups.Length; ++i)
        {
            groups[i] = groups[i].Trim().Trim('[').Trim(']');
        }

        return groups;
    }
}
