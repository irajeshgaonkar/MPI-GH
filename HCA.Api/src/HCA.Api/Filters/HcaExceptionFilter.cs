using System.Net;
using HCA.Infrastructure.Exceptions;
using HCA.Infrastructure.Logger;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace HCA.Api.Filters;

public class HcaExceptionFilter : IExceptionFilter
{
    public void OnException(ExceptionContext context)
    {
        var appLogger = context.HttpContext.RequestServices.GetService(typeof(IAppLogger)) as IAppLogger;
        var exception = context.Exception;

        if (exception is HcaBadRequestException bre)
        {
            appLogger?.LogError(bre);
            context.Result = new JsonResult(bre.Message)
            {
                StatusCode = (int)HttpStatusCode.BadRequest
            };
        }

        else if (exception is HcaMuleSoftException mse)
        {
            appLogger?.LogError(mse);
            context.Result = new JsonResult(mse.Message)
            {
                StatusCode = (int)HttpStatusCode.BadRequest
            };
        }

        else
        {
            appLogger?.LogError(exception);
            context.Result = new JsonResult(exception.Message)
            {
                StatusCode = (int)HttpStatusCode.InternalServerError
            };
        }
    }
}
