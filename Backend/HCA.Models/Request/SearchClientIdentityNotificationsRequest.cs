using HCA.Models.Enums;

namespace HCA.Models.Request
{
    /// <summary>
    /// Request to search for client identity notifications.
    /// </summary>
    public class SearchClientIdentityNotificationsRequest : BaseRequest
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SearchClientIdentityNotificationsRequest"/> class.
        /// </summary>
        /// <param name="trackingId"></param>
        public SearchClientIdentityNotificationsRequest(string trackingId) : base(ApiCallType.VESearchNotifications, trackingId)
        {
        }

        /// <summary>
        /// Content of the request containing search parameters.
        /// </summary>
        public SearchClientIdentityNotificationsRequestContent Content { get; set; }
    }

    /// <summary>
    /// Content for the search client identity notifications request.
    /// </summary>
    public class SearchClientIdentityNotificationsRequestContent
    {
        /// <summary>
        /// Page Number, zero index based.
        /// </summary>
        public int PageNumber { get; set; }

        /// <summary>
        /// Page Size
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// Start date for the search.
        /// </summary>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// End date for the search.
        /// </summary>
        public DateTime EndDate { get; set; }
    }
}
