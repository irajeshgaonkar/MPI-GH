using HCA.Models.Enums;

namespace HCA.Models
{
    /// <summary>
    /// Data share mapping
    /// </summary>
    public class DataShareMapping
    {
        /// <summary>
        /// Allowed system for data sharing
        /// </summary>
        public string AllowedSystemName { get; set; }

        /// <summary>
        /// Data sharing level
        /// </summary>
        public string DataSharingLevel { get; set; }
    }
}
