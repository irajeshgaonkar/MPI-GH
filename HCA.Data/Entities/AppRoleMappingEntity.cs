using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HCA.Data.Entities
{
    /// <summary>
    /// App Role Mapping details
    /// </summary>
    [Table("app_roles_mapping")]
    public class AppRoleMappingEntity : BaseEntity
    {
        /// <summary>
        /// Primary key mapping
        /// </summary>
        [Key]
        [Column("mapping_id")]
        public int MappingId { get; set; }

        /// <summary>
        /// Role Id
        /// </summary>
        [Column("role_id")]
        [Required]
        [MaxLength(20)]
        public int RoleId { get; set; }

        /// <summary>
        /// Role Entra Group Name
        /// </summary>
        [Column("role_group_name")]
        [Required]
        public string RoleGroupName { get; set; }

        /// <summary>
        /// Role Entra Group Id
        /// </summary>
        [Column("role_group_id")]
        [Required]
        public string RoleGroupId { get; set; }

        /// <summary>
        /// Source System Id
        /// </summary>
        [Column("source_system_id")]
        [Required]
        public int SystemId { get; set; }

        /// <summary>
        /// Agency Id
        /// </summary>
        [Column("source_system_agency_id")]
        public int AgencyId { get; set; }

        public virtual AppRolesEntity AppRole { get; set; }

        public virtual OnboardedSystemEntity System { get; set; }
    }
}
