using System;
using HCA.Core.Processors;
using HCA.Infrastructure.Exceptions;
using HCA.Infrastructure.Extensions;
using HCA.Models;
using HCA.Models.Request;

namespace HCA.Core.Processors.CsvFileProcessor;

public class CsvFileParser : IFileParser
{
    private readonly IFileReader _fileReader;

    private readonly BaseParser<FileHeaderDataModel> _headerDataParser;

    private readonly BaseParser<ClientIdentityRequest> _clientIdentityParser;

    public CsvFileParser(IFileReader fileReader, BaseParser<FileHeaderDataModel> headerDataParser, BaseParser<ClientIdentityRequest> clientIdentityParser)
    {
        _fileReader = fileReader;
        _headerDataParser = headerDataParser;
        _clientIdentityParser = clientIdentityParser;
    }

    public (IEnumerable<ClientIdentityRequest>, Dictionary<int, string>, FileHeaderDataModel) ParseFile(StreamReader stream)
    {
        var errorMessages = new Dictionary<int, string>();
        var clientIdentities = new List<ClientIdentityRequest>();
        var lines = _fileReader.ReadLines(stream);
        var (headerData, headerErrorMessage, isEmptyLine) = _headerDataParser.ParseData(lines[0]);

        if (!headerErrorMessage.IsEmpty() || isEmptyLine)
            return(clientIdentities, new Dictionary<int, string>() { }, headerData);

        for (int i = 3; i < lines.Count; ++i)
        {
            var (clientIdentityData, identityErrorMessage, isLastLine) = _clientIdentityParser.ParseData(lines[i]);

            if (isLastLine)
                break;

            if (!identityErrorMessage.IsEmpty())
            {
                errorMessages.Add(i, $"Row {i} {identityErrorMessage}");
                continue;
            }

            clientIdentityData.SourceSystemAgency = headerData.SourceSystemAgency;
            clientIdentityData.SourceSystemName = headerData.SourceSystemName;
            clientIdentities.Add(clientIdentityData);

        }

        return (clientIdentities, errorMessages, headerData);
    }
}

