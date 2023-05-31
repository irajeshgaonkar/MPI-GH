using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HCA.Data.Entities
{
    /// <summary>
    /// service_accounts Details
    /// </summary>
    [Table("service_accounts")]
    public class ServiceAccountEntity : BaseEntity
    {
        /// <summary>
        /// Primary key for service accounts
        /// </summary>
        [Key]
        [Column("id")]
        public int Id { get; set; }

        /// <summary>
        /// Source system Name
        /// </summary>
        [Column("source_system_name")]
        [Required]
        [MaxLength(1000)]
        public string SourceSystemName { get; set; }

        /// <summary>
        /// AppId
        /// </summary>
        [Column("app_id")]
        [Required]
        [MaxLength(1000)]
        public string AppId { get; set; }
    }
}
