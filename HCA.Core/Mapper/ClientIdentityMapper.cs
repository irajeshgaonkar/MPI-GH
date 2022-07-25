using HCA.Data.Entities;
using HCA.Infrastructure.Comparer;
using HCA.Models;
using HCA.Models.MuleSoft;
using HCA.Models.Request;

namespace HCA.Core.Mapper;

public class ClientIdentityMapper
{
    //public static ClientIdentity MapToModel(ClientIdentityEntity entity)
    //{
    //    return new ClientIdentity();
    //}

    public static ClientIdentityEntity MapFromRequestToEntity(string linkId, DateTime mpiUpdated, IEnumerable<ClientIdentityRequest> requests)
    {
        var result = new ClientIdentityEntity();
        result.Addresses = new List<ClientIdentityAddressEntity>();
        result.Communications = new List<ClientIdentityCommunicationEntity>();

        foreach (var request in requests)
        {
            result.MpiLinkId = linkId;
            result.SourceSystemName = request.SourceSystemName;
            result.SourceSystemId = request.SourceSystemId;
            result.SourceSystemAgency = request.SourceSystemAgency;
            result.FirstName = request.FirstName;
            result.LastName = request.LastName;
            result.NameSuffix = request.NameSuffix;
            result.Ssn = request.Ssn;
            result.Dob = request.Dob;
            result.Gender = request.Gender;
            result.ProtectedPopulationFlag = request.ProtectedPopulationFlag;
            result.ProtectedPopulationType = request.ProtectedPopulationType ?? "";
            result.MpiUpdated = mpiUpdated;
            result.SourceSystemUpdated = request.SourceSystemUpdated;
            result.IsActive = true;
            result.IsDelete = false;
            result.CreatedBy = "Batch File";
            result.CreatedDate = DateTime.Now;
            result.UpdatedBy = "Batch File";
            result.UpdatedDate = DateTime.Now;

            var requestGroupedByAddress = requests.GroupBy(r => r, new ClientIdentityRequestAddressComparer());

            foreach (var addressGroup in requestGroupedByAddress)
            {
                var address = new ClientIdentityAddressEntity();
                var addressRequest = addressGroup.First();
                address.MpiLinkId = result.MpiLinkId;
                address.SourceSystemName = result.SourceSystemName;
                address.SourceSystemId = result.SourceSystemId;
                address.AddressType = addressRequest.AddressType ?? "";
                address.AddressLine1 = addressRequest.AddressLine1;
                address.AddressLine2 = addressRequest.AddressLine2;
                address.AddressLine3 = addressRequest.AddressLine3;
                address.City = addressRequest.City;
                address.State = addressRequest.State;
                address.ZipCode = addressRequest.ZipCode;
                address.ZipFour = addressRequest.ZipFour;
                address.ZipCode = addressRequest.ZipCode;
                address.ZipFour = addressRequest.ZipFour;
                address.SourceSystemUpdated = request.SourceSystemUpdated;
                address.IsActive = true;
                address.IsDelete = false;
                address.CreatedBy = "Batch File";
                address.CreatedDate = DateTime.Now;
                address.UpdatedBy = "Batch File";
                address.UpdatedDate = DateTime.Now;
                result.Addresses.Add(address);
            }

            var requestGroupedByCommunication = requests.GroupBy(r => r, new ClientIdentityRequestCommunicationComparer());

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
                communication.SourceSystemUpdated = request.SourceSystemUpdated;
                communication.IsActive = true;
                communication.IsDelete = false;
                communication.CreatedBy = "Batch File";
                communication.CreatedDate = DateTime.Now;
                communication.UpdatedBy = "Batch File";
                communication.UpdatedDate = DateTime.Now;
                result.Communications.Add(communication);
            }
        }

        return result;
    }

    public static ClientIdentityModel MapToClientIdentityModel(ClientIdentityEntity entity)
    {
        var addresses = new List<ClientIdentityAddress>();

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

            addresses.Add(address);
        }

        var communications = new List<ClientIdentityCommunication>();

        foreach (var clientIdentyCommunication in entity.Communications)
        {
            var communication = new ClientIdentityCommunication()
            {
                PhoneType = clientIdentyCommunication.PhoneType ?? "",
                PhoneNumber = clientIdentyCommunication.PhoneNumber ?? "",
                EmailType = clientIdentyCommunication.EmailType ?? "",
                EmailAddress = clientIdentyCommunication.EmailAddress ?? ""
            };

            communications.Add(communication);
        }

        var result = new ClientIdentityModel();

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
        result.Communications = communications;

        return result;
    }

    public static List<ClientIdentity> MapToClientIdentity(ClientIdentityModel model)
    {
        var clientIdentities = new List<ClientIdentity>();

        foreach (var address in model.Addresses)
        {
            var clientIdentity = new ClientIdentity();

            clientIdentity.Id = model.Id;
            clientIdentity.MPILinkId = model.MpiLinkId ?? "";
            clientIdentity.SourceSystemId = model.SourceSystemId;
            clientIdentity.SourceName = model.SourceSystemName;
            clientIdentity.SourceSystemLastUpdate = model.SourceSystemUpdated;
            clientIdentity.FirstName = model.FirstName;
            clientIdentity.MiddleName = model.MiddleName ?? "";
            clientIdentity.LastName = model.LastName;
            clientIdentity.Suffix = model.NameSuffix ?? "";
            clientIdentity.BirthDate = model.DOB?.ToString() ?? "";
            clientIdentity.Gender = model.Gender;
            clientIdentity.SSN = model.SSN ?? "";
            clientIdentity.AddressType = address.AddressType ?? "";
            clientIdentity.AddressLine1 = address.AddressLine1;
            clientIdentity.AddressLine2 = address.AddressLine2 ?? "";
            clientIdentity.AddressLine3 = address.AddressLine3 ?? "";
            clientIdentity.City = address.City ?? "";
            clientIdentity.State = address.State ?? "";
            clientIdentity.ZipCode = address.ZipCode;
            clientIdentity.ZipPlusFour = address.ZipFour;
            clientIdentity.ProtectecPopulationFlag = model.ProtectedPopulationFlag;
            clientIdentity.ProtectedPopulationType = model.ProtectedPopulationType ?? "";

            foreach (var communication in model.Communications)
            {
                clientIdentity.PhoneNumber = communication.PhoneNumber ?? "";
                clientIdentity.EmailType = communication.EmailType ?? "";
                clientIdentity.EmailAddress = communication.EmailAddress ?? "";
            }

            clientIdentities.Add(clientIdentity);
        }

        return clientIdentities;
    }
}

