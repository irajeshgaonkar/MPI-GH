using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace HCA.Data.Entities;

/// <summary>
/// Client Identity request entity for batch processing of data
/// </summary>
[Table("client_identity_requests")]
public class ClientIdentityRequestEntity : BaseEntity
{
    /// <summary>
    /// Primary Key for uniquely identifying the request
    /// </summary>
    [Key]
    [Column("client_identity_request_id")]
    public int Id { get; set; }

    /// <summary>
    /// Unique Id for the request - mapping to file request
    /// </summary>
    [Column("request_id")]
    [Required]
    [MaxLength(200)]
    public string RequestId { get; set; }

    /// <summary>
    /// Unique Id for the request - mapping to file request
    /// </summary>
    [Column("batch_number")]
    [Required]
    public int BatchNumber { get; set; }

    /// <summary>
    /// Unique Id for the request
    /// </summary>
    [Column("tracking_id")]
    [Required]
    [MaxLength(1024)]
    public string TrackingId { get; set; }

    /// <summary>
    /// Universal link id
    /// </summary>
    [Column("mpi_link_id")]
    [MaxLength(1024)]
    public string? MpiLinkId { get; set; }

    /// <summary>
    /// Source System Update Date
    /// </summary>
    [Column("source_system_updated")]
    [MaxLength(100)]
    [Required]
    public string SourceSystemUpdated { get; set; }

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
    [MaxLength(50)]
    public string? Dob { get; set; }

    /// <summary>
    /// Gender
    /// </summary>
    [Column("gender")]
    [MaxLength(10)]
    public string Gender { get; set; }

    /// <summary>
    /// Protected Population flag
    /// </summary>
    [Column("protected_population_flag")]
    [MaxLength(10)]
    public string ProtectedPopulationFlag { get; set; }

    /// <summary>
    /// Protected Population type
    /// </summary>
    [Column("protected_population_type")]
    [MaxLength(100)]
    public string ProtectedPopulationType { get; set; }

    /// <summary>
    /// Address Type
    /// </summary>
    [Column("address_type")]
    [Required]
    [MaxLength(40)]
    public string AddressType { get; set; }

    /// <summary>
    /// Address Line 1
    /// </summary>
    [Column("address_line_1")]
    [MaxLength(255)]
    [Required]
    public string AddressLine1 { get; set; }

    /// <summary>
    /// Address Line 2
    /// </summary>
    [Column("address_line_2")]
    [MaxLength(255)]
    public string? AddressLine2 { get; set; }

    /// <summary>
    /// Address Line 3
    /// </summary>
    [Column("address_line_3")]
    [MaxLength(255)]
    public string? AddressLine3 { get; set; }

    /// <summary>
    /// City
    /// </summary>
    [Column("city")]
    [Required]
    [MaxLength(100)]
    public string City { get; set; }

    /// <summary>
    /// State
    /// </summary>
    [Column("state")]
    [Required]
    [MaxLength(40)]
    public string State { get; set; }

    /// <summary>
    /// Zip Code
    /// </summary>
    [Column("zip_code")]
    [MaxLength(5)]
    [Required]
    public string ZipCode { get; set; }

    /// <summary>
    /// Zip Plus Four
    /// </summary>
    [Column("zip_four")]
    [MaxLength(4)]
    [Required]
    public string ZipFour { get; set; }


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
    [Required]
    public string PhoneNumber { get; set; }

    /// <summary>
    /// Email Address
    /// </summary>
    [Column("email_address")]
    [MaxLength(255)]
    [Required]
    public string EmailAddress { get; set; }

    /// <summary>
    /// Status of the request
    /// </summary>
    [Column("status")]
    public string? Status { get; set; }

    /// <summary>
    /// Human Readable message
    /// </summary>
    [Column("message")]
    [Required]
    public string Message { get; set; }

    /// <summary>
    /// Retry Count
    /// </summary>
    [Column("retry_count")]
    [Required]
    public int RetryCount { get; set; }
}

