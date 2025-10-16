
namespace HCA.Models.Verato.Response;

/// <summary>
/// Client Identity
/// </summary>
public class IdentityResponse
{
    public IdentityResponse()
    {
        Sources = new List<Source>();
        Emails = new List<EmailResponse>();
        Addresses = new List<AddressResponse>();
        Names = new List<NameResponse>();
        Ssns = new List<SsnsResponse>();
        Genders = new List<GendersResponse>();
        DatesOfBirth = new List<DatesOfBirthResponse>();
        PhoneNumbers = new List<PhoneNumberResponse>();
    }

    /// <summary>
    /// Collection of source details <see cref="Source"/>
    /// </summary>
    public List<Source> Sources { get; set; }

    /// <summary>
    /// Collection of email details <see cref="EmailResponse"/>
    /// </summary>
    public List<EmailResponse> Emails { get; set; }

    /// <summary>
    /// Collection of address details <see cref="AddressResponse"/>
    /// </summary>
    public List<AddressResponse> Addresses { get; set; }

    /// <summary>
    /// Collection of name details <see cref="NameResponse"/>
    /// </summary>
    public List<NameResponse> Names { get; set; }

    /// <summary>
    /// Collection of social Security Number details <see cref="SsnsResponse"/>
    /// </summary>
    public List<SsnsResponse> Ssns { get; set; }

    /// <summary>
    /// Collection of gender details <see cref="SsnsResponse"/>
    /// </summary>
    public List<GendersResponse> Genders { get; set; }

    /// <summary>
    /// Collection of date of Birth details <see cref="DatesOfBirthResponse"/>
    /// </summary>
    public List<DatesOfBirthResponse> DatesOfBirth { get; set; }

    /// <summary>
    /// Collection of phone Number details <see cref="PhoneNumberResponse"/>
    /// </summary>
    public List<PhoneNumberResponse> PhoneNumbers { get; set; }
}

/// <summary>
/// Base response entity
/// </summary>
public class BaseResponseEntity
{
    /// <summary>
    /// First asserted date of the response
    /// </summary>
    public string FirstAsserted { get; set; }

    /// <summary>
    /// Last asserted date of the response
    /// </summary>
    public string LastAsserted { get; set; }
}

/// <summary>
/// Name response
/// </summary>
public class NameResponse : BaseResponseEntity
{
    /// <summary>
    /// Name <see cref="Name"/>
    /// </summary>
    public Name Name { get; set; }
}

/// <summary>
/// Dates of Births response
/// </summary>
public class DatesOfBirthResponse : BaseResponseEntity
{
    /// <summary>
    /// Date of the birth (date only)
    /// </summary>
    public string DateOfBirth { get; set; }
}

/// <summary>
/// Gender response
/// </summary>
public class GendersResponse : BaseResponseEntity
{
    /// <summary>
    /// Gender
    /// </summary>
    public string Gender { get; set; }
}

/// <summary>
/// Social security number response
/// </summary>
public class SsnsResponse : BaseResponseEntity
{
    /// <summary>
    /// Social Security Number
    /// </summary>
    public string Ssn { get; set; }
}

/// <summary>
/// Email response
/// </summary>
public class EmailResponse : BaseResponseEntity
{
    /// <summary>
    /// Email
    /// </summary>
    public string Email { get; set; }
}

/// <summary>
/// Address response
/// </summary>
public class AddressResponse : BaseResponseEntity
{
    /// <summary>
    /// Address <see cref="Address"/>
    /// </summary>
    public Address Address { get; set; }
}

/// <summary>
/// Phone number response
/// </summary>
public class PhoneNumberResponse : BaseResponseEntity
{
    /// <summary>
    /// Phone Number <see cref="PhoneNumber"/>
    /// </summary>
    public PhoneNumber PhoneNumber { get; set; }
}


