namespace HCA.MuleSoft.Models.Request;

/// <summary>
/// Base Request for all the Verato Calls through MuleSoft
/// </summary>
/// <typeparam name="T">Request Content</typeparam>
public class BaseRequest<T>
{
	public BaseRequest(string trackingId, T content)
	{
		TrackingId = trackingId;
		Content = content;
	}

	/// <summary>
	/// Id for Tracking the request and response
	/// </summary>
	public string TrackingId { get; set; }

	/// <summary>
	/// Content of the request
	/// </summary>
	public T Content { get; set; }
}

