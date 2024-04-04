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
    private static CultureInfo culture = CultureInfo.CreateSpecificCulture("en-US");

    public static ClientIdentityEntity MapFromRequestToEntity(string linkId, DateTime mpiUpdated, DOH_PostClientIdentityRequest requests)
    {
        var result = new ClientIdentityEntity();
        result.Addresses = new List<ClientIdentityAddressEntity>();
        result.Communications = new List<ClientIdentityCommunicationEntity>();
        string strIdentities = requests.Content.Identity.ToString();
        Identity identity = JsonConvert.DeserializeObject<Identity>(strIdentities);
        //if (identity == null)
          //  throw Exception(invalid records");
        //var request = identity.;
        //DateTime.TryParse(Identity.SourceSystemUpdated, culture, DateTimeStyles.None, out var sourceSystemUpdated);
        //bool.TryParse(request.ProtectedPopulationFlag, out var protectedPopulationFlag);
        result.MpiLinkId = linkId;
        result.SourceSystemName = identity.Sources.FirstOrDefault().Name;
        result.SourceSystemId = identity.Sources.FirstOrDefault().Id;
        result.SourceSystemAgency = "";
        if (identity.Names.Count > 0)
        {
            result.FirstName = identity.Names.FirstOrDefault().First ?? "";
            result.MiddleName = identity.Names.FirstOrDefault().Middle ?? "";
            result.LastName = identity.Names.FirstOrDefault().Last ?? "";
            result.NameSuffix = identity.Names.FirstOrDefault().Suffix ?? "";
        }
        else
        {
            result.FirstName = "";
            result.MiddleName = "";
            result.LastName = "";
            result.NameSuffix = "";
        }
        if (identity.Ssns.Count > 0)
            result.Ssn = identity.Ssns.FirstOrDefault();
        else
            result.Ssn = "";

        if (identity.DatesOfBirth.Count > 0)
        {
        bool valiDob = DateOnly.TryParse(identity.DatesOfBirth.FirstOrDefault(), culture, DateTimeStyles.None, out var dob);
            result.Dob = valiDob ? dob : null;
        }
        if (identity.Genders.Count > 0)
            result.Gender = identity.Genders.FirstOrDefault();
        else
            result.Gender = "unknown";
        if (requests.protectedPopulation != null && requests.protectedPopulation.First() != null)
        {
            
            if (requests.protectedPopulation.First().ProtectedPopulationFlag != null)
            {
                if (requests.protectedPopulation.First().ProtectedPopulationFlag == "Y")
                {
                    result.ProtectedPopulationFlag = true;
                }
                else
                {
                    result.ProtectedPopulationFlag = false;
                }
            }
            else
            {
                result.ProtectedPopulationFlag = false;
            }
            if (requests.protectedPopulation.First().ProtectedPopulationTypes != null)
            {
                result.ProtectedPopulationType = string.Join(",", requests.protectedPopulation.First().ProtectedPopulationTypes);
            }
            else
            {
                result.ProtectedPopulationType = "";
            }
        }   
        else {
            result.ProtectedPopulationFlag = false;
            result.ProtectedPopulationType = "";
        }
        result.MpiUpdated = mpiUpdated;
        result.SourceSystemUpdated = DateTime.Now;
        result.IsActive = true;
        result.IsDelete = false;
        result.CreatedBy = "DOH";
        result.CreatedDate = DateTime.Now;
        result.UpdatedBy = "DOH";
        result.UpdatedDate = DateTime.Now;

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
        
        result.CustomJson = customCreateDatesJson;

        //var requestGroupedByAddress = requests.GroupBy(r => r, new ClientIdentityRequestAddressComparer());

        // TODO: make mapping shared code
        foreach (Address addressGroup in identity.Addresses)
        {
            List<ClientIdentityAddressCommunicationEntity> addressCommunications = new List<ClientIdentityAddressCommunicationEntity>();

            //var requestGroupedByCommunication = addressGroup.GroupBy(r => r, new ClientIdentityRequestCommunicationComparer());

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
                var communication = new ClientIdentityCommunicationEntity();
                //var communicationRequest = communicationGroup.FirstOrDefault();
                communication.MpiLinkId = result.MpiLinkId;
                communication.SourceSystemName = result.SourceSystemName;
                communication.SourceSystemId = result.SourceSystemId;
                communication.PhoneType = "";
                communication.EmailType = "";
                communication.EmailAddress = identity.Emails.FirstOrDefault() ?? "";
                communication.PhoneNumber = Regex.Replace(communicationGroup.Number ?? "", @"\D", ""); 
                communication.SourceSystemUpdated = result.SourceSystemUpdated;
                communication.IsActive = true;
                communication.IsDelete = false;
                communication.CreatedBy = "DOH";
                communication.CreatedDate = DateTime.Now;
                communication.UpdatedBy = "DOH";
                communication.UpdatedDate = DateTime.Now;
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
        return result;
    }

    public static List<ClientIdentityModel> MapToClientIdentityModel(IEnumerable<ClientIdentityEntity> entities)
    {
        var result = new List<ClientIdentityModel>();
        foreach (var entity in entities)
        {
            var model = MapToClientIdentityModel(entity);
            result.Add(model);
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

