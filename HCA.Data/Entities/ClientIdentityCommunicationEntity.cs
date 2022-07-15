using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace HCA.Data.Entities;

/// <summary>
/// Table for storing the client identity communication details
/// </summary>
[Table("client_identity_communication")]
public class ClientIdentityCommunicationEntity : BaseEntity
{
    /// <summary>
    /// primary key
    /// </summary>
    [Key]
    [Column("client_identity_communication_id")]
    public int Id { get; set; }

    /// <summary>
    /// Universal link id
    /// </summary>
    [Column("mpi_link_id")]
    [Required]
    [MaxLength(1024)]
    public string MpiLinkId { get; set; }

    /// <summary>
    /// Source system Name
    /// </summary>
    [Column("source_system_name")]
    [Required]
    [MaxLength(100)]
    public string SourceSystemName { get; set; }

    /// <summary>
    /// Source System id
    /// </summary>
    [Column("source_system_id")]
    [Required]
    [MaxLength(100)]
    public string SourceSystemId { get; set; }

    /// <summary>
    /// Phone Type
    /// </summary>
    [Column("phone_type")]
    [MaxLength(40)]
    [Required]
    public string PhoneType { get; set; }

    /// <summary>
    /// Email Type
    /// </summary>
    [Column("email_type")]
    [MaxLength(40)]
    [Required]
    public string EmailType { get; set; }

    /// <summary>
    /// Phone Number
    /// </summary>
    [Column("phone_number")]
    [MaxLength(20)]
    public string PhoneNumber { get; set; }

    /// <summary>
    /// Email Address
    /// </summary>
    [Column("email_address")]
    [MaxLength(255)]
    public string EmailAddress { get; set; }

    /// <summary>
    /// Source System Update Date
    /// </summary>
    [Column("source_system_updated")]
    public DateTime SourceSystemUpdated { get; set; }

    /// <summary>
    /// Expiry Date
    /// </summary>
    /// <remarks>used when the identity is deleted from Verato</remarks>
    [Column("expiry_date")]
    public DateTime? ExpiryDate { get; set; }

    /// <summary>
    /// Indicates whether address is active or not (deleted from verato)
    /// </summary>
    [Column("is_active")]
    public bool IsActive { get; set; }

    /// <summary>
    /// Indicates wheter address id deleted
    /// </summary>
    [Column("is_delete")]
    public bool IsDelete { get; set; }

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
    public string? UpdatedBy { get; set; }

    /// <summary>
    /// Updated Date Time
    /// </summary>
    [Column("updated_date")]
    public DateTime? UpdatedDate { get; set; }

    /// <summary>
    /// Client Identity
    /// </summary>
    public ClientIdentityEntity ClientIdentity { get; set; }

    /// <summary>
    /// Client Identity Address Details
    /// </summary>
    public List<ClientIdentityAddressCommunicationEntity> AddressCommunications { get; set; }
}

