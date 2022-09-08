using System;
using HCA.Models.Enums;
using HCA.Models.MuleSoft.Response;

namespace HCA.Models.Request;

public abstract class BaseRequest
{
    public BaseRequest(ApiCallType apiCallType, string trackingId)
    {
        ApiCallType = apiCallType;
        TrackingId = trackingId;
    }

    public string TrackingId { get; set; }

    public ApiCallType ApiCallType { get; set; }
}
