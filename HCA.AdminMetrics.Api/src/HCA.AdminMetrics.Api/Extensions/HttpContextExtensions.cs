using HCA.AdminMetrics.Api.Constants;

namespace HCA.AdminMetrics.Api.Extensions;

/// <summary>
/// Provides helper extensions for working with <see cref="HttpContext"/>.
/// </summary>
public static class HttpContextExtensions
{
    /// <summary>
    /// Gets the authorization header value from the current request.
    /// </summary>
    /// <param name="context">The current HTTP context.</param>
    /// <returns>The authorization header value when present; otherwise, <see langword="null"/>.</returns>
    public static string? GetAuthorizationHeader(this HttpContext context)
    {
        return context.Request.Headers[RequestHeaders.Authorization].FirstOrDefault();
    }
}
