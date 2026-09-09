namespace HCA.AdminMetrics.Api.Middleware;

/// <summary>
/// Adds authorization-related middleware extensions to the application builder.
/// </summary>
public static class AuthorizationAppBuilderExtensions
{
    /// <summary>
    /// Registers the JWT middleware in the request pipeline.
    /// </summary>
    /// <param name="app">The application builder.</param>
    public static void UseJwtMiddleware(this IApplicationBuilder app)
    {
        app.UseMiddleware<JwtMiddleware>();
    }
}
