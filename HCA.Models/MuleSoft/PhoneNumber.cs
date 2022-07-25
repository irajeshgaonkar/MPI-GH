namespace HCA.Models.MuleSoft;

/// <summary>
/// Phone Number details
/// </summary>
public class PhoneNumber
{
    /// <summary>
    /// <see cref="PhoneNumber"/>
    /// </summary>
    /// <param name="number">Phone number</param>
    /// <param name="areaCode">Area code</param>
    /// <param name="extension">Extension</param>
    /// <param name="countryCode">Country code</param>
    public PhoneNumber(string number, string areaCode, string extension, string countryCode)
    {
        Number = number;
        AreaCode = areaCode;
        Extension = extension;
        CountryCode = countryCode;
    }

    /// <summary>
    /// Phone number
    /// </summary>
    public string Number { get; set; }

    /// <summary>
    /// Area code
    /// </summary>
    public string AreaCode { get; set; }

    /// <summary>
    /// Extension
    /// </summary>
    public string Extension { get; set; }

    /// <summary>
    /// Country code
    /// </summary>
    public string CountryCode { get; set; }
}

