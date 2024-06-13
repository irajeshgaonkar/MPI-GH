using Amazon.DynamoDBv2.Model;
using HCA.Infrastructure.DynamoDb;
using HCA.Models.DynamoDb;

namespace HCA.Core.Services;

public class NotificationService : INotificationService
{
    private readonly HcaDynamoDbClient _hcaDynamoDbClient;

    public NotificationService()
    {
        _hcaDynamoDbClient = new HcaDynamoDbClient();
    }

    public async Task<(IEnumerable<HcaMpiNotification>, string)> GetAllNotifications(NotificationFilter notificationFilter, int? pageSize = null, string? startKey = null)
    {
        //var returnValue = new List<HcaMpiNotification>();
        //var query = BuildQueryRequest(notificationFilter, pageSize, startKey);
        //var (items, lastEvaluatedKey) = await _hcaDynamoDbClient.ScanAsync(query);


        //for (int i = 0; i < items.Count && i < items.Count; i++)
        //{
        //    var notification = new HcaMpiNotification();
        //    foreach (var attribute in items[i])
        //    {
        //        if (attribute.Key == DynamoDbNotificationColumnNames.LinkId) notification.LinkId = attribute.Value.S;
        //        if (attribute.Key == DynamoDbNotificationColumnNames.TrackingId) notification.TrackingId = attribute.Value.S;
        //        if (attribute.Key == DynamoDbNotificationColumnNames.TimeStamp) notification.TimeStamp = DateTime.Parse(attribute.Value.S);
        //        if (attribute.Key == DynamoDbNotificationColumnNames.SourceSystemName) notification.SourceSystemName = attribute.Value.S;
        //        if (attribute.Key == DynamoDbNotificationColumnNames.SourceSystemName) notification.SourceSystemName = attribute.Value.S;
        //        if (attribute.Key == DynamoDbNotificationColumnNames.SourceSystemId) notification.SourceSystemId = attribute.Value.S;
        //        if (attribute.Key == DynamoDbNotificationColumnNames.Operation) notification.Operation = attribute.Value.S;
        //        if (attribute.Key == DynamoDbNotificationColumnNames.Request) notification.Request = attribute.Value.S;
        //        if (attribute.Key == DynamoDbNotificationColumnNames.Response) notification.Response = attribute.Value.S;
        //        if (attribute.Key == DynamoDbNotificationColumnNames.PreviousLinkId) notification.PreviousLinkId = attribute.Value.S;
        //    }

        //    returnValue.Add(notification);
        //}

        //lastEvaluatedKey.TryGetValue(DynamoDbNotificationColumnNames.LinkId, out var linkId);
        //lastEvaluatedKey.TryGetValue(DynamoDbNotificationColumnNames.TrackingId, out var trackingId);
        //var lastEvaluatedKeyStr = $"{linkId?.S},{trackingId?.S}";
        //return (returnValue, lastEvaluatedKeyStr);
        return await Task.FromResult<(IEnumerable<HcaMpiNotification>, string)>((null, null));
    }

    private ScanRequest BuildQueryRequest(NotificationFilter notificationFilter, int? pageSize = null, string? exclusiveStartKey = null)
    {
        //var request = new ScanRequest(DynamoDbTableNames.Notification);

        //if (notificationFilter.SourceName != null)
        //{
        //    request.ScanFilter.Add(DynamoDbNotificationColumnNames.SourceSystemName,
        //        new Condition() { ComparisonOperator = Amazon.DynamoDBv2.ComparisonOperator.EQ, AttributeValueList = new List<AttributeValue>() { new AttributeValue { S = notificationFilter.SourceName } } });
        //};

        //if (notificationFilter.SourceId != null)
        //{
        //    request.ScanFilter.Add(DynamoDbNotificationColumnNames.SourceSystemId,
        //        new Condition() { ComparisonOperator = Amazon.DynamoDBv2.ComparisonOperator.EQ, AttributeValueList = new List<AttributeValue>() { new AttributeValue { S = notificationFilter.SourceId } } });
        //};

        //if (notificationFilter.TrackingId != null)
        //{
        //    request.ScanFilter.Add(DynamoDbNotificationColumnNames.TrackingId,
        //        new Condition() { ComparisonOperator = Amazon.DynamoDBv2.ComparisonOperator.EQ, AttributeValueList = new List<AttributeValue>() { new AttributeValue { S = notificationFilter.TrackingId } } });
        //};

        //if (notificationFilter.LinkId != null)
        //{
        //    request.ScanFilter.Add(DynamoDbNotificationColumnNames.LinkId,
        //        new Condition() { ComparisonOperator = Amazon.DynamoDBv2.ComparisonOperator.EQ, AttributeValueList = new List<AttributeValue>() { new AttributeValue { S = notificationFilter.LinkId } } });
        //};

        //if (notificationFilter.OperationType != null)
        //{
        //    request.ScanFilter.Add(DynamoDbNotificationColumnNames.Operation,
        //        new Condition() { ComparisonOperator = Amazon.DynamoDBv2.ComparisonOperator.EQ, AttributeValueList = new List<AttributeValue>() { new AttributeValue { S = notificationFilter.OperationType } } });
        //};

        //if (exclusiveStartKey != null)
        //{
        //    var splits = exclusiveStartKey.Split(',');

        //    if (splits.Length == 2)
        //    {
        //        var linkId = splits[0];
        //        var trackingId = splits[1];

        //        if (!string.IsNullOrWhiteSpace(linkId) && !string.IsNullOrWhiteSpace(trackingId))
        //        {
        //            request.ExclusiveStartKey = new Dictionary<string, AttributeValue>()
        //            {
        //                { DynamoDbNotificationColumnNames.LinkId, new AttributeValue { S = linkId } },
        //                { DynamoDbNotificationColumnNames.TrackingId, new AttributeValue { S = trackingId } }
        //            };
        //        }
        //    }
        //}

        //if (pageSize != null)
        //    request.Limit = pageSize ?? 0;

        //return request;

        return null;
    }
}

