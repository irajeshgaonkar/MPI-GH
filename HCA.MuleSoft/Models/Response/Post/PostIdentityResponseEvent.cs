using HCA.Models.MuleSoft;

namespace HCA.MuleSoft.Models.Response.Post;

/// <summary>
/// Post Identity Response Event
/// </summary>
public class PostIdentityResponseEvent
{
	/// <summary>
    /// Type of the Event
    /// </summary>
	public string Type { get; set; }

	/// <summary>
    /// Previous Link Id if any
    /// </summary>
	public string? PreviousLinkId { get; set; }

	/// <summary>
    /// Sources affected because of this post
    /// </summary>
	public List<Source>? Sources { get; set; }
}

