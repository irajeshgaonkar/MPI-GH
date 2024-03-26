namespace HCA.Models.MuleSoft;

/// <summary>
/// Address Model
/// </summary>
public class Address
{
    /// <summary>
    /// <see cref="Address"/>
    /// </summary>
    /// <param name="line1">Address line 1</param>
    /// <param name="line2">Address line 2</param>
    /// <param name="city">City name</param>
    /// <param name="state">State name / code</param>
    /// <param name="postalCode">Postal code</param>
    /// <param name="zipFour"></param>
    public Address(string line1, string line2, string city, string state, string postalCode, string zipFour)
    {
        Line1 = line1;
        Line2 = line2;
        City = city;
        State = state;
        PostalCode = postalCode;
        ZipFour = zipFour;
    }

    /// <summary>
    /// Address line 1
    /// </summary>
    public string Line1 { get; set; }

    /// <summary>
    /// Address line 2
    /// </summary>
    public string Line2 { get; set; }

    /// <summary>
    /// City name
    /// </summary>
    public string City { get; set; }

    /// <summary>
    /// State name / code
    /// </summary>
    public string State { get; set; }

    /// <summary>
    /// Postal code
    /// </summary>
    public string PostalCode { get; set; }

    /// <summary>
    /// Zip Four
    /// </summary>
    public string ZipFour { get; set; }
}

