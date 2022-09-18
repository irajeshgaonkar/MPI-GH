
using HCA.Api.Constants;
using HCA.Api.Options;
using HCA.Infrastructure.Extensions;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;

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
        var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

        if (token != null)
            attachUserToContext(context, token);

        await _next(context);
    }

    private void attachUserToContext(HttpContext context, string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(token);
            var email = jwtToken.Claims.First(x => x.Type == "email").Value;
            var customGroups = jwtToken.Claims.First(x => x.Type == "custom:groups").Value;
            var groups = ParseUserGroups(customGroups);
            var roles = new List<string>() { Roles.ReadOnly };
            var isAdmin = groups?.Any(g => g == _securityOptions.AdminGroupName);
            if (isAdmin == true) roles.Add(Roles.Admin);
            var identity = new ClaimsIdentity("token");
            var claims = new List<Claim>() { new Claim(ClaimTypes.NameIdentifier, email)};
            identity.AddClaims(claims);
            var user = new GenericPrincipal(identity, roles.ToArray());
            context.User = user;
        }
        catch (Exception ex)
        {
            // do nothing if jwt validation fails
            // user is not attached to context so request won't have access to secure routes
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
