using System.ComponentModel.DataAnnotations;

namespace HCA.Models.MuleSoft;

/// <summary>
/// Un merging sources
/// </summary>
public class UnMergingSources
{
    /// <summary>
    /// <see cref="UnMergingSources"/>
    /// </summary>
    /// <param name="unmergeFromSource">Un merge from source</param>
    /// <param name="unmergeSource">un merge source</param>
    public UnMergingSources(Source unmergeFromSource, Source unmergeSource)
    {
        UnmergeFromSource = unmergeFromSource;
        UnmergeSource = unmergeSource;
    }

    /// <summary>
    /// Un merge from source
    /// </summary>
    [Required(ErrorMessage = "Un merge from source is required")]
    public Source UnmergeFromSource { get; set; }

    /// <summary>
    /// un merge source
    /// </summary>
    [Required(ErrorMessage = "Un merge source is required")]
    public Source UnmergeSource { get; set; }
}

