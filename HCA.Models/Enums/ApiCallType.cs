namespace HCA.Models.Enums;

/// <summary>
/// Api call types
/// </summary>
public enum ApiCallType
{
    [StringValue("VE VEPost")]
    VEPost,

    [StringValue("VE Link")]
    VELink,

    [StringValue("VE Un Link")]
    VEUnLink,

    [StringValue("VE Merge")]
    VEMerge,

    [StringValue("VE Un Merge")]
    VEUnMerge,

    [StringValue("VE Delete")]
    VEDelete,

    [StringValue("VE Demographic Search")]
    VEDemographicSearch,

    [StringValue("VE Demographic Query")]
    VEDemographicQuery
}
