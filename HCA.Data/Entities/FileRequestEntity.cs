using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace HCA.Data.Entities;

/// <summary>
/// Batch Processing File Request Details
/// </summary>
[Table("file_requests")]
public class FileRequestEntity : BaseEntity
{
    /// <summary>
    /// Primary key, File request Id
    /// </summary>
    [Key]
    [Column("file_request_id")]
    public int Id { get; set; }

    /// <summary>
    /// Uniquely identifies the request, Foreign key for the all the request
    /// </summary>
    [Column("request_id")]
    public Guid RequestId { get; set; }

    /// <summary>
    /// File Name
    /// </summary>
    [Column("file_name")]
    [Required]
    [MaxLength(1024)]
    public string FileName { get; set; }

    /// <summary>
    /// Records count to process in the request
    /// </summary>
    [Column("records_count")]
    public int RecordsCount { get; set; }

    /// <summary>
    /// Operation to be performed on the File
    /// </summary>
    [Column("operation_type")]
    [MaxLength(40)]
    public string OperationType { get; set; }

    /// <summary>
    /// Requested Date time
    /// </summary>
    [Column("request_date_time")]
    public DateTime RequestDateTime { get; set; }

    /// <summary>
    /// Process Start Time
    /// </summary>
    [Column("process_start_time")]
    public DateTime? ProcessStartTime { get; set; }

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
    public string Status { get; set; }

    /// <summary>
    /// Human Readable message
    /// </summary>
    [Column("message")]
    public string Message { get; set; }

    /// <summary>
    /// Output File Name
    /// </summary>
    [Column("output_file_name")]
    [MaxLength(1024)]
    public string? OutputFileName { get; set; }
}