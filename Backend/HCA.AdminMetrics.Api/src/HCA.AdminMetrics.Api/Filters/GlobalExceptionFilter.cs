using System.Net;
using HCA.Infrastructure.Logger;
using HCA.Models.Response;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace HCA.AdminMetrics.Api.Filters;

/// <summary>
/// Handles unhandled exceptions and converts them into a standardized API error response.
/// </summary>
public class GlobalExceptionFilter(IAppLogger logger) : IExceptionFilter
{
    private readonly IAppLogger _logger = logger;

    /// <summary>
    /// Executes when an unhandled exception occurs during request processing.
    /// </summary>
    /// <param name="context">The exception context.</param>
    public void OnException(ExceptionContext context)
    {
        var exception = context.Exception;
        var auditId = Guid.NewGuid();

        _logger.LogError(exception, "Unhandled exception occurred in Admin Metrics API.");

        var response = new APIErrorResponse
        {
            Success = false,
            TrackingId = string.Empty,
            AuditId = auditId,
            RetryableError = false,
            ErrorCode = ((int)HttpStatusCode.InternalServerError).ToString(),
            Message = "An unexpected error occurred. Please try again later.",
            Errors = [exception.Message]
        };

        context.Result = new ObjectResult(response)
        {
            StatusCode = (int)HttpStatusCode.InternalServerError
        };

        context.ExceptionHandled = true;
    }
}
