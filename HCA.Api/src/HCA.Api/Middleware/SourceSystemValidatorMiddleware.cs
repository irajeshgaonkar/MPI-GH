
using HCA.Api.Constants;
using HCA.Api.Extensions;
using HCA.Api.Options;
using HCA.Api.Providers;
using HCA.Infrastructure.Extensions;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;

namespace HCA.Api.Middleware;

public class SourceSystemValidatorMiddleware
{
    private readonly RequestDelegate _next;

    public SourceSystemValidatorMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        var absoluteUri = context.GetAbsoluteUrl();
        var requestBody = context.GetRequestBody();

        var sourceSystemProvider = SourceSystemProviderFactory.GetSourceSystemProvider(absoluteUri);
        var sourceSystemNames = sourceSystemProvider.GetSourceSystem(requestBody);
        var userSourceSystem = context.GetCurrentUserSourceSystem();

        if (!sourceSystemNames.Any() || string.IsNullOrWhiteSpace(userSourceSystem))
            await _next(context);


        foreach (var sourceSystem in sourceSystemNames)
        {
            if (sourceSystem.ToLower() != userSourceSystem.ToLower())
            {
                // Set bad Request in the request
                // return from here
            }
        }

        await _next(context);
    }
}
