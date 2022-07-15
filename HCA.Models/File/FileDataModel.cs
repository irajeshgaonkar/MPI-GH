using System;
using HCA.Models;
using HCA.Models.Request;

namespace HCA.Core.Processors.CsvFileProcessor;

public class FileDataModel
{
    public FileDataModel(FileHeaderDataModel headerData, List<ClientIdentityRequest> identities)
    {
        HeaderData = headerData;
        Identities = identities;
    }

    public FileHeaderDataModel HeaderData { get; set; }

    public List<ClientIdentityRequest> Identities { get; set; }
}

public class FileHeaderDataModel
{
    public FileHeaderDataModel() { }

    public FileHeaderDataModel(string sourceSystemAgency, string sourceSystemName, DateOnly date, TimeOnly time, string operationType)
    {
        SourceSystemAgency = sourceSystemAgency;
        SourceSystemName = sourceSystemName;
        FileCreatedDate = date;
        FileCreatedTime = time;
        OperationType = operationType;
    }

    public string SourceSystemAgency { get; set; }

    public string SourceSystemName { get; set; }

    public DateOnly FileCreatedDate { get; set; }

    public TimeOnly FileCreatedTime { get; set; }

    public string OperationType { get; set; }

    public string TrackingId { get; set; }
}

