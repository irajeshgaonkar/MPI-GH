using System.ComponentModel.DataAnnotations;

namespace HCA.Models.Verato;

/// <summary>
/// Un linking sources
/// </summary>
public class UnLinkingSources
{
    /// <summary>
    /// <see cref="UnLinkingSources"/>
    /// </summary>
    /// <param name="unlInkFromSource">Un link from source</param>
    /// <param name="source">Source to be un linked</param>
    public UnLinkingSources(Source unlInkFromSource, Source source)
    {
        UnlinkFromSource = unlInkFromSource;
        Source = source;
    }

    /// <summary>
    /// Un link from source
    /// </summary>
    [Required(ErrorMessage = "Un link from source is required")]
    public Source UnlinkFromSource { get; set; }

    /// <summary>
    /// Source to be un linked
    /// </summary>
    [Required(ErrorMessage = "source to be un linked is required")]
    public Source Source { get; set; }
}

