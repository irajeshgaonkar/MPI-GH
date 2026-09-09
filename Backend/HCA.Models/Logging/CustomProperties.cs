namespace HCA.Models.Logging
{
    /// <summary>
    /// CustomProperties Base
    /// </summary>
    public class CustomProperties
    {
        /// <summary>
        /// User in contex
        /// </summary>
        public string? User { get; set; }

        /// <summary>
        /// Agency in context
        /// </summary>
        public string? Agency { get; set; }

        /// <summary>
        /// SourceSystem in context
        /// </summary>
        public string? SourceSystem { get; set; }

        /// <summary>
        /// Role in context
        /// </summary>
        public string? Role { get; set; }
    }
}
