using HCA.Api.Options;

namespace HCA.Api.Middleware;

public static class AuthorizationAppBuilderExtensions
{
    public static void UseJwtMiddleware(this IApplicationBuilder app, Func<SecurityOptions> optionsProvider)
    {
        var securityOptions = optionsProvider();
        app.UseMiddleware<JwtMiddleware>(securityOptions);
    }
}
