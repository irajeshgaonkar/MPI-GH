namespace HCA.Models.Logging
{
    /// <summary>
    /// Custom Properties for Exception
    /// </summary>
    public class ExceptionCustomProperties : CustomProperties
    {
        /// <summary>
        /// Function Name in context
        /// </summary>
        public string? FunctionName { get; set; }

        /// <summary>
        /// Error Message
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// Error Code
        /// </summary>
        public string? ErrorCode { get; set; }

        /// <summary>
        /// StackTrace
        /// </summary>
        public string? StackTrace { get; set; }
    }
}
