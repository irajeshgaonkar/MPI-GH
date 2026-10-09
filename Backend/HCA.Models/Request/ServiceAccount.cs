namespace HCA.Models.Request
{
    public class ServiceAccount
    {
        /// <summary>
        /// Primary key for service accounts
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Source system Name
        /// </summary>
        public string SourceSystemName { get; set; }

        /// <summary>
        /// AppId
        /// </summary>
        public string AppId { get; set; }
    }
}
