using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HCA.Data.Entities
{
    /// <summary>
    /// Ip Address Details
    /// </summary>
    [Table("ip_addresses")]
    public class IpAddressesEntity : BaseEntity
    {
        /// <summary>
        /// Primary key for ip address
        /// </summary>
        [Key]
        [Column("ip_address_id")]
        public int Id { get; set; }

        /// <summary>
        /// Source System
        /// </summary>
        [Column("source_system")]
        [Required]
        [MaxLength(50)]
        public string SourceSystem { get; set; }

        /// <summary>
        /// ip address
        /// </summary>
        [Column("ip_address")]
        [Required]
        public string IpAddress { get; set; }

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
    }
}
