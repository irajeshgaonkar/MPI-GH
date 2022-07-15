namespace HCA.Models.MuleSoft;

public class PhoneNumber
{
    public PhoneNumber(string number, string areaCode, string extension, string countryCode)
    {
        Number = number;
        AreaCode = areaCode;
        Extension = extension;
        CountryCode = countryCode;
    }

    public string Number { get; set; }

    public string AreaCode { get; set; }

    public string Extension { get; set; }

    public string CountryCode { get; set; }
}

