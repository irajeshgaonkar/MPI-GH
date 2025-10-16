
namespace HCA.Models.Verato.Response;

/// <summary>
/// Un link identities response content
/// </summary>
public class UnLinkIdentitiesResponseContent
{
    /// <summary>
    /// Generated link id for the un linked source
    /// </summary>
    public string UnlinkedId { get; set; }

    /// <summary>
    /// Un linked source
    /// </summary>
    public Source UnlinkedSource { get; set; }

    /// <summary>
    /// link id from which the identity is un linked
    /// </summary>
    public string UnlinkedFromId { get; set; }

    /// <summary>
    /// source details from which the identity is un linked
    /// </summary>
    public Source UnlinkedFromSource { get; set; }
}

