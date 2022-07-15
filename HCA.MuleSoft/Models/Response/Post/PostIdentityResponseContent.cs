using HCA.Models.MuleSoft;

namespace HCA.MuleSoft.Models.Response.Post;

public class PostIdentityResponseContent
{
    public PostIdentityResponseContent(string linkId, List<IdentityResponse> identityGroupedBySource, List<PostIdentityResponseEvent> events)
    {
        LinkId = linkId;
        IdentityGroupedBySource = identityGroupedBySource;
        Events = events;
    }

    public string LinkId { get; set; }

    public List<IdentityResponse> IdentityGroupedBySource { get; set; }

    public List<PostIdentityResponseEvent> Events { get; set; }
}

public class BaseResponseEntity
{
    public string FirstAsserted { get; set; }

    public string LastAsserted { get; set; }
}

public class DatesOfBirthResponse : BaseResponseEntity
{
    public Name Name { get; set; }
}

public class NameResponse : BaseResponseEntity
{
    public string DateOfBirth { get; set; }
}

public class GendersResponse : BaseResponseEntity
{
    public string Gender { get; set; }
}

public class SsnsResponse : BaseResponseEntity
{
    public string Ssn { get; set; }
}

public class EmailResponse : BaseResponseEntity
{
    public string Email { get; set; }
}

public class AddressResponse : BaseResponseEntity
{
    public Address Address { get; set; }
}

public class PhoneNumberResponse : BaseResponseEntity
{
    public PhoneNumber PhoneNumber { get; set; }
}

public class PhoneNumber
{
    public string CountryCode { get; set; }

    public string AreaCode { get; set; }

    public string Number { get; set; }
}

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
    /// Source details
    /// </summary>
    public List<Source> Sources { get; set; }

    /// <summary>
    /// Email details
    /// </summary>
    public List<EmailResponse> Emails { get; set; }

    /// <summary>
    /// Address details
    /// </summary>
    public List<AddressResponse> Addresses { get; set; }

    /// <summary>
    /// Name details
    /// </summary>
    public List<NameResponse> Names { get; set; }

    /// <summary>
    /// Social Security Number details
    /// </summary>
    public List<SsnsResponse> Ssns { get; set; }

    /// <summary>
    /// Gender details
    /// </summary>
    public List<GendersResponse> Genders { get; set; }

    /// <summary>
    /// Date of Birth details
    /// </summary>
    public List<DatesOfBirthResponse> DatesOfBirth { get; set; }

    /// <summary>
    /// Phone Number details
    /// </summary>
    public List<PhoneNumberResponse> PhoneNumbers { get; set; }
}

