namespace HCA.Models.Verato.Response;

/// <summary>
/// Verato Base Response
/// </summary>
public class VeratoResponse
{
    /// <summary>
    /// Unique Id for tracking the request
    /// </summary>
    public string TrackingId { get; set; }

    /// <summary>
    /// Id for debugging on the Verato api
    /// </summary>
    public Guid AuditId { get; set; }

    /// <summary>
    /// Indicates whether it is a success or not
    /// </summary>
    public bool Success { get; set; }

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
}

