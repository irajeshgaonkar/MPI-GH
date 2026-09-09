namespace HCA.Models.Verato;

public class ClientIdentity
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

    public int Id { get; set; }

    public string MPILinkId { get; set; }

    public string SourceName { get; set; }

    public string SourceSystemId { get; set; }

    public DateTime SourceSystemLastUpdate { get; set; }

    public string FirstName { get; set; }

    public string MiddleName { get; set; }

    public string LastName { get; set; }

    public string Suffix { get; set; }

    public string BirthDate { get; set; }

    public string Gender { get; set; }

    public string SSN { get; set; }

    public string AddressType { get; set; }

    public string AddressLine1 { get; set; }

    public string AddressLine2 { get; set; }

    public string AddressLine3 { get; set; }

    public string City { get; set; }

    public string State { get; set; }

    public string ZipCode { get; set; }

    public string ZipPlusFour { get; set; }

    public string PhoneType { get; set; }

    public string PhoneNumber { get; set; }

    public string EmailType { get; set; }

    public string EmailAddress { get; set; }

    public bool ProtectedPopulationFlag { get; set; }

    public string ProtectedPopulationType { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member

}

