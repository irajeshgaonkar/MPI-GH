using HCA.Models.DynamoDb;

namespace HCA.Core.Services;

public interface INotificationService
{
    Task<IEnumerable<HcaMpiNotification>> GetNotifications(string sourceName, string? linkId, string? sourceId, string? trackingId, int pageNumber = 0, int pageSize = 20);
}

