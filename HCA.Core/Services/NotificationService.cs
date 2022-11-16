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

    public async Task<IEnumerable<HcaMpiNotification>> GetNotifications(string sourceName, string? linkId, string? sourceId, string? trackingId, int pageNumber = 0, int pageSize = 20)
    {
        var returnValue = new List<HcaMpiNotification>();
        var query = BuildQueryRequest(sourceName, linkId, sourceId, trackingId, pageSize, pageNumber);
        var result = await _hcaDynamoDbClient.QueryAsync(query);

        if (result.Count < pageSize * pageNumber) return returnValue;
        
        for(int i = pageSize * pageNumber;i < result.Count && i <  pageSize * (pageNumber + 1); i++)
        {
            var notification = new HcaMpiNotification();
            foreach(var attribute in result[i])
            {
                if (attribute.Key == DynamoDbNotificationColumnNames.LinkId) notification.LinkId = attribute.Value.S;
                if (attribute.Key == DynamoDbNotificationColumnNames.TrackingId) notification.TrackingId = attribute.Value.S;
                if (attribute.Key == DynamoDbNotificationColumnNames.TimeStamp) notification.TimeStamp = DateTime.Parse(attribute.Value.S);
                if (attribute.Key == DynamoDbNotificationColumnNames.SourceSystemName) notification.SourceSystemName = attribute.Value.S;
                if (attribute.Key == DynamoDbNotificationColumnNames.SourceSystemId) notification.SourceSystemId = attribute.Value.S;
                if (attribute.Key == DynamoDbNotificationColumnNames.Operation) notification.Operation = attribute.Value.S;
                if (attribute.Key == DynamoDbNotificationColumnNames.Request) notification.Request = attribute.Value.S;
                if (attribute.Key == DynamoDbNotificationColumnNames.Response) notification.Response = attribute.Value.S;
                if (attribute.Key == DynamoDbNotificationColumnNames.PreviousLinkId) notification.PreviousLinkId = attribute.Value.S;
            }

            returnValue.Add(notification);
        }

        return returnValue;
    }

    private QueryRequest BuildQueryRequest(string sourceName, string? linkId, string? sourceId, string? trackingId, int pageSize, int pageNumber)
    {
        var request = new QueryRequest(DynamoDbTableNames.Notification)
        {
            KeyConditionExpression = $"{DynamoDbNotificationColumnNames.SourceSystemName} = :sourceName",
            ExpressionAttributeValues = new Dictionary<string, AttributeValue>
            {
                {  ":sourceName",  new AttributeValue { S = sourceName.ToLower() } }
            },
        };

        if (linkId != null)
        {
            request.KeyConditionExpression += $" and {DynamoDbNotificationColumnNames.LinkId} = :linkId";
            request.ExpressionAttributeValues.Add(":linkId", new AttributeValue { S = linkId });
        }

        if (sourceId != null)
        {
            request.KeyConditionExpression += $" and {DynamoDbNotificationColumnNames.SourceSystemId} = :sourceId";
            request.ExpressionAttributeValues.Add(":sourceId", new AttributeValue { S = sourceId });
        }

        if (trackingId != null)
        {
            request.KeyConditionExpression += $" and {DynamoDbNotificationColumnNames.TrackingId} = :trackingId";
            request.ExpressionAttributeValues.Add(":trackingId", new AttributeValue { S = trackingId });
        }

        return request;
    }
}

