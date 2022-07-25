namespace HCA.Models.Request;

public class ClientIdentityRequest
{
    /// <summary>
    /// Unique id for the request
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Unique Id for the file - mapping to file request
    /// </summary>
    public string RequestTrackingId { get; set; }

    /// <summary>
    /// Unique Id for the request
    /// </summary>
    public string? TrackingId { get; set; }

    /// <summary>
    /// Universal Link Id
    /// </summary>
    public string? MpiLinkId { get; set; }

    /// <summary>
    /// Souce System Agency
    /// </summary>
    public string SourceSystemAgency { get; set; }

    /// <summary>
    /// Source System Name
    /// </summary>
    public string SourceSystemName { get; set; }

    /// <summary>
    /// Source System Id
    /// </summary>
    public string SourceSystemId { get; set; }

    /// <summary>
    /// Source System Updated Date time
    /// </summary>
    public DateTime SourceSystemUpdated { get; set; }

    /// <summary>
    /// First Name
    /// </summary>
    public string FirstName { get; set; }

    /// <summary>
    /// Middel Name
    /// </summary>
    public string? MiddleName { get; set; }

    /// <summary>
    /// Last Name
    /// </summary>
    public string LastName { get; set; }

    /// <summary>
    /// Name Suffix
    /// </summary>
    public string? NameSuffix { get; set; }

    /// <summary>
    /// Social Security Number
    /// </summary>
    public string? Ssn { get; set; }

    /// <summary>
    /// Date of Birth
    /// </summary>
    public DateOnly? Dob { get; set; }

    /// <summary>
    /// Gender
    /// </summary>
    public string Gender { get; set; }

    /// <summary>
    /// Protected Population Flag
    /// </summary>
    public bool ProtectedPopulationFlag { get; set; }

    /// <summary>
    /// Protected Population Flag Type
    /// </summary>
    public string? ProtectedPopulationType { get; set; }

    /// <summary>
    /// Address Type
    /// </summary>
    public string? AddressType { get; set; }

    /// <summary>
    /// Address Line 1
    /// </summary>
    public string AddressLine1 { get; set; }

    /// <summary>
    /// Address Line 2
    /// </summary>
    public string? AddressLine2 { get; set; }

    /// <summary>
    /// Address Line 3
    /// </summary>
    public string? AddressLine3 { get; set; }

    /// <summary>
    /// City
    /// </summary>
    public string City { get; set; }

    /// <summary>
    /// State
    /// </summary>
    public string State { get; set; }

    /// <summary>
    /// Zip Code
    /// </summary>
    public string ZipCode { get; set; }

    /// <summary>
    /// Zip plus Four
    /// </summary>
    public string ZipFour { get; set; }

    /// <summary>
    /// Phone Type
    /// </summary>
    public string? PhoneType { get; set; }

    /// <summary>
    /// Email Type
    /// </summary>
    public string? EmailType { get; set; }

    /// <summary>
    /// Phone Number
    /// </summary>
    public string? PhoneNumber { get; set; }

    /// <summary>
    /// Email Address
    /// </summary>
    public string? EmailAddress { get; set; }

    /// <summary>
    /// Status
    /// </summary>
    public string? Status { get; set; }

    /// <summary>
    /// Status
    /// </summary>
    public string? Message { get; set; }
}

