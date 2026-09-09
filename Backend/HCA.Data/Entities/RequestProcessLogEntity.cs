using System.ComponentModel.DataAnnotations.Schema;

namespace HCA.Data.Entities;

/// <summary>
/// Request Process Logs
/// </summary>
[Table("request_process_logs")]
public class RequestProcessLogEntity : BaseEntity
{
    /// <summary>
    /// Primary Key, Request Process Log Entity Id
    /// </summary>
    [Column("request_process_log_id")]
    public int Id { get; set; }

    /// <summary>
    /// Uniquely identifies the process, Foreign key for the all the request
    /// </summary>
    [Column("tracking_id")]
    public string TrackingId { get; set; }

    /// <summary>
    /// Date Time fo the log
    /// </summary>
    [Column("date_time")]
    public DateTime DateTime { get; set; }

    /// <summary>
    /// Log Message
    /// </summary>
    [Column("status")]
    public string Status { get; set; }

    /// <summary>
    /// Log Message
    /// </summary>
    [Column("message")]
    public string Message { get; set; }
}
