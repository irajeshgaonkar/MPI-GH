using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HCA.Data.Entities;

/// <summary>
/// Table for maintaining the user request
/// </summary>
[Table("user_modify_records")]
public class UserModifyRecordsEntity : BaseEntity
{
    /// <summary>
    /// Primary key, User request Id
    /// </summary>
    [Key]
    [Column("user_modify_record_id")]
    public int Id { get; set; }

    /// <summary>
    /// User email
    /// </summary>
    [Column("user_name")]
    [Required]
    public string UserName { get; set; }

    /// <summary>
    /// Client Identity Record
    /// </summary>
    [Column("client_identity_id")]
    [Required]
    public int ClientIdentityId { get; set; }
}