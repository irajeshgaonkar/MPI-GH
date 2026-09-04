using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HCA.Data.Entities
{
    [Table("onboarded_system")]
    public class OnboardedSystemEntity
    {
        /// <summary>
        /// Unique identifier for the onboarded system
        /// </summary>
        [Key]
        [Column("id")]
        [Required]
        public int Id { get; set; }

        /// <summary>
        /// The name of the source system that was onboarded
        /// </summary>
        [Column("source_system_name")]
        [Required]
        public string SourceSystemName { get; set; }

        /// <summary>
        /// The agency associated with the onboarded system
        /// </summary>
        [Column("agency_name")]
        [Required]
        public string AgencyName { get; set; }

        /// <summary>
        /// The tenant that owns or uses the onboarded system
        /// </summary>
        [Column("tenant")]
        public string? Tenant { get; set; }

        /// <summary>
        /// The mode of connectivity used by the onboarded system
        /// </summary>
        [Column("connectivity_mode")]
        public string? ConnectivityMode { get; set; }

        /// <summary>
        /// The date when the system was onboarded
        /// </summary>
        [Column("onboarding_date")]
        [Required]
        public DateTime OnboardingDate { get; set; }

        /// <summary>
        /// The user who created the onboarded system entry
        /// </summary>
        [Column("created_by")]
        [Required]
        public string CreatedBy { get; set; }

        /// <summary>
        /// The timestamp of when the record was created
        /// </summary>
        [Column("created_date")]
        [Required]
        public DateTime CreatedDate { get; set; }

        /// <summary>
        /// The user who last modified the onboarded system entry
        /// </summary>
        [Column("modified_by")]
        public string? ModifiedBy { get; set; }

        /// <summary>
        /// The timestamp of when the record was last modified
        /// </summary>
        [Column("modified_date")]
        public DateTime? ModifiedDate { get; set; }

        /// <summary>
        /// A flag indicating whether the system is active (true) or inactive (false)
        /// </summary>
        [Column("is_active")]
        public bool? IsActive { get; set; }

        /// <summary>
        /// The starting IP address associated with the system
        /// </summary>
        [Column("start_ip_address")]
        public string? StartIpAddress { get; set; }

        /// <summary>
        /// The ending IP address associated with the system
        /// </summary>
        [Column("end_ip_address")]
        public string? EndIpAddress { get; set; }

        /// <summary>
        /// The CIDR notation for the IP address range
        /// </summary>
        [Column("ip_address_cidr")]
        public string? IpCidr { get; set; }

        /// <summary>
        /// A flag indicating whether notifications are enabled for the system
        /// </summary>
        [Column("enable_notification")]
        public bool? EnableNotification { get; set; }

        public virtual ICollection<DataShareMappingEntity> SourceSystemMappings { get; set; } 
        public virtual ICollection<DataShareMappingEntity> AllowedSystemMappings { get; set; }
    }
}
