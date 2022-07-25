using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HCA.Data.Entities;

/// <summary>
/// Table for maintaining the user request
/// </summary>
[Table("user_requests")]
public class UserRequestEntity : BaseEntity
{
    /// <summary>
    /// Primary key, User request Id
    /// </summary>
    [Key]
    [Column("user_request_id")]
    public int Id { get; set; }

    /// <summary>
    /// Uniquely identifies the request, Foreign key for the all the request
    /// </summary>
    [Column("tracking_id")]
    [MaxLength(1024)]
    [Required]
    public string TrackingId { get; set; }

    /// <summary>
    /// File Name
    /// </summary>
    [Column("user_id")]
    [Required]
    public int UserId { get; set; }

    /// <summary>
    /// Request Json
    /// </summary>
    [Column("request_json")]
    [Required]
    public string RequestJson { get; set; }

    /// <summary>
    /// Requested Date time
    /// </summary>
    [Column("request_date_time")]
    [Required]
    public DateTime RequestDateTime { get; set; }

    /// <summary>
    /// Process Start Time
    /// </summary>
    [Column("process_start_time")]
    [Required]
    public DateTime ProcessStartTime { get; set; }

    /// <summary>
    /// Process End Time
    /// </summary>
    [Column("process_end_time")]
    public DateTime? ProcessEndTime { get; set; }

    /// <summary>
    /// Process Status
    /// </summary>
    /// <remarks>Values: Not Started, Pending, Processing, Failed, Succeded</remarks>
    [Column("status")]
    [MaxLength(40)]
    [Required]
    public string Status { get; set; }

    /// <summary>
    /// Human Readable message
    /// </summary>
    [Column("message")]
    [Required]
    public string Message { get; set; }

    /// <summary>
    /// Human Readable message
    /// </summary>
    [Column("retry_count")]
    [Required]
    public int RetryCount { get; set; }
}