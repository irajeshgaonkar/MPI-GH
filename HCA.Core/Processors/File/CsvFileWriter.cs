using HCA.Data.Entities;
using HCA.Data.Repository;
namespace HCA.Core.Processors.File;

public class CsvFileWriter : IFileWriter
{
    private readonly IFileRequestRepository _fileRequestRepository;
    private readonly IClientIdentityRequestRepository _clientIdentityRequestRepository;

    public CsvFileWriter(IFileRequestRepository fileRequestRepository, IClientIdentityRequestRepository clientIdentityRequestRepository)
    {
        _fileRequestRepository = fileRequestRepository;
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
        var fileCreatedTime = fileRequestEntity.FileCreatedDateTime.ToString("hh:mm tt");
        var line = $"{fileRequestEntity.SourceSystemAgency},{fileRequestEntity.SourceSystemName},{fileCreatedDate},{fileCreatedTime},{fileRequestEntity.ApiCallType},";
        for (int i = 6; i < 26; ++i)
        {
            line += ",";
        }
        return line;
    }

    public string GetTrailerLine(FileRequestEntity fileRequestEntity)
    {
        var line = $"{fileRequestEntity.Trailer},";
        for(int i = 1; i <= 26; ++i)
        {
            line += ",";
        }
        return line;
    }

    public List<string> GetRequestLines(IEnumerable<ClientIdentityRequestEntity> requests)
    {
        var lines = new List<string>();

        foreach (var request in requests)
        {
            var line = $"{request.MpiLinkId},{request.SourceSystemId},{request.SourceSystemUpdated},{request.FirstName},";
            line += $"{request.MiddleName},{request.LastName},{request.NameSuffix},{request.Dob},";
            line += $"{request.Gender},{request.Ssn},{request.AddressType},{request.AddressLine1},{request.AddressLine2},";
            line += $"{request.AddressLine3},{request.City},{request.State},{request.ZipCode},";
            line += $"{request.ZipFour},{request.PhoneType},{request.PhoneNumber},";
            line += $"{request.EmailType},{request.EmailAddress},{request.ProtectedPopulationFlag},";
            line += $"{request.ProtectedPopulationType},,,";
            lines.Add(line);
        }

        return lines;
    }


}