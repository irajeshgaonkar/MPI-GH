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

    public async Task<IEnumerable<HcaMpiNotification>> GetNotifications(string linkId)
    {
        var returnValue = new List<HcaMpiNotification>();
        //var query = BuildQueryRequest(sourceName, linkId, sourceId, trackingId, pageSize, pageNumber);
        var result = await _hcaDynamoDbClient.Query<HcaMpiNotification>(linkId);

        //if (result.Count < pageSize * pageNumber) return returnValue;
        
        //for(int i = pageSize * pageNumber;i < result.Count && i <  pageSize * (pageNumber + 1); i++)
        //{
        //    var notification = new HcaMpiNotification();
        //    foreach(var attribute in result[i])
        //    {
        //        if (attribute.Key == DynamoDbNotificationColumnNames.LinkId) notification.LinkId = attribute.Value.S;
        //        if (attribute.Key == DynamoDbNotificationColumnNames.TrackingId) notification.TrackingId = attribute.Value.S;
        //        if (attribute.Key == DynamoDbNotificationColumnNames.TimeStamp) notification.TimeStamp = DateTime.Parse(attribute.Value.S);
        //        if (attribute.Key == DynamoDbNotificationColumnNames.SourceSystemName) notification.SourceSystemName = attribute.Value.S;
        //        if (attribute.Key == DynamoDbNotificationColumnNames.SourceSystemId) notification.SourceSystemId = attribute.Value.S;
        //        if (attribute.Key == DynamoDbNotificationColumnNames.Operation) notification.Operation = attribute.Value.S;
        //        if (attribute.Key == DynamoDbNotificationColumnNames.Request) notification.Request = attribute.Value.S;
        //        if (attribute.Key == DynamoDbNotificationColumnNames.Response) notification.Response = attribute.Value.S;
        //        if (attribute.Key == DynamoDbNotificationColumnNames.PreviousLinkId) notification.PreviousLinkId = attribute.Value.S;
        //    }

        //    returnValue.Add(notification);
        //}

        return result;
    }

    public async Task<IEnumerable<HcaMpiNotification>> GetAllNotifications(string? sourceId = null)
    {
        var returnValue = new List<HcaMpiNotification>();
        var query = new ScanRequest(DynamoDbTableNames.Notification);
        
        if (!string.IsNullOrWhiteSpace(sourceId))
        {
            query.FilterExpression += $"{DynamoDbNotificationColumnNames.SourceSystemId} = :sourceId";
            query.ExpressionAttributeValues.Add(":sourceId", new AttributeValue { S = sourceId });
        }

        var result = await _hcaDynamoDbClient.ScanAsync(query);


        for (int i =0; i < result.Count && i < result.Count; i++)
        {
            var notification = new HcaMpiNotification();
            foreach (var attribute in result[i])
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

    private ScanRequest BuildQueryRequest(string? sourceName, string? linkId, string? sourceId, string? trackingId, int pageSize, int pageNumber)
    {
        var request = new ScanRequest(DynamoDbTableNames.Notification);

        if(sourceName != null)
        {
            request.FilterExpression = $"{DynamoDbNotificationColumnNames.SourceSystemName} = :sourceName";
            request.ExpressionAttributeValues = new Dictionary<string, AttributeValue>
            {
                {  ":sourceName",  new AttributeValue { S = sourceName } }
            };
        };

        if (linkId != null)
        {
            if (!string.IsNullOrEmpty(request.FilterExpression)) request.FilterExpression = " and ";
            request.FilterExpression += $"{DynamoDbNotificationColumnNames.LinkId} = :linkId";
            request.ExpressionAttributeValues.Add(":linkId", new AttributeValue { S = linkId });
        }

        if (sourceId != null)
        {
            if (!string.IsNullOrEmpty(request.FilterExpression)) request.FilterExpression = " and ";
            request.FilterExpression += $"{DynamoDbNotificationColumnNames.SourceSystemId} = :sourceId";
            request.ExpressionAttributeValues.Add(":sourceId", new AttributeValue { S = sourceId });
        }

        if (trackingId != null)
        {
            if (!string.IsNullOrEmpty(request.FilterExpression)) request.FilterExpression = " and ";
            request.FilterExpression += $"{DynamoDbNotificationColumnNames.TrackingId} = :trackingId";
            request.ExpressionAttributeValues.Add(":trackingId", new AttributeValue { S = trackingId });
        }

        return request;
    }
}

