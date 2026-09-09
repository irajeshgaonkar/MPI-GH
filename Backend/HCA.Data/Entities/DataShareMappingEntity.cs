using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HCA.Data.Entities
{
    [Table("data_share_mapping")]
    public class DataShareMappingEntity
    {
        [Key]
        [Column("mapping_id")]
        public int MappingId { get; set; }

        [Column("source_system_id")]
        public int SourceSystemId { get; set; }

        [Column("allowed_system_id")]
        public int AllowedSystemId { get; set; }

        [Column("data_sharing_level")]
        public string DataSharingLevel { get; set; }

        [Column("created_by")]
        public string CreatedBy { get; set; }

        [Column("created_date")]
        public DateTime CreatedDate { get; set; }

        [Column("modified_by")]
        public string? ModifiedBy { get; set; }

        [Column("modified_date")]
        public DateTime? ModifiedDate { get; set; }

        [Column("is_active")]
        public bool IsActive { get; set; }

        // Navigation properties
        public virtual OnboardedSystemEntity SourceSystem { get; set; }
        public virtual OnboardedSystemEntity AllowedSystem { get; set; }
    }
}
