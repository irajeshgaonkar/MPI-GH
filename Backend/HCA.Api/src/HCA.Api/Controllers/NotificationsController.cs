using HCA.Api.Constants;
using HCA.Api.Filters;
using HCA.Core.Services;
using HCA.Infrastructure.Extensions;
using HCA.Infrastructure.Logger;
using HCA.Models.DynamoDb;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace HCA.Api.Controllers;

/// <summary>
/// Notification Controller
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class NotificationsController : ControllerBase
{

    private readonly IAppLogger _appLogger;

    private readonly INotificationService _notificationService;

    /// <summary>
    /// <see cref="NotificationsController"/>
    /// </summary>
    /// <param name="notificationService">Client identity service <see cref="INotificationService"/></param>
    /// <param name="appLogger">Applicaiton logger <see cref="IAppLogger"/></param>
    public NotificationsController(INotificationService notificationService, IAppLogger appLogger)
    {
        _notificationService = notificationService;
        _appLogger = appLogger;
    }

    /// <summary>
    /// Fetches the Dashboard data based on the filter condition
    /// </summary>
    /// <returns>Paginated collection of Notification <see cref="String"/></returns>
    [SwaggerResponse(StatusCodes.Status200OK, "List of notifications", typeof(IEnumerable<NotificationDto>))]
    [SwaggerResponse(StatusCodes.Status401Unauthorized)]
    [SwaggerResponse(StatusCodes.Status403Forbidden)]
    [SwaggerResponse(StatusCodes.Status500InternalServerError)]
    [HcaAuthorize(Roles.ReadOnly, Roles.Admin)]
    [HttpGet()]
    public async Task<IActionResult> GetAllNotifications([FromQuery] string? filter = null, [FromQuery] int? pageSize = null, [FromQuery] string? startKey = null)
    {
        var notificationFilter = new NotificationFilter();

        if(filter != null)
        {
            var filterValues = filter.Split("AND");

            foreach(var filterValue in filterValues)
            {
                var filterValueSplit = filterValue.Split("EQ", StringSplitOptions.RemoveEmptyEntries);
                if (filterValueSplit.Length < 2) continue;
                var key  = filterValueSplit[0];
                var value = filterValueSplit[1];

                if (key.ToLower() == "sourceName".ToLower()) notificationFilter.SourceName = value;
                if (key.ToLower() == "linkId".ToLower()) notificationFilter.LinkId = value;
                if (key.ToLower() == "sourceId".ToLower()) notificationFilter.SourceId = value;
                if (key.ToLower() == "operation".ToLower()) notificationFilter.OperationType = value;
                if (key.ToLower() == "trackingId".ToLower()) notificationFilter.TrackingId = value;
            }
        }

        var result = await _notificationService.GetAllNotifications(notificationFilter, pageSize, startKey);
        var returnValue = GetDtos(result.Item1);
        return Ok(new { items = returnValue, lastKey = result.Item2 });
    }


    private IEnumerable<NotificationDto> GetDtos(IEnumerable<HcaMpiNotification> notifications)
    {
        var result = new List<NotificationDto>();

        foreach(var notification in notifications)
        {
            var notificationDto = new NotificationDto();
            notificationDto.SourceSystemName = notification.SourceSystemName;
            notificationDto.SourceSystemId = notification.SourceSystemId;
            notificationDto.LinkId = notification.LinkId;
            notificationDto.TrackingId = notification.TrackingId;
            notificationDto.TimeStamp = notification.TimeStamp;
            notificationDto.Operation = notification.Operation;
            notificationDto.Request = notification.Request == null ? new { } : SerializationExtensions.DeSerialize<dynamic>(notification.Request)!;
            notificationDto.Response = notification.Response == null ? new { } : SerializationExtensions.DeSerialize<dynamic>(notification.Response)!;
            notificationDto.PreviousLinkId = notification.PreviousLinkId;

            result.Add(notificationDto);
        }

        return result;
    }

    /// <summary>
    /// Notification Dto
    /// </summary>
    public class NotificationDto
    {
        /// <summary>
        /// Source System Name
        /// </summary>
        public string SourceSystemName { get; set; }

        /// <summary>
        /// Link Id
        /// </summary>
        public string LinkId { get; set; }

        /// <summary>
        /// Tracking Id
        /// </summary>
        public string TrackingId { get; set; }

        /// <summary>
        /// Source System Name
        /// </summary>
        public DateTime TimeStamp { get; set; }

        /// <summary>
        /// Source System Id
        /// </summary>
        public string SourceSystemId { get; set; }

        /// <summary>
        /// Operation
        /// </summary>
        public string Operation { get; set; }

        /// <summary>
        /// Request Json Object
        /// </summary>
        public dynamic Request { get; set; }

        /// <summary>
        /// Response Json Object
        /// </summary>
        public dynamic Response { get; set; }

        /// <summary>
        /// Message
        /// </summary>
        public string PreviousLinkId { get; set; }
    }
}
