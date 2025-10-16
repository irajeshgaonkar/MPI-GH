using System.ComponentModel.DataAnnotations;

namespace HCA.Models.Verato;

/// <summary>
/// Merging Sources
/// </summary>
public class MergingSources
{
    /// <summary>
    /// <see cref="MergingSources"/>
    /// </summary>
    /// <param name="toSurviveSource">To survive source</param>
    /// <param name="toRetireSource">To retire source</param>
    public MergingSources(Source toSurviveSource, Source toRetireSource)
    {
        ToSurviveSource = toSurviveSource;
        ToRetireSource = toRetireSource;
    }

    /// <summary>
    /// To survive source
    /// </summary>
    [Required(ErrorMessage = "To survive source is required")]
    public Source ToSurviveSource { get; set; }

    /// <summary>
    /// To retire source
    /// </summary>
    [Required(ErrorMessage = "To retire source is required")]
    public Source ToRetireSource { get; set; }
}

