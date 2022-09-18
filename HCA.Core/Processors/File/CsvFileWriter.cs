using HCA.Data.Entities;
using HCA.Data.Repository;
namespace HCA.Core.Processors.File;

public class CsvFileWriter : IFileWriter
{
    private readonly IClientIdentityRequestRepository _clientIdentityRequestRepository;

    public CsvFileWriter(IClientIdentityRequestRepository clientIdentityRequestRepository)
    {
        _clientIdentityRequestRepository = clientIdentityRequestRepository;
    }

    public async Task<MemoryStream> WriteFile(FileRequestEntity fileRequest)
    {
        var requests = await _clientIdentityRequestRepository.GetRequests(fileRequest.RequestId);

        MemoryStream streamToReturn = new MemoryStream();
        var writer = new StreamWriter(streamToReturn);

        var headerLine = GetCsvHeader(fileRequest);
        writer.WriteLine(headerLine);
        var lines = GetRequestLines(requests);

        foreach (var line in lines)
        {
            writer.WriteLine(line);
        }

        var trailerLine = GetTrailerLine(fileRequest);
        writer.WriteLine(trailerLine);
        writer.Flush();
        return streamToReturn;
    }

    public string GetCsvHeader(FileRequestEntity fileRequestEntity)
    {
        var fileCreatedDate = fileRequestEntity.FileCreatedDateTime.ToString("MM/dd/yyyy");
        var fileCreatedTime = fileRequestEntity.FileCreatedDateTime.ToString("hh:mm:ss");
        var line = $"{fileRequestEntity.SourceSystemAgency},{fileRequestEntity.SourceSystemName},{fileCreatedDate},{fileCreatedTime},{fileRequestEntity.ApiCallType}";
        return line;
    }

    public string GetTrailerLine(FileRequestEntity fileRequestEntity)
    {
        var line = $"TRALR,{fileRequestEntity.Trailer}";
        return line;
    }

    public List<string> GetRequestLines(IEnumerable<ClientIdentityRequestEntity> requests)
    {
        var lines = new List<string>();

        foreach (var request in requests)
        {
            var line = $"{ProcessFieldForWriting(request.MpiLinkId)},{ProcessFieldForWriting(request.SourceSystemId)},{ProcessFieldForWriting(request.SourceSystemUpdated)},{ProcessFieldForWriting(request.FirstName)},";
            line += $"{ProcessFieldForWriting(request.MiddleName)},{ProcessFieldForWriting(request.LastName)},{ProcessFieldForWriting(request.NameSuffix)},{ProcessFieldForWriting(request.Dob)},";
            line += $"{ProcessFieldForWriting(request.Gender)},{ProcessFieldForWriting(request.Ssn)},{ProcessFieldForWriting(request.AddressType)},{ProcessFieldForWriting(request.AddressLine1)},{ProcessFieldForWriting(request.AddressLine2)},";
            line += $"{ProcessFieldForWriting(request.AddressLine3)},{ProcessFieldForWriting(request.City)},{ProcessFieldForWriting(request.State)},{ProcessFieldForWriting(request.ZipCode)},";
            line += $"{ProcessFieldForWriting(request.ZipFour)},{ProcessFieldForWriting(request.PhoneType)},{ProcessFieldForWriting(request.PhoneNumber)},";
            line += $"{ProcessFieldForWriting(request.EmailType)},{ProcessFieldForWriting(request.EmailAddress)},{ProcessFieldForWriting(request.ProtectedPopulationFlag)},";
            line += $"{ProcessFieldForWriting(request.ProtectedPopulationType)}";
            lines.Add(line);
        }

        return lines;
    }

    private string ProcessFieldForWriting(string? value)
    {
        if (value?.Contains(",") == true) return $"\"{value}\"";
        return value ?? string.Empty;
    }


}