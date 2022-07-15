using System;
using System.ComponentModel.DataAnnotations;

namespace HCA.Models
{
    public class PagenatedCollection<T>
    {
        public int RecordsCount { get; set; }

        public int PageNumber { get; set; }

        public int RecordsPerPage { get; set; }

        public IEnumerable<T> Data { get; set; }
    }

    public class ClientIdentityModel
    {
        public string? MpiLinkId { get; set; }

        public int Id { get; set; }

        [Required]
        public string SourceSystemId { get; set; }

        [Required]
        public string SourceSystemName { get; set; }

        [Required]
        public string SourceSystemAgency { get; set; }

        [Required]
        public string FirstName { get; set; }

        public string? MiddleName { get; set; }

        [Required]  
        public string LastName { get; set; }

        public string? NameSuffix { get; set; }

        public string? SSN { get; set; }

        public DateOnly? DOB { get; set; }

        [Required]
        public string Gender { get; set; }

        public bool ProtectedPopulationFlag { get; set; }

        public string? ProtectedPopulationType { get; set; }

        [Required]
        public DateTime SourceSystemUpdated { get; set; }

        public List<ClientIdentityAddress> Addresses { get; set; }

        public List<ClientIdentityCommunication> Communications { get; set; }
    }

    public class ClientIdentityAddress
    {
        public string? AddressType { get; set; }

        [Required]
        public string AddressLine1 { get; set; }

        public string? AddressLine2 { get; set; }

        public string? AddressLine3 { get; set; }

        [Required]
        public string City { get; set; }

        public string State { get; set; }

        [Required]
        public string ZipCode { get; set; }

        [Required]
        public string ZipFour { get; set; }
    }

    public class ClientIdentityCommunication
    {
        public string? PhoneType { get; set; }

        public string? EmailType { get; set; }

        [Phone]
        public string? PhoneNumber { get; set; }

        [EmailAddress]
        public string? EmailAddress { get; set; }
    }
}

