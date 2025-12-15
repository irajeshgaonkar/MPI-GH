using System.Text.Json.Serialization;
using HCA.Models.Enums;
using HCA.Models.Verato;
using HCA.Models.Verato.Request;

namespace HCA.Models.Request;

public class PostClientIdentityRequest : BaseRequest
{
    public PostClientIdentityRequest(string trackingId) : base(ApiCallType.VEPost, trackingId)
    {
    }

    public IList<ClientIdentityRequest> Content { get; set; }
}

public class DeleteClientIdentityRequest : BaseRequest
{
    public DeleteClientIdentityRequest(string trackingId) : base(ApiCallType.VEDelete, trackingId)
    {
    }

    public Source Content { get; set; }
}

public class DOH_DeleteClientIdentityRequest : BaseRequest
{
    public DOH_DeleteClientIdentityRequest(string? trackingId = "") : base(ApiCallType.DOH_VEDelete, trackingId)
    {
    }
    /// <summary>
    /// 
    /// </summary>
    public string? SourceSystem { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public string? Agency { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public string? IpAddress { get; set; }
    public DeleteIdentyRequestContent Content { get; set; }
}

/// <summary>
/// 
/// </summary>
public class DOH_PostClientIdentityRequest : BaseRequest
{
    /// <summary>
    /// 
    /// </summary>
    public DOH_PostClientIdentityRequest(string? trackingId = "") : base(ApiCallType.DOH_VEPost, trackingId)
    {
    }

    /// <summary>
    /// 
    /// </summary>
    public ProtectedPopulation[]? protectedPopulation { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public string? SourceSystem { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public string? Agency { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public string? IpAddress { get; set; }

    [JsonPropertyName("content")]
    public DOH_PostIdentityRequestContent Content { get; set; }
}

/// <summary>
/// 
/// </summary>
public class ProtectedPopulation
{
    /// <summary>
    /// 
    /// </summary>
    public string? ProtectedPopulationFlag { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public string[]? ProtectedPopulationTypes { get; set; }
}