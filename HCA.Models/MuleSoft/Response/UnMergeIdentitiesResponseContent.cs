
namespace HCA.Models.MuleSoft.Response;

/// <summary>
/// Un merge identities response content
/// </summary>
public class UnMergeIdentitiesResponseContent
{
    /// <summary>
    /// un merge link id
    /// </summary>
	public string UnmergedId { get; set; }

    /// <summary>
    /// Un merged from link id
    /// </summary>
	public string UnmergedFromId { get; set; }

    /// <summary>
    /// Un merged from source details <see cref="Source"/>
    /// </summary>
    public Source UnmergedFromSource { get; set; }

    /// <summary>
    /// Un merge source details <see cref="Source"/>
    /// </summary>
    public Source UnmergedSource { get; set; }
}



