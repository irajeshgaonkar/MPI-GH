using System;
using HCA.Models.Request;

namespace HCA.Core.Processors.CsvFileProcessor;

public class CsvFileReader : IFileReader
{
    public List<string> ReadLines(StreamReader streamReader)
    {
        List<string> lines = new List<string>();

        string? line;

        do
        {
            line = streamReader.ReadLine();
            if (!string.IsNullOrWhiteSpace(line))
                lines.Add(line);

        } while (line != null);

        return lines;
    }
}

public class CsvFileWrite
{
    public MemoryStream WriteLine(List<string> streamReader)
    {
        return new MemoryStream();
    }

    public List<string> GetCsvFileLines(IEnumerable<ClientIdentityRequest> requests)
    {
        string[] _fieldNames = new string[] { "MPI Link ID", "Source System ID",
            "First Name", "Middle Name", "Last Name","Suffix", "Birth Date", "Gender", "SSN", "Address Type", "Address Line 1",
            "Address Line 2", "Address Line 3", "City", "State", "Zip Code", "Zip Plus Four", "Phone type", "Phone number",
            "Email type", "Email Address", "Protectec Population Flag", "Protected Population Type" };

        var lines = new List<string>();

        foreach (var request in requests)
        {
            var line = $"{request.MpiLinkId},{request.SourceSystemId},{request.SourceSystemUpdated},{request.FirstName},";
            line += $"{request.MiddleName},{request.LastName},{request.NameSuffix},{request.Dob},";
            line += $"{request.Gender},{request.Ssn},{request.AddressType},{request.AddressLine1},{request.AddressLine2},";
            line += $"{request.AddressLine3},{request.City},{request.State},{request.ZipCode},";
            line += $"{request.ZipFour},{request.PhoneType},{request.PhoneNumber},";
            line += $"{request.EmailType},{request.EmailAddress},{request.ProtectedPopulationFlag},";
            line += $"{request.ProtectedPopulationType},";
            line += $"{request.Status},{request.Message}";

            lines.Add(line);
        }

        return lines;
    }
}

