
namespace HCA.Models.Verato.Response;

/// <summary>
/// Merge identities response content
/// </summary>
public class MergeIdentitiesResponseContent
{
	/// <summary>
    /// Link id of the merged requests
    /// </summary>
	public string LinkId { get; set; }

	/// <summary>
    /// Source details of the merged requests
    /// </summary>
	public Source Source { get; set; }
}



