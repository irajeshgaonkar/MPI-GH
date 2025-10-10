namespace HCA.Models.MuleSoft.Response
{
    /// <summary>
    /// SearchNotificationsResponse
    /// </summary>
    public class SearchNotificationsResponse : MuleSoftResponse
    {
        /// <summary>
        /// SearchNotifications content <see cref="SearchNotificationsResponseContent"/>
        /// </summary>
        public SearchNotificationsResponseContent Content { get; set; }
    }

    /// <summary>
    /// SearchNotificationsResponseContent
    /// </summary>
    public class SearchNotificationsResponseContent
    {
        /// <summary>
        /// Indicates whether there is a next page of results
        /// </summary>
        public bool HasNext { get; set; }

        /// <summary>
        /// Total elements in the result
        /// </summary>
        public int TotalElements { get; set; }

        /// <summary>
        /// List of notifications
        /// </summary>
        public List<Notification> Notifications { get; set; }

        /// <summary>
        /// Customer that notifications belong to
        /// </summary>
        public string CustomerId { get; set; }
    }

    /// <summary>
    /// Notification
    /// </summary>
    public class Notification
    {
        /// <summary>
        /// Timestamp in epoch format
        /// </summary>
        public long Ts { get; set; }

        /// <summary>
        /// Service that generated the notification
        /// </summary>
        public string Service { get; set; }

        /// <summary>
        /// Type of notification
        /// </summary>
        public string NotificationType { get; set; }

        /// <summary>
        /// Notification body
        /// </summary>
        public string Body { get; set; }

        /// <summary>
        /// User, whose action generated the notification
        /// </summary>
        public string Username { get; set; }
    }
}
