namespace HCA.Models.Enums;

/// <summary>
/// Api call types
/// </summary>
public enum ApiCallType
{
    [StringValue("VE VEPost")]
    VEPost,

    [StringValue("VE VEPost")]
    DOH_VEPost,

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

    [StringValue("VE Demographic Search")]
    DOH_VEDemographicSearch,
    
    [StringValue("VE Demographic Query")]
    VEDemographicQuery,

    [StringValue("VE Demographic Query")]
    DOH_VEDemographicQuery,

    [StringValue("VE Enrich Demographic Query")]
    DOH_VEEnrichDemographicQuery,

    [StringValue("VE Link")]
    DOH_VELink,

    [StringValue("VE Un Link")]
    DOH_VEUnLink,

    [StringValue("VE Merge")]
    DOH_VEMerge,

    [StringValue("VE Un Merge")]
    DOH_VEUnMerge,
    
    [StringValue("VE Delete")]
    DOH_VEDelete,

    [StringValue("VE Identity Exists")]
    VEIdentityExists,

    [StringValue("VE Native Id Query")]
    VENativeIdQuery,

    [StringValue("VE Search Notifications")]
    VESearchNotifications,
}
