namespace HCA.Models.Logging
{
    /// <summary>
    /// Standard Schema for LogItem
    /// </summary>
    public class LogItem
    {
        /// <summary>
        /// LogItem Name
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// TrackingId
        /// </summary>
        public string? TrackingId { get; set; }  

        /// <summary>
        /// LinkId
        /// </summary>
        public string? LinkId { get; set; }

        /// <summary>
        /// Layer - UI/API/Batch
        /// </summary>
        public string? Layer {  get; set; }

        /// <summary>
        /// CustomProperties
        /// </summary>
        public ExceptionCustomProperties? ExceptionCustomProperties { get; set; }

    }
}
