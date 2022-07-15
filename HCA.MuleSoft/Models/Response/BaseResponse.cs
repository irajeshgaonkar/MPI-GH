namespace HCA.MuleSoft.Models.Response;

/// <summary>
/// Base Response
/// </summary>
/// <typeparam name="T">Response Content</typeparam>
public class BaseResponse<T>
{
    public BaseResponse(string trackingId, Guid auditId, bool retryableError, string message, List<string> errors, T content)
    {
        TrackingId = trackingId;
        AuditId = auditId;
        RetryableError = retryableError;
        Message = message;
        Errors = errors;
        Content = content;
    }

    /// <summary>
    /// Unique Id for tracking the request
    /// </summary>
    public string TrackingId { get; set; }

    /// <summary>
    /// Id for debugging on the Verato api
    /// </summary>
    public Guid AuditId { get; set; }

    /// <summary>
    /// Indicates whether it is re triable error or not
    /// </summary>
    public bool RetryableError { get; set; }

    /// <summary>
    /// Human readable message, in case of success
    /// </summary>
    public string Message { get; set; }

    /// <summary>
    /// Human readable error message, in case of failure
    /// </summary>
    public List<string> Errors { get; set; }

    /// <summary>
    /// Response for the request
    /// </summary>
    public T Content { get; set; }
}

