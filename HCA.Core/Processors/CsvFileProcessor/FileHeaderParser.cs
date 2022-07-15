using System;
using System.Globalization;
using HCA.Infrastructure.Exceptions;
using HCA.Infrastructure.Extensions;

namespace HCA.Core.Processors.CsvFileProcessor;

public class FileHeaderParser : BaseParser<FileHeaderDataModel>
{
    private readonly string[] _fieldNames = new string[] { "Header", "Source System Agency", "Source System Name", "Created Date", "Created Time", "Api Call Type", "Operation Type" };

    public override (FileHeaderDataModel, string, bool) ParseData(string line)
    {
        var fieldValues = ParseValues(line, _fieldNames, true);

        var isEmptyLine = IsEmptyLine(fieldValues);

        if (isEmptyLine)
            return (new FileHeaderDataModel(), "", isEmptyLine);

        var (data, errorMessage) = CreateModel(fieldValues);
        return (data, errorMessage, false);
    }

    private (FileHeaderDataModel, string) CreateModel(Dictionary<string, string> values)
    {
        List<string> errorMessages = new List<string>();
        FileHeaderDataModel data = new FileHeaderDataModel();
        CultureInfo culture = CultureInfo.CreateSpecificCulture("en-US");
        string? sourceAgencyName = values.GetValue(_fieldNames[1]);
        string? sourceSystemName = values.GetValue(_fieldNames[2]);
        string? fileCreateDateStr = values.GetValue(_fieldNames[3]);
        string? fileCreateTimeStr = values.GetValue(_fieldNames[4]);
        string? operationTypeStr = values.GetValue(_fieldNames[5]);
        string? trackingIdStr = values.GetValue(_fieldNames[6]);

        if (sourceAgencyName.IsEmpty())
            SetErrorMessage(errorMessages, "Source Agency Name value is missing");

        if (sourceSystemName.IsEmpty())
            SetErrorMessage(errorMessages, "Source System Name value is missing");

        if (fileCreateDateStr.IsEmpty() || !DateOnly.TryParse(fileCreateDateStr, culture, DateTimeStyles.None, out DateOnly fileCreatedDate))
            SetErrorMessage(errorMessages, "File Created Date value is missing or invalid");

        if (fileCreateTimeStr.IsEmpty() || !TimeOnly.TryParse(fileCreateTimeStr, out TimeOnly fileCreatedTime))
            SetErrorMessage(errorMessages, "File Created Time value is missing or invalid");

        if (null == operationTypeStr || operationTypeStr.IsEmpty() || operationTypeStr == "NULL")
            operationTypeStr = "VE Post";

        if (operationTypeStr != "VE Post" || operationTypeStr != "VE Link" || operationTypeStr != "VE Un Link"
            || operationTypeStr != "VE Merge" || operationTypeStr != "VE Un Merge")
        {
            operationTypeStr = "VE Post";
        }

        data.SourceSystemAgency = sourceAgencyName ?? "";
        data.SourceSystemName = sourceSystemName ?? "";
        data.FileCreatedDate = fileCreatedDate;
        data.FileCreatedTime = fileCreatedTime;
        data.OperationType = operationTypeStr;
        return (data, GetParsingErrors(errorMessages));
    }
}

