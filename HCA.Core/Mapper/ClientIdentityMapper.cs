using System.Globalization;
using HCA.Data.Entities;
using HCA.Infrastructure.Comparer;
using HCA.Models;
using HCA.Models.Request;

namespace HCA.Core.Mapper;

// TODO: Use AutoMapper
public class ClientIdentityMapper
{
    private static CultureInfo culture = CultureInfo.CreateSpecificCulture("en-US");

    public static ClientIdentityEntity MapFromRequestToEntity(string linkId, DateTime mpiUpdated, IEnumerable<ClientIdentityRequest> requests)
    {
        var result = new ClientIdentityEntity();
        result.Addresses = new List<ClientIdentityAddressEntity>();
        result.Communications = new List<ClientIdentityCommunicationEntity>();

        var request = requests.First();
        bool valiDob = DateOnly.TryParse(request.Dob, culture, DateTimeStyles.None, out var dob);
        DateTime.TryParse(request.SourceSystemUpdated, culture, DateTimeStyles.None, out var sourceSystemUpdated);
        bool.TryParse(request.ProtectedPopulationFlag, out var protectedPopulationFlag);
        result.MpiLinkId = linkId;
        result.SourceSystemName = request.SourceSystemName;
        result.SourceSystemId = request.SourceSystemId;
        result.SourceSystemAgency = request.SourceSystemAgency;
        result.FirstName = request.FirstName;
        result.MiddleName = request.MiddleName;
        result.LastName = request.LastName;
        result.NameSuffix = request.NameSuffix;
        result.Ssn = request.Ssn;
        result.Dob = valiDob ? dob : null;
        result.Gender = request.Gender;
        if (request.ProtectedPopulationFlag != null)
        {
            result.ProtectedPopulationFlag = string.Equals("Y", request.ProtectedPopulationFlag, StringComparison.OrdinalIgnoreCase);
        }
        else
            result.ProtectedPopulationFlag = false;
        result.ProtectedPopulationType = request.ProtectedPopulationType is not null ? string.Join(",", request.ProtectedPopulationType) : "";
        result.MpiUpdated = mpiUpdated;
        result.SourceSystemUpdated = sourceSystemUpdated;
        result.IsActive = true;
        result.IsDelete = false;
        result.CreatedBy = "Batch File";
        result.CreatedDate = DateTime.Now;
        result.UpdatedBy = "Batch File";
        result.UpdatedDate = DateTime.Now;
        result.CustomJson = request.CustomJson;

        var requestGroupedByAddress = requests.GroupBy(r => r, new ClientIdentityRequestAddressComparer());
        
        foreach (var addressGroup in requestGroupedByAddress)
        {
            List<ClientIdentityAddressCommunicationEntity> addressCommunications = new List<ClientIdentityAddressCommunicationEntity>();
            var requestGroupedByCommunication = addressGroup.GroupBy(r => r, new ClientIdentityRequestCommunicationComparer());
            ClientIdentityRequest addressRequest = addressGroup.First();

            var address = new ClientIdentityAddressEntity
            {
                ClientIdentity = result,
                AddressType = addressRequest.AddressType ?? "",
                AddressLine1 = addressRequest.AddressLine1,
                AddressLine2 = addressRequest.AddressLine2,
                AddressLine3 = addressRequest.AddressLine3,
                City = addressRequest.City,
                State = addressRequest.State,
                ZipCode = addressRequest.ZipCode,
                ZipFour = addressRequest.ZipFour,
                IsActive = true,
                IsDelete = false,
                CreatedBy = "Batch File",
                CreatedDate = DateTime.Now,
                UpdatedBy = "Batch File",
                UpdatedDate = DateTime.Now
            };
            result.Addresses.Add(address);

            foreach (var communicationGroup in requestGroupedByCommunication)
            {
                var communication = new ClientIdentityCommunicationEntity();
                var communicationRequest = communicationGroup.First();
                communication.MpiLinkId = result.MpiLinkId;
                communication.SourceSystemName = result.SourceSystemName;
                communication.SourceSystemId = result.SourceSystemId;
                communication.PhoneType = communicationRequest.PhoneType ?? "";
                communication.EmailType = communicationRequest.EmailType ?? "";
                communication.EmailAddress = communicationRequest.EmailAddress ?? "";
                communication.PhoneNumber = communicationRequest.PhoneNumber ?? "";
                communication.SourceSystemUpdated = result.SourceSystemUpdated;
                communication.IsActive = true;
                communication.IsDelete = false;
                communication.CreatedBy = "Batch File";
                communication.CreatedDate = DateTime.Now;
                communication.UpdatedBy = "Batch File";
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

