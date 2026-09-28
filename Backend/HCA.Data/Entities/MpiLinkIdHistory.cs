using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace HCA.Data.Entities;

/// <summary>
/// Mpi Link Id History
/// </summary>
[Table("mpi_link_id_history")]
public class MpiLinkIdHistoryEntity : BaseEntity
{
    /// <summary>
    /// Primary key for client identity address
    /// </summary>
    [Key]
    [Column("mpi_link_history_id")]
    public int Id { get; set; }

    /// <summary>
    /// Universal link id
    /// </summary>
    [Column("previous_mpi_link_id")]
    [Required]
    public string PreviousMpiLinkId { get; set; }

    /// <summary>
    /// Universal link id
    /// </summary>
    [Column("previous_source_system_name")]
    [Required]
    public string PreviousSourceSystemName { get; set; }

    /// <summary>
    /// Universal link id
    /// </summary>
    [Column("previous_source_system_id")]
    [Required]
    public string PreviousSourceSystemId { get; set; }

    /// <summary>
    /// Universal link id
    /// </summary>
    [Column("current_mpi_link_id")]
    [Required]
    public string CurrentMpiLinkId { get; set; }

    /// <summary>
    /// Universal link id
    /// </summary>
    [Column("current_source_system_name")]
    [Required]
    public string CurrentSourceSystemName { get; set; }

    /// <summary>
    /// Universal link id
    /// </summary>
    [Column("current_source_system_id")]
    [Required]
    public string CurrentSourceSystemId { get; set; }

    /// <summary>
    /// Created by User Name
    /// </summary>
    [Column("created_by")]
    [MaxLength(255)]
    public string CreatedBy { get; set; }

    /// <summary>
    /// Created Date time
    /// </summary>
    [Column("created_date")]
    public DateTime CreatedDate { get; set; }

    /// <summary>
    /// Updated by User Name
    /// </summary>
    [Column("updated_by")]
    [MaxLength(255)]
    public string UpdatedBy { get; set; }

    /// <summary>
    /// Updated Date Time
    /// </summary>
    [Column("updated_date")]
    public DateTime UpdatedDate { get; set; }
}

