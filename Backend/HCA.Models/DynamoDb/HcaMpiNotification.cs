namespace HCA.Models.DynamoDb;

/// <summary>
/// MPI Notifications
/// </summary>
//[DynamoDBTable(DynamoDbTableNames.Notification)]
public class HcaMpiNotification
{

    /// <summary>
    /// Source System Name
    /// </summary>
    //[DynamoDBProperty(DynamoDbNotificationColumnNames.SourceSystemName)]
    public string SourceSystemName { get; set; }

    /// <summary>
    /// Link Id
    /// </summary>
    //[DynamoDBHashKey(DynamoDbNotificationColumnNames.LinkId)]
    public string LinkId { get; set; }

    /// <summary>
    /// Tracking Id
    /// </summary>
    //[DynamoDBProperty(DynamoDbNotificationColumnNames.TrackingId)]
    public string TrackingId { get; set; }

    /// <summary>
    /// Source System Name
    /// </summary>
    //[DynamoDBProperty(DynamoDbNotificationColumnNames.TimeStamp)]
    public DateTime TimeStamp { get; set; }

    /// <summary>
    /// Source System Id
    /// </summary>
    //[DynamoDBProperty(DynamoDbNotificationColumnNames.SourceSystemId)]
    public string SourceSystemId { get; set; }

    /// <summary>
    /// Operation
    /// </summary>
    //[DynamoDBProperty(DynamoDbNotificationColumnNames.Operation)]
    public string Operation { get; set; }

    /// <summary>
    /// Request Json Object
    /// </summary>
    //[DynamoDBProperty(DynamoDbNotificationColumnNames.Request)]
    public string Request { get; set; }

    /// <summary>
    /// Response Json Object
    /// </summary>
    //[DynamoDBProperty(DynamoDbNotificationColumnNames.Response)]
    public string Response { get; set; }

    /// <summary>
    /// Message
    /// </summary>
    //[DynamoDBProperty(DynamoDbNotificationColumnNames.PreviousLinkId)]
    public string PreviousLinkId { get; set; }
}
