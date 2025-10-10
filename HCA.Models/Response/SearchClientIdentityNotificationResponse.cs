namespace HCA.Models.Response
{
    /// <summary>
    /// SearchClientIdentityNotificationResponse
    /// </summary>
    public class SearchClientIdentityNotificationResponse: BaseResponse
    {
        /// <summary>
        /// collection of search results <see cref="SearchClientIdentityNotificationContent"/>
        /// </summary>
        public SearchClientIdentityNotificationContent Content { get; set; } = new SearchClientIdentityNotificationContent();
    }

    /// <summary>
    /// SearchClientIdentityNotificationResponseContent class represents the content of the search client identity notification response
    /// </summary>
    public class SearchClientIdentityNotificationContent
    {
        /// <summary>
        /// Indicates whether there are more notifications to fetch
        /// </summary>
        public bool HasNext { get; set; }
        /// <summary>
        /// Total number of notifications available
        /// </summary>
        public int TotalElements { get; set; }
        /// <summary>
        /// List of notifications
        /// </summary>
        public List<ClientIdentityNotification> Notifications { get; set; }
        /// <summary>
        /// Customer Id associated with the notifications
        /// </summary>
        public string CustomerId { get; set; }
    }

    /// <summary>
    /// ClientIdentityNotification
    /// </summary>
    public class ClientIdentityNotification
    {
        /// <summary>
        /// Timestamp of the notification
        /// </summary>
        public long Ts { get; set; }
        /// <summary>
        /// Service that generated the notification
        /// </summary>
        public string Service { get; set; }
        /// <summary>
        /// Type of the notification
        /// </summary>
        public string NotificationType { get; set; }
        /// <summary>
        /// Body of the notification
        /// </summary>
        public string Body { get; set; }
        /// <summary>
        /// Username associated with the notification
        /// </summary>
        public string Username { get; set; }
    }
}
