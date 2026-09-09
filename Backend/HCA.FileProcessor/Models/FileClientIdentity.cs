using HCA.FileProcessor.Attributes;
using HCA.FileProcessor.Enums;

namespace HCA.FileProcessor.Models;

/// <summary>
/// Data row in the input file
/// </summary>
public class FileClientIdentity
{
    /// <summary>
    /// Universal Link Id
    /// </summary>
    [FieldPosition(0)]
    public string? MpiLinkId { get; set; }

    /// <summary>
    /// Source System Id
    /// </summary>
    [FieldPosition(1)]
    [FieldValidator(ValidationType.Required, ErrorMessage = "Source system id is required")]
    public string SourceSystemId { get; set; }

    /// <summary>
    /// Source System Updated Date time
    /// </summary>
    [FieldPosition(2)]
    [FieldValidator(ValidationType.Required, ErrorMessage = "Source system updated is required")]
    [FieldValidator(ValidationType.DateTime, ErrorMessage = "Source system updated should be a valid date time")]
    public DateTime SourceSystemUpdated { get; set; }

    /// <summary>
    /// First Name
    /// </summary>
    [FieldPosition(3)]
    [FieldValidator(ValidationType.Required, ErrorMessage = "First name is required")]
    public string FirstName { get; set; }

    /// <summary>
    /// Middel Name
    /// </summary>
    [FieldPosition(4)]
    public string? MiddleName { get; set; }

    /// <summary>
    /// Last Name
    /// </summary>
    [FieldPosition(5)]
    [FieldValidator(ValidationType.Required, ErrorMessage = "Last name is required")]
    public string LastName { get; set; }

    /// <summary>
    /// Name Suffix
    /// </summary>
    [FieldPosition(6)]
    public string? NameSuffix { get; set; }

    /// <summary>
    /// Date of Birth
    /// </summary>
    [FieldPosition(7)]
    [FieldValidator(ValidationType.DateTime, ErrorMessage = "Date of birth should be a valid date")]
    public DateOnly? Dob { get; set; }

    /// <summary>
    /// Gender
    /// </summary>
    [FieldPosition(8)]
    [FieldValidator(ValidationType.Required, ErrorMessage = "Gender is required")]
    public string Gender { get; set; }

    /// <summary>
    /// Social Security Number
    /// </summary>
    [FieldPosition(9)]
    public string? Ssn { get; set; }

    /// <summary>
    /// Address Type
    /// </summary>
    [FieldPosition(10)]
    public string? AddressType { get; set; }

    /// <summary>
    /// Address Line 1
    /// </summary>
    [FieldPosition(11)]
    [FieldValidator(ValidationType.Required, ErrorMessage = "Address line 1 is required")]
    public string AddressLine1 { get; set; }

    /// <summary>
    /// Address Line 2
    /// </summary>
    [FieldPosition(12)]
    public string? AddressLine2 { get; set; }

    /// <summary>
    /// Address Line 3
    /// </summary>
    [FieldPosition(13)]
    public string? AddressLine3 { get; set; }

    /// <summary>
    /// City
    /// </summary>
    [FieldPosition(14)]
    [FieldValidator(ValidationType.Required, ErrorMessage = "City is required")]
    public string City { get; set; }

    /// <summary>
    /// State
    /// </summary>
    [FieldPosition(15)]
    [FieldValidator(ValidationType.Required, ErrorMessage = "State is required")]
    public string State { get; set; }

    /// <summary>
    /// Zip Code
    /// </summary>
    [FieldPosition(16)]
    [FieldValidator(ValidationType.Required, ErrorMessage = "Zip Code is required")]
    public string ZipCode { get; set; }

    /// <summary>
    /// Zip plus Four
    /// </summary>
    [FieldPosition(17)]
    public string ZipFour { get; set; }

    /// <summary>
    /// Phone Type
    /// </summary>
    [FieldPosition(18)]
    public string? PhoneType { get; set; }

    /// <summary>
    /// Phone Number
    /// </summary>
    [FieldPosition(19)]
    [FieldValidator(ValidationType.Phone, ErrorMessage = "Phone number should be valid")]
    public string? PhoneNumber { get; set; }

    /// <summary>
    /// Email Type
    /// </summary>
    [FieldPosition(20)]
    public string? EmailType { get; set; }

    /// <summary>
    /// Email Address
    /// </summary>
    [FieldPosition(21)]
    [FieldValidator(ValidationType.Email, ErrorMessage = "Email should be valid")]
    public string? EmailAddress { get; set; }

    /// <summary>
    /// Protected Population Flag
    /// </summary>
    [FieldPosition(22)]
    public bool ProtectedPopulationFlag { get; set; }

    /// <summary>
    /// Protected Population Flag Type
    /// </summary>
    [FieldPosition(23)]
    public string? ProtectedPopulationType { get; set; }

    public string Status { get; set; }

    public string Message { get; set; }
}

