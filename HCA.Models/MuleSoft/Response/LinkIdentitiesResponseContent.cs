
namespace HCA.Models.MuleSoft.Response;

/// <summary>
/// Link identities response content
/// </summary>
public class LinkIdentitiesResponseContent
{
    /// <summary>
    /// link id of the linked identities
    /// </summary>
    public string LinkId { get; set; }

    /// <summary>
    /// source of the linked sources
    /// </summary>
    public Source LinkToSource { get; set; }
}

