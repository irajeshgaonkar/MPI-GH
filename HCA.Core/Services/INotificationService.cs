using Amazon.DynamoDBv2.Model;
using HCA.Models.DynamoDb;

namespace HCA.Core.Services;

public interface INotificationService
{
    Task<(IEnumerable<HcaMpiNotification>, string)> GetAllNotifications(NotificationFilter notificationFilter, int? pageSize = null, string? startKey = null);
}

public class NotificationFilter
{
    public string? LinkId { get; set; }

    public string? SourceName { get; set; }

    public string? SourceId { get; set; }

    public string? OperationType { get; set; }

    public string? TrackingId { get; set; }
}

