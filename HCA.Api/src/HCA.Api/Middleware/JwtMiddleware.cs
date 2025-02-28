
using HCA.Api.Constants;
using HCA.Api.Extensions;
using HCA.Api.Options;
using HCA.Infrastructure.Logger;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Principal;

namespace HCA.Api.Middleware;

public class JwtMiddleware
{
    private readonly RequestDelegate _next;

    private readonly SecurityOptions _securityOptions;

    public JwtMiddleware(RequestDelegate next, SecurityOptions securityOptions)
    {
        _next = next;
        _securityOptions = securityOptions;
    }

    public async Task Invoke(HttpContext context)
    {
        var authorizationHeader = context.GetAuthorizationHeader();
        var token = authorizationHeader?.Split(" ").Last();

        if (token != null)
            AttachUserToContext(context, token);

        await _next(context);
    }

    private void AttachUserToContext(HttpContext context, string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(token);
            var email = jwtToken.Claims.First(x => x.Type == Claims.Email).Value;
            var customGroups = jwtToken.Claims.First(x => x.Type == "custom:groups").Value;
            var groups = ParseUserGroups(customGroups);
            var roles = new List<string>() { Roles.ReadOnly };
            var isAdmin = groups?.Any(g => g == _securityOptions.AdminGroupName);
            if (isAdmin == true) roles.Add(Roles.Admin);
            var identity = new ClaimsIdentity(Claims.Token);
            var claims = new List<Claim>() { new Claim(ClaimTypes.NameIdentifier, email)};

            if (groups != null)
            {
                foreach (var group in groups)
                {
                    claims.Add(new Claim(ClaimTypes.GroupSid, group));
                }
            }

            identity.AddClaims(claims);
            var user = new GenericPrincipal(identity, roles.ToArray());
            context.User = user;

            // Todo: !Refactor - add Service Principal Authentication
        }
        catch (Exception ex)
        {
            //// user is not attached to context so request won't have access to secure routes
        }
    }

    private string[] ParseUserGroups(string customGroups)
    {
        var groups = customGroups.Split(',');

        for(int i = 0; i < groups.Length; ++i)
        {
            groups[i] = groups[i].Trim().Trim('[').Trim(']');
        }

        return groups;
    }
}
