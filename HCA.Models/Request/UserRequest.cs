namespace HCA.Models.Request;

public class UserRequest
{
    /// <summary>
    /// Primary key, User request Id
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Uniquely identifies the request, Foreign key for the all the request
    /// </summary>
    public string TrackingId { get; set; }

    /// <summary>
    /// File Name
    /// </summary>
    public string ApiCallType { get; set; }

    /// <summary>
    /// File Name
    /// </summary>
    public string UserName { get; set; }

    /// <summary>
    /// Request Json
    /// </summary>
    public string RequestJson { get; set; }

    /// <summary>
    /// Request Json
    /// </summary>
    public string ResponseJson { get; set; }

    /// <summary>
    /// Request Json
    /// </summary>
    public string? NotificationOptions { get; set; }

    /// <summary>
    /// Requested Date time
    /// </summary>
    public DateTime RequestDateTime { get; set; }

    /// <summary>
    /// Process Start Time
    /// </summary>
    public DateTime ProcessStartTime { get; set; }

    /// <summary>
    /// Process End Time
    /// </summary>
    public DateTime? ProcessEndTime { get; set; }

    /// <summary>
    /// Process Status
    /// </summary>
    /// <remarks>Values: Not Started, Pending, Processing, Failed, Succeded</remarks>
    public string Status { get; set; }

    /// <summary>
    /// Human Readable message
    /// </summary>
    public string Message { get; set; }

    /// <summary>
    /// Human Readable message
    /// </summary>
    public int RetryCount { get; set; }
}

