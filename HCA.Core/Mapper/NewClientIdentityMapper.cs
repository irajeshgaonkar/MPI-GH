using System.Globalization;
using System.Text.RegularExpressions;
using HCA.Data.Entities;
using HCA.Models;
using HCA.Models.MuleSoft;
using HCA.Models.Request;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace HCA.Core.Mapper;

public class NewClientIdentityMapper
{
    private static readonly CultureInfo culture = CultureInfo.CreateSpecificCulture("en-US");

    public static ClientIdentityEntity MapFromRequestToEntity(string linkId, DateTime mpiUpdated, DOH_PostClientIdentityRequest requests)
    {
        string strIdentities = requests.Content.Identity.ToString();
        Identity identity = JsonConvert.DeserializeObject<Identity>(strIdentities) ?? throw new Exception("Invalid Identity");

        var clientIdentityEntity = MapIdentityToClientIdentityEntity(linkId, mpiUpdated, identity);

        ProtectedPopulation? protectedPopulationFromRequest = requests.protectedPopulation?.First();
        if (protectedPopulationFromRequest != null)
        {
            // TODO: use String comparison everywhere; set up warning rule on direct comparison
            clientIdentityEntity.ProtectedPopulationFlag = string.Equals( "Y", protectedPopulationFromRequest.ProtectedPopulationFlag, StringComparison.OrdinalIgnoreCase );

            clientIdentityEntity.ProtectedPopulationType = protectedPopulationFromRequest.ProtectedPopulationTypes is not null
                ? string.Join( ";", protectedPopulationFromRequest.ProtectedPopulationTypes )
                : "";
        }   
        else {
            clientIdentityEntity.ProtectedPopulationFlag = false;
            clientIdentityEntity.ProtectedPopulationType = "";
        }

        // Deserialize the JSON string into a JObject 
        JObject json = JObject.Parse(strIdentities);
        string customCreateDatesJson = string.Empty;
        // Find the index of the start and end of "custom.CreateDates" array
        int startIndex = strIdentities.IndexOf("\"custom.CreateDates\":");
        if (startIndex != -1)
        {
            int endIndex = strIdentities.IndexOf("]", startIndex);

            // Extract the substring containing "custom.CreateDates" array
            if (startIndex > 0)
            {
                customCreateDatesJson = strIdentities.Substring(startIndex, endIndex - startIndex + 1);
            }
        }
        
        clientIdentityEntity.CustomJson = customCreateDatesJson;

        return clientIdentityEntity;
    }

    public static ClientIdentityEntity MapIdentityToClientIdentityEntity(string linkId, DateTime mpiUpdated, Identity identity, bool isDelete = false)
    {
        var result = new ClientIdentityEntity
        {
            Addresses = [],
            Communications = [],

            MpiLinkId = linkId
        };

        if (identity.Sources.Count > 0 && identity.Sources.First().Name != null && identity.Sources.First().Id != null)
        {
            result.SourceSystemName = identity.Sources.First().Name;
            result.SourceSystemId = identity.Sources.First().Id;
        }
        else 
        {
            throw new InvalidDataException("Invalid Identity Sources.");
        }

        result.SourceSystemAgency = "";

        if (identity.Names.Count > 0)
        {
            result.FirstName = identity?.Names?.First()?.First ?? "";
            result.MiddleName = identity?.Names?.First()?.Middle ?? "";
            result.LastName = identity?.Names?.First()?.Last ?? "";
            result.NameSuffix = identity?.Names?.First()?.Suffix ?? "";
        }
        else
        {
            result.FirstName = "";
            result.MiddleName = "";
            result.LastName = "";
            result.NameSuffix = "";
        }

        result.Ssn = identity?.Ssns.Count > 0 ? identity.Ssns.First() : "";

        if (identity?.DatesOfBirth.Count > 0)
        {
            bool valiDob = DateOnly.TryParse(identity.DatesOfBirth.First(), culture, DateTimeStyles.None, out var dob);
            result.Dob = valiDob ? dob : null;
        }
        
        result.Gender = identity?.Genders != null && identity.Genders.Count > 0
            ? identity.Genders.First() ?? "unknown"
            : "unknown";

        result.IsDelete = isDelete;
        result.CreatedBy = result.SourceSystemName;
        result.UpdatedBy = result.SourceSystemName;
        result.SetAllDateTimesToNow();

        if(identity?.Addresses.Count > 0)
        {
            foreach (Address addressGroup in identity.Addresses)
            {
                List<ClientIdentityAddressCommunicationEntity> addressCommunications = [];

                var address = new ClientIdentityAddressEntity
                {
                    ClientIdentity = result,
                    AddressType = "",
                    AddressLine1 = addressGroup.Line1,
                    AddressLine2 = addressGroup.Line2,
                    AddressLine3 = "",
                    City = addressGroup.City,
                    State = addressGroup.State,
                    ZipCode = addressGroup.PostalCode,
                    ZipFour = addressGroup.ZipFour ?? "",
                    IsActive = true,
                    IsDelete = false,
                    CreatedBy = "DOH",
                    CreatedDate = DateTime.Now,
                    UpdatedBy = "DOH",
                    UpdatedDate = DateTime.Now
                };
                result.Addresses.Add(address);


                foreach (var communicationGroup in identity.PhoneNumbers)
                {
                    var communication = new ClientIdentityCommunicationEntity
                    {
                        MpiLinkId = result.MpiLinkId,
                        SourceSystemName = result.SourceSystemName,
                        SourceSystemId = result.SourceSystemId,
                        PhoneType = "",
                        EmailType = "",
                        EmailAddress = identity.Emails.FirstOrDefault() ?? "",
                        PhoneNumber = Regex.Replace(communicationGroup.Number ?? "", @"\D", ""),
                        SourceSystemUpdated = result.SourceSystemUpdated,
                        IsActive = true,
                        IsDelete = false,
                        CreatedBy = "DOH",
                        CreatedDate = DateTime.Now,
                        UpdatedBy = "DOH",
                        UpdatedDate = DateTime.Now
                    };
                    result.Communications.Add(communication);

                    var addressCommunication = new ClientIdentityAddressCommunicationEntity()
                    {

                        Address = address,
                        Communication = communication
                    };

                    addressCommunications.Add(addressCommunication);
                }

                address.AddressCommunications = addressCommunications;

            }
        }

        return result;
    }

    public static ClientIdentityModel MapToClientIdentityModel(ClientIdentityEntity entity)
    {
        var addresses = new List<ClientIdentityAddress>();

        if (entity.Addresses != null)
        {
            foreach (var clientIdentyAddress in entity.Addresses)
            {
                var address = new ClientIdentityAddress()
                {
                    AddressType = clientIdentyAddress.AddressType ?? "",
                    AddressLine1 = clientIdentyAddress.AddressLine1,
                    AddressLine2 = clientIdentyAddress.AddressLine2,
                    AddressLine3 = clientIdentyAddress.AddressLine3,
                    City = clientIdentyAddress.City,
                    State = clientIdentyAddress.State,
                    ZipCode = clientIdentyAddress.ZipCode,
                    ZipFour = clientIdentyAddress.ZipFour
                };

                var communications = new List<ClientIdentityCommunication>();
                foreach (var addressCommunication in clientIdentyAddress.AddressCommunications)
                {
                    var clientIdentyCommunication = addressCommunication.Communication;

                    var communication = new ClientIdentityCommunication()
                    {
                        PhoneType = clientIdentyCommunication.PhoneType ?? "",
                        PhoneNumber = clientIdentyCommunication.PhoneNumber ?? "",
                        EmailType = clientIdentyCommunication.EmailType ?? "",
                        EmailAddress = clientIdentyCommunication.EmailAddress ?? ""
                    };
                    communications.Add(communication);
                }
                address.Communications = communications;
                addresses.Add(address);
            }
        }
        var result = new ClientIdentityModel();
        result.Id = entity.Id;
        result.MpiLinkId = entity.MpiLinkId!;
        result.FirstName = entity.FirstName;
        result.LastName = entity.LastName;
        result.MiddleName = entity.MiddleName;
        result.SourceSystemId = entity.SourceSystemId;
        result.SourceSystemName = entity.SourceSystemName;
        result.SourceSystemAgency = entity.SourceSystemAgency;
        result.NameSuffix = entity.NameSuffix;
        result.SSN = entity.Ssn;
        result.DOB = entity.Dob;
        result.Gender = entity.Gender;
        result.ProtectedPopulationFlag = entity.ProtectedPopulationFlag;
        result.ProtectedPopulationType = entity.ProtectedPopulationType!;
        result.SourceSystemUpdated = entity.SourceSystemUpdated.ToUniversalTime();
        result.Addresses = addresses;
        return result;
    }

}

