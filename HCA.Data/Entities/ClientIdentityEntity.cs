using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using HCA.Infrastructure.Converters;

namespace HCA.Data.Entities;

/// <summary>
/// Client Identity Details
/// </summary>
[Table("client_identity")]
public class ClientIdentityEntity : BaseEntity
{
    /// <summary>
    /// Primary key for client identity address
    /// </summary>
    [Key]
    [Column("client_identity_id")]
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
    /// Source System Agency
    /// </summary>
    [Column("source_system_agency")]
    [Required]
    [MaxLength(100)]
    public string SourceSystemAgency { get; set; }

    /// <summary>
    /// First Name
    /// </summary>
    [Column("first_name")]
    [Required]
    [MaxLength(40)]
    public string FirstName { get; set; }

    /// <summary>
    /// Middle Name
    /// </summary>
    [Column("middle_name")]
    [MaxLength(40)]
    public string? MiddleName { get; set; }

    /// <summary>
    /// Last Name
    /// </summary>
    [Column("last_name")]
    [Required]
    [MaxLength(40)]
    public string LastName { get; set; }

    /// <summary>
    /// Name Suffix
    /// </summary>
    [Column("name_suffix")]
    [MaxLength(40)]
    public string? NameSuffix { get; set; }

    /// <summary>
    /// Social Security Number
    /// </summary>
    [Column("ssn")]
    [Required]
    [MaxLength(12)]
    public string? Ssn { get; set; }

    /// <summary>
    /// Date of Birth
    /// </summary>
    [Column("dob")]
    [JsonConverter(typeof(DateOnlyJsonConverter))]
    public DateOnly? Dob { get; set; }

    /// <summary>
    /// Gender
    /// </summary>
    [Column("gender")]
    [MaxLength(10)]
    [Required]
    public string Gender { get; set; }

    /// <summary>
    /// Protected Population flag
    /// </summary>
    [Column("protected_population_flag")]
    [Required]
    public bool ProtectedPopulationFlag { get; set; }

    /// <summary>
    /// Protected Population type
    /// </summary>
    [Column("protected_population_type")]
    [MaxLength(100)]
    [Required]
    public string ProtectedPopulationType { get; set; }

    /// <summary>
    /// Updated date time in mpi
    /// </summary>
    [Column("mpi_updated")]
    public DateTime? MpiUpdated { get; set; }

    /// <summary>
    /// Source System Update Date
    /// </summary>
    [Column("source_system_updated")]
    [Required]
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
    [Required]
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
    /// Client Address Details
    /// </summary>
    //[ForeignKey("client_identity_id")]
    public List<ClientIdentityAddressEntity> Addresses { get; set; }

    /// <summary>
    /// Client Address Details
    /// </summary>
    //[ForeignKey("client_identity_id")]
    public List<ClientIdentityCommunicationEntity> Communications { get; set; }
}