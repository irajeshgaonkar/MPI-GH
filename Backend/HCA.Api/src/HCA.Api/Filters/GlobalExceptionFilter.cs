using System.Net;
using HCA.Infrastructure.Exceptions;
using HCA.Infrastructure.Logger;
using HCA.Models.Response;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace HCA.Api.Filters;

public class GlobalExceptionFilter( IAppLogger logger ) : IExceptionFilter
{
    private readonly IAppLogger _logger = logger;

    public void OnException( ExceptionContext context )
    {
        var exception = context.Exception;
        string trackingId = (context.HttpContext.Items.TryGetValue("TrackingId", out var trackingIdValue) ? trackingIdValue?.ToString() : "") ?? "";
        var auditId = Guid.NewGuid();

        _logger.LogError( exception, $"Unhandled exception occurred. TrackingId: {trackingId}" );

        var statusCode = (int)HttpStatusCode.InternalServerError;

        var response = new APIErrorResponse
        {
            Success = false,
            TrackingId = trackingId,
            AuditId = auditId,
            RetryableError = false,
            Errors = [exception.Message]
        };

        switch( exception )
        {
            case HcaBadRequestException:
                statusCode = (int)HttpStatusCode.BadRequest;
                response.Message = "The request could not be processed due to invalid input. Please verify your data and try again.";
                response.ErrorCode = "400";
                break;

            case HcaVeratoException:
                statusCode = (int)HttpStatusCode.InternalServerError;
                response.Message = "An unexpected error occurred on Verato. Please try again later.";
                response.ErrorCode = "500";
                break;

            case TimeoutException:
                statusCode = (int)HttpStatusCode.RequestTimeout;
                response.Message = "The request timed out. Please try again later.";
                response.ErrorCode = "408";
                response.RetryableError = true;
                break;

            case IPValidationException:
                statusCode = (int)HttpStatusCode.Unauthorized;
                response.Message = exception.Message;
                response.ErrorCode = "401";
                break;

            default:
                response.Message = "An unexpected error occurred. Please try again later.";
                response.ErrorCode = "500";
                break;
        }

        context.Result = new ObjectResult( response )
        {
            StatusCode = statusCode
        };

        context.ExceptionHandled = true;
    }
}
