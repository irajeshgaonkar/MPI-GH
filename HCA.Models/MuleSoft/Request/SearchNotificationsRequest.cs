namespace HCA.Models.MuleSoft.Request
{
    /// <summary>
    /// Request to search notifications
    /// </summary>
    public class SearchNotificationsRequest : MuleSoftRequest
    {
        /// <summary>
        /// SearchNotificationsRequest constructor
        /// </summary>
        /// <param name="trackingId"></param>
        /// <param name="content"></param>
        public SearchNotificationsRequest(string trackingId, SearchNotificationsRequestContent content) : base(trackingId)
        {
            Content = content;
        }

        /// <summary>
        /// Content of the request containing search parameters
        /// </summary>
        public SearchNotificationsRequestContent Content { get; set; }
    }

    /// <summary>
    /// Content for the SearchNotificationsRequest
    /// </summary>
    public class SearchNotificationsRequestContent
    {
        /// <summary>
        /// Page Number, Zero-Index based
        /// </summary>
        public int PageNumber { get; set; }

        /// <summary>
        /// Page Size, Number of records per page
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// Start Date for filtering notifications
        /// </summary>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// End Date for filtering notifications
        /// </summary>
        public DateTime EndDate { get; set; }

    }
}
