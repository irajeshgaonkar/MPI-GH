using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HCA.Data.Entities;

/// <summary>
/// Table for maintaining the user request
/// </summary>
[Table("sftp_file_transfer")]
public class SftpFileTransferEntity : BaseEntity
{
    /// <summary>
    /// Primary key, User request Id
    /// </summary>
    [Key]
    [Column("sftp_file_transfer_id")]
    public int Id { get; set; }


    /// <summary>
    /// User email
    /// </summary>
    [Column("path")]
    [Required]
    public string Path { get; set; }

    /// <summary>
    /// File name
    /// </summary>
    [Column("file_name")]
    [Required]
    public string FileName { get; set; }

    /// <summary>
    /// Last modified date time
    /// </summary>
    [Column("last_modified")]
    [Required]
    public DateTime LastModified { get; set; }

    /// <summary>
    /// File Transfer date time
    /// </summary>
    [Column("transfer_date_time")]
    [Required]
    public DateTime TransferDateTime { get; set; }

    /// <summary>
    /// Status
    /// </summary>
    [Column("status")]
    [Required]
    [MaxLength(100)]
    public string  Status { get; set; }

}