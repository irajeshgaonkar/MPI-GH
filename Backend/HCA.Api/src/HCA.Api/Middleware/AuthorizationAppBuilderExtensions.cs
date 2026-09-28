using HCA.Api.Options;

namespace HCA.Api.Middleware;

public static class AuthorizationAppBuilderExtensions
{
    public static void UseJwtMiddleware(this IApplicationBuilder app)
    {
        app.UseMiddleware<JwtMiddleware>();
    }
}
