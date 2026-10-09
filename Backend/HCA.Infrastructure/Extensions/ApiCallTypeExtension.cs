using HCA.Models.Enums;

namespace HCA.Infrastructure.Extensions;

public static class ApiCallTypeExtension
{
    public static ApiCallType ParseToApiCallType(this string operationType)
    {
        if (operationType.StringEquals("VE Post")) return ApiCallType.VEPost;
        if (operationType.StringEquals("VE Link")) return ApiCallType.VELink;
        if (operationType.StringEquals("VE Un Link")) return ApiCallType.VEUnLink;
        if (operationType.StringEquals("VE Merge")) return ApiCallType.VEMerge;
        if (operationType.StringEquals("VE Un Merge")) return ApiCallType.VEUnMerge;
        if (operationType.StringEquals("VE Delete")) return ApiCallType.VEDelete;
        return ApiCallType.VEPost;
    }
}
