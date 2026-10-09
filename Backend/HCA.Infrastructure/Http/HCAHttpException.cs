using System.Net;

namespace HCA.Infrastructure.Http;

/// <summary>
/// Exception thrown when there is an error or exception in http / https request
/// </summary>
public class HcaHttpException : Exception
{
    /// <summary>
    /// Http status code
    /// </summary>
    public HttpStatusCode StatusCode { get; set; }

    /// <summary>
    /// <see cref="HcaHttpException"/>
    /// </summary>
    /// <param name="statusCode">Response status code</param>
    /// <param name="message">Error message</param>
    public HcaHttpException(HttpStatusCode statusCode, string message) : base($"{statusCode}: {message}")
    {
        StatusCode = statusCode;
    }
}

