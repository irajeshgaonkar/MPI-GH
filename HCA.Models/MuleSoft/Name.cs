namespace HCA.Models.MuleSoft;

public class Name
{
    public Name(string first, string middle, string last, string suffix)
    {
        First = first;
        Middle = middle;
        Last = last;
        Suffix = suffix;
    }

    public string First { get; set; }

    public string Middle { get; set; }

    public string Last { get; set; }

    public string Suffix { get; set; }
}

