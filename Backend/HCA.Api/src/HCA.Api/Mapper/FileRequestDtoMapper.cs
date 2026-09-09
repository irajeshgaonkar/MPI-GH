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
            if(model.Addresses == null || model.Addresses.Count == 0)
            {
                var dto = CreateBaseDto(model, showSensitiveData);
                dtos.Add(dto);
            }
            else
            {
                foreach (var address in model.Addresses)
                {
                    // If there are no communications, still create a record for the address
                    if (address.Communications == null || address.Communications.Count == 0)
                    {
                        var dto = CreateBaseDto(model, showSensitiveData);
                        AddAddressData(dto, address);
                        dtos.Add(dto);
                    }
                    else
                    {
                        foreach (var communication in address.Communications)
                        {
                            var dto = CreateBaseDto(model, showSensitiveData);
                            AddAddressData(dto, address);
                            AddCommunicationData(dto, communication);
                            dtos.Add(dto);
                        }
                    }
                }
            }
        }

        return dtos;
    }

    private static ClientIdentityDto CreateBaseDto(ClientIdentityModel model, bool showSensitiveData)
    {
        return new ClientIdentityDto
        {
            Id = model.Id,
            MPILinkId = model.MpiLinkId ?? "",
            SourceName = model.SourceSystemName,
            Tenant = model.Tenant ?? "",
            SourceSystemId = model.SourceSystemId,
            SourceSystemLastUpdate = model.SourceSystemUpdated,
            FirstName = model.FirstName,
            MiddleName = model.MiddleName ?? "",
            LastName = model.LastName ?? "",
            Suffix = model.NameSuffix ?? "",
            BirthDate = showSensitiveData ? model.DOB?.ToString() ?? "" : "*****",
            Gender = model.Gender ?? "",
            SSN = showSensitiveData ? model.SSN ?? "" : "*****",
            ProtectecPopulationFlag = model.ProtectedPopulationFlag,
            ProtectedPopulationType = model.ProtectedPopulationType ?? ""
        };
    }

    private static void AddAddressData(ClientIdentityDto dto, ClientIdentityAddress address)
    {
        dto.AddressType = address.AddressType ?? "";
        dto.AddressLine1 = address.AddressLine1 ?? "";
        dto.AddressLine2 = address.AddressLine2 ?? "";
        dto.AddressLine3 = address.AddressLine3 ?? "";
        dto.City = address.City ?? "";
        dto.State = address.State ?? "";
        dto.ZipCode = address.ZipCode ?? "";
        dto.ZipPlusFour = address.ZipFour ?? "";
    }

    private static void AddCommunicationData(ClientIdentityDto dto, ClientIdentityCommunication communication)
    {
        dto.PhoneType = communication.PhoneType ?? "";
        dto.PhoneNumber = communication.PhoneNumber ?? "";
        dto.EmailType = communication.EmailType ?? "";
        dto.EmailAddress = communication.EmailAddress ?? "";
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
                Tenant = model.Tenant ?? "",
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

