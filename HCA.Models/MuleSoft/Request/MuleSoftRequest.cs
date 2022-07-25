namespace HCA.Models.MuleSoft.Request;

/// <summary>
/// Base Request for all the Verato Calls through MuleSoft
/// </summary>
public class MuleSoftRequest
{
	public MuleSoftRequest(string trackingId)
	{
		TrackingId = trackingId;
	}

	/// <summary>
	/// Id for Tracking the request and response
	/// </summary>
	public string TrackingId { get; set; }
}

