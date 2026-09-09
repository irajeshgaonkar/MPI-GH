using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HCA.Data.Entities;

/// <summary>
/// Client Identity Address Details
/// </summary>
[Table("client_identity_address")]
public class ClientIdentityAddressEntity : BaseEntity
{
    /// <summary>
    /// Primary key for client identity address
    /// </summary>
    [Key]
    [Column("client_identity_address_id")]
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

    private string _zipCode;

    /// <summary>
    /// Zip Code
    /// </summary>
    [Column("zip_code")]
    [MaxLength(9)]
    public string ZipCode
    {
        get { return _zipCode; }
        set
        {
            if (string.IsNullOrEmpty(value))
            {
                _zipCode = "";
            }
            else
            {
                _zipCode = value[..Math.Min(9, value.Length)];
            }
        } }

    /// <summary>
    /// Zip Plus Four
    /// </summary>
    [Column("zip_four")]
    [MaxLength(4)]
    public string ZipFour { get; set; }

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
    [Required]
    [MaxLength(255)]
    public string CreatedBy { get; set; }

    /// <summary>
    /// Created Date time
    /// </summary>
    [Column("created_date")]
    [Required]
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

    private ClientIdentityEntity _clientIdentity;
    /// <summary>
    /// Client Identity
    /// Automatically populates available fields
    /// </summary>
    public ClientIdentityEntity ClientIdentity
    {
        get { return _clientIdentity; }
        set
        {
            _clientIdentity = value;
            MpiLinkId = value.MpiLinkId;
            SourceSystemName = value.SourceSystemName;
            SourceSystemId = value.SourceSystemId;
            SourceSystemUpdated = value.SourceSystemUpdated;
        }
    }

    /// <summary>
    /// Client Identity Id
    /// </summary>
    public List<ClientIdentityAddressCommunicationEntity> AddressCommunications { get; set; }
}
