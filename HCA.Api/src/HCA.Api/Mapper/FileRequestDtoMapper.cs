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
    public static IList<ClientIdentityDto> GetDto(IEnumerable<ClientIdentityModel> models, bool showSensitiveData)
    {
        var dtos = new List<ClientIdentityDto>();

        foreach (var model in models)
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
                BirthDate = showSensitiveData ? model.DOB.ToString() ?? "" : "*****",
                Gender = model.Gender ?? "",
                SSN = showSensitiveData ? model.SSN ?? "" : "*****",
                ProtectecPopulationFlag = model.ProtectedPopulationFlag,
                ProtectedPopulationType = model.ProtectedPopulationType ?? ""
            };

            foreach (var address in model.Addresses)
            {
                clientIdentityDto.AddressType = address.AddressType ?? "";
                clientIdentityDto.AddressLine1 = address.AddressLine1 ?? "";
                clientIdentityDto.AddressLine2 = address.AddressLine2 ?? "";
                clientIdentityDto.AddressLine3 = address.AddressLine3 ?? "";
                clientIdentityDto.City = address.City ?? "";
                clientIdentityDto.State = address.State ?? "";
                clientIdentityDto.ZipCode = address.ZipCode ?? "";
                clientIdentityDto.ZipPlusFour = address.ZipFour ?? "";

                foreach (var communication in address.Communications)
                {
                    clientIdentityDto.PhoneType = communication.PhoneType ?? "";
                    clientIdentityDto.PhoneNumber = communication.PhoneNumber ?? "";
                    clientIdentityDto.EmailType = communication.EmailType ?? "";
                    clientIdentityDto.EmailAddress = communication.EmailAddress ?? "";

                }
            }

            dtos.Add(clientIdentityDto);
        }

        return dtos;
    }

    public static IList<ClientIdentityDto> GetReportsDto(IEnumerable<ClientIdentityModel> models, bool showSensitiveData)
    {
        var dtos = new List<ClientIdentityDto>();

        foreach (var model in models)
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
                BirthDate = showSensitiveData ? model.DOB.ToString() ?? "" : "*****",
                Gender = model.Gender ?? "",
                SSN = showSensitiveData ? model.SSN ?? "" : "*****",
                ProtectecPopulationFlag = model.ProtectedPopulationFlag,
                ProtectedPopulationType = model.ProtectedPopulationType ?? ""
            };
            dtos.Add(clientIdentityDto);
        }
        return dtos;
    }
}
public static class CustomDataMappingDtoMapper
{
    public static CustomDataMappingDto GetDto(CustomDataMapping customData)
    {
        return new CustomDataMappingDto()
        {
            Id = customData.Id,
            SourceSystemName = customData.SourceSystemName,
            InputIndex = customData.InputIndex,
            InputColumnName = customData.InputColumnName,
            VeratoRequestPath = customData.VeratoRequestPath,
            VeratoResponsePath = customData.VeratoResponsePath,
            APIResponsePath = customData.APIResponsePath,
            OutputIndex = customData.OutputIndex,
            OutputColumnName = customData.OutputColumnName
        };
    }
    public static IList<CustomDataMappingDto?> GetListDto(IEnumerable<CustomDataMapping?> customDatas)
    {
        var listDto = new List<CustomDataMappingDto?>();
        foreach (CustomDataMapping customData in customDatas)
        {
            var fileDto = new CustomDataMappingDto()
            {
                Id = customData.Id,
                SourceSystemName = customData.SourceSystemName,
                InputIndex = customData.InputIndex,
                InputColumnName = customData.InputColumnName,
                VeratoRequestPath = customData.VeratoRequestPath,
                VeratoResponsePath = customData.VeratoResponsePath,
                APIResponsePath = customData.APIResponsePath,
                OutputIndex = customData.OutputIndex,
                OutputColumnName = customData.OutputColumnName
            };
            listDto.Add(fileDto);
        };
        return listDto;
    }

    public static CustomDataMapping MapDtoToEntity(CustomDataMappingDto customData)
    {
        return new CustomDataMapping()
        {
            Id = customData.Id,
            SourceSystemName = customData.SourceSystemName,
            InputIndex = customData.InputIndex,
            InputColumnName = customData.InputColumnName,
            VeratoRequestPath = customData.VeratoRequestPath,
            VeratoResponsePath = customData.VeratoResponsePath,
            APIResponsePath = customData.APIResponsePath,
            OutputIndex = customData.OutputIndex,
            OutputColumnName = customData.OutputColumnName
        };
    }
}

