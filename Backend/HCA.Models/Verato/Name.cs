namespace HCA.Models.Verato;

/// <summary>
/// Name details
/// </summary>
public class Name
{
    /// <summary>
    /// <see cref="Name"/>
    /// </summary>
    /// <param name="first">First name</param>
    /// <param name="middle">Middle name</param>
    /// <param name="last">Last name</param>
    /// <param name="suffix">Name suffix</param>
    public Name(string first, string middle, string last, string suffix)
    {
        First = first;
        Middle = middle;
        Last = last;
        Suffix = suffix;
    }

    /// <summary>
    /// First name
    /// </summary>
    public string First { get; set; }

    /// <summary>
    /// Middle name
    /// </summary>
    public string Middle { get; set; }

    /// <summary>
    /// Last name
    /// </summary>
    public string Last { get; set; }

    /// <summary>
    /// Name suffix
    /// </summary>
    public string Suffix { get; set; }
}

