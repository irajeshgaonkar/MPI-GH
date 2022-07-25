using System.ComponentModel.DataAnnotations;

namespace HCA.Models.MuleSoft;

/// <summary>
/// Linking Sources
/// </summary>
public class LinkingSources
{
    /// <summary>
    /// <see cref="LinkingSources"/>
    /// </summary>
    /// <param name="linkToSource">Link to Source</param>
    /// <param name="source">Source to be linked</param>
    public LinkingSources(Source linkToSource, Source source)
    {
        LinkToSource = linkToSource;
        Source = source;
    }

    /// <summary>
    /// Link to Source
    /// </summary>
    [Required(ErrorMessage = "Link to source is required")]
    public Source LinkToSource { get; set; }

    /// <summary>
    /// Source to be linke
    /// </summary>
    [Required(ErrorMessage = "Source is required")]
    public Source Source { get; set; }
}

