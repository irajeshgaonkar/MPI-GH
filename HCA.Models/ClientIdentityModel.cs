using System;
namespace HCA.Models;

public class ClientIdentityModel
{
    public string? MpiLinkId { get; set; }

    public int Id { get; set; }

    public string SourceSystemId { get; set; }

    public string SourceSystemName { get; set; }

    public string SourceSystemAgency { get; set; }

    public string FirstName { get; set; }

    public string? MiddleName { get; set; }

    public string LastName { get; set; }

    public string? NameSuffix { get; set; }

    public string? SSN { get; set; }

    public DateOnly? DOB { get; set; }

    public string Gender { get; set; }

    public bool ProtectedPopulationFlag { get; set; }

    public string? ProtectedPopulationType { get; set; }

    public DateTime SourceSystemUpdated { get; set; }

    public List<ClientIdentityAddress> Addresses { get; set; }
}

public class ClientIdentityAddress
{
    public string? AddressType { get; set; }

    public string AddressLine1 { get; set; }

    public string? AddressLine2 { get; set; }

    public string? AddressLine3 { get; set; }

    public string City { get; set; }

    public string State { get; set; }

    public string ZipCode { get; set; }

    public string ZipFour { get; set; }

    public List<ClientIdentityCommunication> Communications { get; set; }
}

public class ClientIdentityCommunication
{
    public string? PhoneType { get; set; }

    public string? EmailType { get; set; }

    public string? PhoneNumber { get; set; }

    public string? EmailAddress { get; set; }
}
