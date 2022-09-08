using System;
using HCA.Api.Dto;
using HCA.Models;
using HCA.Models.Request;

namespace HCA.Api.Mapper;

public static class FileRequestDtoMapper
{
    public static FileRequestDto GetDto(FileRequest fileRequest)
    {
        return new FileRequestDto()
        {
            RequestId = fileRequest.RequestId,
            TrackingId = fileRequest.TrackingId,
            FileName = fileRequest.FileName,
            OutputFileName = fileRequest.OutputFileName,
            SourceSystemAgency = fileRequest.SourceSystemAgency,
            SourceSystemName = fileRequest.SourceSystemName,
            RecordsCount = fileRequest.RecordsCount,
            Trailer = fileRequest.Trailer,
            ApiCallType = fileRequest.ApiCallType,
            FileCreatedDateTime = fileRequest.FileCreatedDateTime,
            RequestDateTime = fileRequest.RequestDateTime,
            ProcessStartTime = fileRequest.ProcessStartTime,
            ProcessEndTime = fileRequest.ProcessEndTime,
            Status = fileRequest.Status,
            Message = fileRequest.Message
        };
    }
}

public static class ClientIdentityDtoMapper
{
    public static IList<ClientIdentityDto> GetDto(IEnumerable<ClientIdentityModel> models)
    {
        var dtos = new List<ClientIdentityDto>();

        foreach (var model in models)
        {
            foreach (var address in model.Addresses)
            {
                foreach (var communication in address.Communications)
                {
                    var clientIdentityDto = new ClientIdentityDto()
                    {
                        Id = model.Id,
                        MPILinkId = model.MpiLinkId ?? "",
                        SourceName = model.SourceSystemName,
                        SourceSystemId = model.SourceSystemId,
                        SourceSystemLastUpdate = model.SourceSystemUpdated,
                        FirstName = model.FirstName,
                        MiddleName = model.MiddleName ?? "",
                        LastName = model.LastName ?? "",
                        Suffix = model.NameSuffix ?? "",
                        BirthDate = model.DOB.ToString() ?? "",
                        Gender = model.Gender ?? "",
                        SSN = model.SSN ?? "",
                        AddressType = address.AddressType ?? "",
                        AddressLine1 = address.AddressLine1 ?? "",
                        AddressLine2 = address.AddressLine2 ?? "",
                        AddressLine3 = address.AddressLine3 ?? "",
                        City = address.City ?? "",
                        State = address.State ?? "",
                        ZipCode = address.ZipCode ?? "",
                        ZipPlusFour = address.ZipFour ?? "",
                        PhoneType = communication.PhoneType ?? "",
                        PhoneNumber = communication.PhoneNumber ?? "",
                        EmailType = communication.EmailType ?? "",
                        EmailAddress = communication.EmailAddress ?? "",
                        ProtectecPopulationFlag = model.ProtectedPopulationFlag,
                        ProtectedPopulationType = model.ProtectedPopulationType ?? ""
                    };

                    dtos.Add(clientIdentityDto);
                }
            }
        }

        return dtos;
    }
}