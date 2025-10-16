namespace HCA.Models.Verato.Request;

/// <summary>
/// Base Request for all the Verato Calls through Verato
/// </summary>
public class VeratoRequest
{
	public VeratoRequest(string trackingId)
	{
		TrackingId = trackingId;
	}

	/// <summary>
	/// Id for Tracking the request and response
	/// </summary>
	public string TrackingId { get; set; }
}

