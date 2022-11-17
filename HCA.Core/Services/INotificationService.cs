using HCA.Models.DynamoDb;

namespace HCA.Core.Services;

public interface INotificationService
{
    Task<IEnumerable<HcaMpiNotification>> GetNotifications(string linkId);
    Task<IEnumerable<HcaMpiNotification>> GetNotifications();
}

