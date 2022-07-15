namespace HCA.Models.MuleSoft;

public class Identity
{
    public Identity()
    {
        Sources = new List<Source>();
        Emails = new List<string>();
        Addresses = new List<Address>();
        Names = new List<Name>();
        Ssns = new List<string>();
        Genders = new List<string>();
        DatesOfBirth = new List<string>();
        PhoneNumbers = new List<PhoneNumber>();
    }

    public List<Source> Sources { get; set; }

    public List<string> Emails { get; set; }

    public List<Address> Addresses { get; set; }

    public List<Name> Names { get; set; }

    public List<string> Ssns { get; set; }

    public List<string> Genders { get; set; }

    public List<string> DatesOfBirth { get; set; }

    public List<PhoneNumber> PhoneNumbers { get; set; }
}

