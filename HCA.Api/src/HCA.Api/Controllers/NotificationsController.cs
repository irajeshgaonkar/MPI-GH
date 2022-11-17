using HCA.Api.Constants;
using HCA.Api.Dto;
using HCA.Api.Filters;
using HCA.Core.Services;
using HCA.Infrastructure.Extensions;
using HCA.Infrastructure.Logger;
using HCA.Models.DynamoDb;
using Microsoft.AspNetCore.Http;
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
    /// <see cref="NotificationController"/>
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
    public async Task<IActionResult> GetAllNotifications()
    {
        var result = await _notificationService.GetNotifications();
        var returnValue = GetDtos(result);
        return Ok(returnValue);
    }

    /// <summary>
    /// Fetches the Dashboard data based on the filter condition
    /// </summary>
    /// <param name="linkId">Filter Condition</param>
    /// <returns>Paginated collection of Notification <see cref="String"/></returns>
    [SwaggerResponse(StatusCodes.Status200OK, "List of notifications", typeof(IEnumerable<NotificationDto>))]
    [SwaggerResponse(StatusCodes.Status401Unauthorized)]
    [SwaggerResponse(StatusCodes.Status403Forbidden)]
    [SwaggerResponse(StatusCodes.Status500InternalServerError)]
    [HcaAuthorize(Roles.ReadOnly, Roles.Admin)]
    [HttpGet("{linkId}")]
    public async Task<IActionResult> GetNotifications([FromRoute] string linkId)
    {
        var result = await _notificationService.GetNotifications(linkId);
        var returnValue = GetDtos(result);
        return Ok(returnValue);
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
            notificationDto.Request = SerializationExtensions.DeSerialize<dynamic>(notification.Request) ?? new { };
            notificationDto.Response = SerializationExtensions.DeSerialize<dynamic>(notification.Response) ?? new { };
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
