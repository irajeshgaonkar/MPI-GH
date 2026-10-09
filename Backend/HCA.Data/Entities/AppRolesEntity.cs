using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HCA.Data.Entities
{
    /// <summary>
    /// App Role details
    /// </summary>
    [Table("application_role")]
    public class AppRolesEntity : BaseEntity
    {
        /// <summary>
        /// Primary key for ip address
        /// </summary>
        [Key]
        [Column("app_role_id")]
        public int AppRoleId { get; set; }

        /// <summary>
        /// Role Name
        /// </summary>
        [Column("app_role_name")]
        [Required]
        [MaxLength(20)]
        public string AppRoleName { get; set; }

        /// <summary>
        /// Role description
        /// </summary>
        [Column("app_role_description")]
        [Required]
        public string AppRoleDescription { get; set; }

        public ICollection<AppRoleMappingEntity> AppRolesMappings { get; set; }
    }
}
