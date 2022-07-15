namespace HCA.Models.MuleSoft;

public class Source
{
    public Source(string name, string id)
    {
        Name = name;
        Id = id;
    }

    public string Name { get; set; }

    public string Id { get; set; }
}

