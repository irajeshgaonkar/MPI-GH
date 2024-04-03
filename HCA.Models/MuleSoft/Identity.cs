namespace HCA.Models.MuleSoft;

/// <summary>
/// Identity details
/// </summary>
public class Identity
{

   
    /// <summary>
    /// <see cref="Identity"/>
    /// </summary>
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
        //CreateDates = new List<CreateDate>();
    }

    /// <summary>
    /// Collection of sources <see cref="Source"/>
    /// </summary>
    /// <example></example>
    public List<Source> Sources { get; set; }

    /// <summary>
    /// Collection of emails
    /// </summary>
    /// <example></example>
    public List<string> Emails { get; set; }

    /// <summary>
    /// Collection of addresses <see cref="Address"/>
    /// </summary>
    /// <example></example>
    public List<Address> Addresses { get; set; }

    /// <summary>
    /// Collection of names <see cref="Name" />
    /// </summary>
    /// <example></example>
    public List<Name> Names { get; set; }

    /// <summary>
    /// Collection of social security numbers
    /// </summary>
    /// <example></example>
    public List<string> Ssns { get; set; }

    /// <summary>
    /// Collection of genders
    /// </summary>
    /// <example></example>
    public List<string> Genders { get; set; }

    /// <summary>
    /// Collection of dates of birth
    /// </summary>
    /// <example></example>
    public List<string> DatesOfBirth { get; set; }

    /// <summary>
    /// Collection of phone number <see cref="PhoneNumber"/>
    /// </summary>
    /// <example></example>
    public List<PhoneNumber> PhoneNumbers { get; set; }

    /// <summary>
    /// Collection of phone number <see cref="CreateDate"/>
    /// </summary>
    /// <example></example>
  //  [JsonPropertyName("CreateDates")]
    //public List<CreateDate> CreateDates { get; set; } = null;



}

