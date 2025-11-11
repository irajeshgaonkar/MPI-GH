namespace HCA.Models.Response
{
    /// <summary>
    /// Standard Error Response with schema
    /// </summary>
    public class APIErrorResponse : BaseResponse
    {
        /// <summary>
        /// Status Code for Error
        /// </summary>
        public string ErroCode { get; set; }
    }
}
