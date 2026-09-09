namespace HCA.Models.Response
{
    /// <summary>
    /// 
    /// </summary>
    public class IdentityExistsResponse : BaseResponse
    {
        /// <summary>
        /// 
        /// </summary>
        public IdentityExistsContent Content { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public class IdentityExistsContent
    {
        /// <summary>
        /// 
        /// </summary>
        public bool Exists { get; set; }
    }
}
