namespace HCA.Api.Dto;

public class FileRequestDto
{
    /// <summary>
    /// Unique Id for the request
    /// </summary>
    public string RequestId { get; set; }

    /// <summary>
    /// Uniquely identifies the request, Foreign key for the all the request
    /// </summary>
    public string TrackingId { get; set; }

    /// <summary>
    /// File Name
    /// </summary>
    public string FileName { get; set; }

    /// <summary>
    /// Output File Name
    /// </summary>
    public string? OutputFileName { get; set; }

    /// <summary>
    /// Source System Agency
    /// </summary>
    public string SourceSystemAgency { get; set; }

    /// <summary>
    /// Source system Name
    /// </summary>
    public string SourceSystemName { get; set; }

    /// <summary>
    /// Records count to process in the request
    /// </summary>
    public int RecordsCount { get; set; }

    /// <summary>
    /// Records count to process in the request
    /// </summary>
    public string Trailer { get; set; }

    /// <summary>
    /// Operation to be performed on the File
    /// </summary>
    public string ApiCallType { get; set; }

    /// <summary>
    /// Requested Date time
    /// </summary>
    public DateTime FileCreatedDateTime { get; set; }

    /// <summary>
    /// Requested Date time
    /// </summary>
    public DateTime RequestDateTime { get; set; }

    /// <summary>
    /// Process Start Time
    /// </summary>
    public DateTime? ProcessStartTime { get; set; }

    /// <summary>
    /// Process End Time
    /// </summary>
    public DateTime? ProcessEndTime { get; set; }

    /// <summary>
    /// Process Status
    /// </summary>
    public string Status { get; set; }

    /// <summary>
    /// Human Readable message
    /// </summary>
    public string Message { get; set; }
}

