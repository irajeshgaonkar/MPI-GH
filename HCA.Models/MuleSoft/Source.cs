using System.ComponentModel.DataAnnotations;

namespace HCA.Models.MuleSoft;

/// <summary>
/// Source details
/// </summary>
public class Source
{
    /// <summary>
    /// <see cref="Source"/>
    /// </summary>
    /// <param name="name">Name of the source</param>
    /// <param name="id">source system id</param>
    public Source(string name, string id)
    {
        Name = name;
        Id = id;
    }

    /// <summary>
    /// Source name
    /// </summary>
    /// <example>wadoh.providerone</example>
    [Required(ErrorMessage = "Source name is required")]
    public string Name { get; set; }

    /// <summary>
    /// Source system id
    /// </summary>
    /// <example>2133169982WA</example>
    [Required(ErrorMessage = "Source system name is required")]
    public string Id { get; set; }
}

