using System;
using System.Text;
using HCA.Infrastructure.Extensions;

namespace HCA.Core.Processors.CsvFileProcessor;

public abstract class BaseParser<T>
{
    public abstract (T, string, bool) ParseData(string line);

    public string GetParsingErrors(List<string> errorMessages) => errorMessages.CombineToString();

    protected string[] GetFields(string line)
    {
        char fieldSplitter = ',';
        var fields = line.SplitByChar(fieldSplitter);
        return fields;
    }
        
    protected Dictionary<string, string> ParseValues(string line, string[] fieldNames, bool removeDescriptionValue = false)
    {
        var fieldValues = new Dictionary<string, string>();
        var fields = GetFields(line);

        for (int i = 0; i < fields.Length && i < fieldNames.Length; ++i)
        {
            fieldValues[fieldNames[i]] = TrimValue(fields[i], removeDescriptionValue) ?? "";
        }

        return fieldValues;
    }

    protected bool IsEmptyLine(Dictionary<string, string> values)
    {
        foreach(var value in values.Values)
        {
            if (!value.IsEmpty())
                return false;
        }

        return true;
    }

    protected void SetErrorMessage(List<string> errorMessages, string message)
    {
        errorMessages.Add(message);
    }

    //protected bool ValidateFieldsLength(string[] fieldNames, string[] fields)
    //{
    //    if (fields.Length > fieldNames.Length) return true;
    //    var missingFields = "";
    //    for (int i = fields.Length; i < fieldNames.Length; ++i)
    //        missingFields += $", {fieldNames[i]}";

    //    ErrorMessages.Add(string.Format(Errors.HeaderFieldMissing, missingFields));
    //    return false;
    //}

    protected string? TrimValue(string value, bool removeDescriptionValue = false)
    {
        if (removeDescriptionValue)
            value = value.RemoveDescription();

        return value.TrimValue();
    }
}

