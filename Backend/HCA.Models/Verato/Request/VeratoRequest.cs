namespace HCA.Models.Verato.Request;

/// <summary>
/// Base Request for all the Verato Calls through Verato
/// </summary>
public class VeratoRequest( string trackingId )
{
    /// <summary>
    /// Id for Tracking the request and response
    /// </summary>
    public string TrackingId { get; set; } = trackingId;
}

