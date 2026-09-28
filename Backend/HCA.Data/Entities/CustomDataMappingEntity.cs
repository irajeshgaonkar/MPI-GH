using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HCA.Data.Entities
{
    /// <summary>
    /// Table for maintaining the file_responses
    /// </summary>
    [Table("custom_data_mapping")]
    public class CustomDataMappingEntity : BaseEntity
    {
        /// <summary>
        /// Primary key, id
        /// </summary>
        [Key]
        [Column("id")]
        public int Id { get; set; }

        /// <summary>
        /// source_system_name
        /// </summary>
        [Column("source_system_name")]
        [MaxLength(1000)]
        public string SourceSystemName { get; set; }

        /// <summary>
        /// InputIndex
        /// </summary>
        [Column("input_index")]
        public int InputIndex { get; set; }
        /// <summary>
        /// input_column_name
        /// </summary>
        [Column("input_column_name")]
        [MaxLength(3000)]
        public string InputColumnName { get; set; }

        /// <summary>
        /// verato_request_path
        /// </summary>
        [Column("verato_request_path")]
        [MaxLength(3000)]
        public string VeratoRequestPath { get; set; }

        /// <summary>
        /// verato_response_path
        /// </summary>
        [Column("verato_response_path")]
        [MaxLength(3000)]
        public string VeratoResponsePath { get; set; }

        /// <summary>
        /// api_response_path
        /// </summary>
        [Column("api_response_path")]
        [MaxLength(3000)]
        public string APIResponsePath { get; set; }


        /// <summary>
        /// OutputIndex
        /// </summary>
        [Column("output_index")]
        public int OutputIndex { get; set; }


        /// <summary>
        /// output_column_name
        /// </summary>
        [Column("output_column_name")]
        [MaxLength(3000)]
        public string OutputColumnName { get; set; }
    }
}
