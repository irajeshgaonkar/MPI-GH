namespace HCA.Models.Enums;

/// <summary>
/// Request status
/// </summary>
public enum RequestStatus
{
    [StringValue("Not Started")]
    NotStarted,

    [StringValue("Processing")]
    Processing,

    [StringValue("Failed")]
    Failed,

    [StringValue("Success")]
    Success,

    [StringValue("Retrying")]
    Retrying,

    [StringValue("Data Loaded")]
    DataLoaded,

    [StringValue("Parsing")]
    Parsing,

    [StringValue("Parsing Failed")]
    ParsingFailed,
}

