using System;
using System.Globalization;
using HCA.Infrastructure.Extensions;
using HCA.Models;
using HCA.Models.Request;

namespace HCA.Core.Processors.CsvFileProcessor;

public class ClientIdentityParser : BaseParser<ClientIdentityRequest>
{
    private readonly string[] _fieldNames = new string[] { "MPI Link ID", "Source System ID", "Source System Last Update",
            "First Name", "Middle Name", "Last Name","Suffix", "Birth Date", "Gender", "SSN", "Address Type", "Address Line 1",
            "Address Line 2", "Address Line 3", "City", "State", "Zip Code", "Zip Plus Four", "Phone type", "Phone number",
            "Email type", "Email Address", "Protectec Population Flag", "Protected Population Type" };

    public override (ClientIdentityRequest, string, bool) ParseData(string line)
    {
        var fieldValues = ParseValues(line, _fieldNames);
        var isEmptyLine = IsEmptyLine(fieldValues);

        if (isEmptyLine)
            return (new ClientIdentityRequest(), "", isEmptyLine);

        var (data, errorMessage) = CreateModel(fieldValues);
        return (data, errorMessage, false);
    }

    private (ClientIdentityRequest, string) CreateModel(Dictionary<string, string> values)
    {
        List<string> errorMessages = new List<string>();
        CultureInfo culture = CultureInfo.CreateSpecificCulture("en-US");
        ClientIdentityRequest data = new ClientIdentityRequest();

        var mpiLinkId = values.GetValue(_fieldNames[0]);
        var sourceSystemId = values.GetValue(_fieldNames[1]);
        var sourceSystemLastUpdatedStr = values.GetValue(_fieldNames[2]);
        var firstName = values.GetValue(_fieldNames[3]);
        var middleName = values.GetValue(_fieldNames[4]);
        var lastName = values.GetValue(_fieldNames[5]);
        var suffix = values.GetValue(_fieldNames[6]);
        var dobStr = values.GetValue(_fieldNames[7]);
        var gender = values.GetValue(_fieldNames[8]);
        var ssn = values.GetValue(_fieldNames[9]);
        var addressType = values.GetValue(_fieldNames[10]);
        var line1 = values.GetValue(_fieldNames[11]);
        var line2 = values.GetValue(_fieldNames[12]);
        var line3 = values.GetValue(_fieldNames[13]);
        var city = values.GetValue(_fieldNames[14]);
        var state = values.GetValue(_fieldNames[15]);
        var zipCode = values.GetValue(_fieldNames[16]);
        var zipPlusFour = values.GetValue(_fieldNames[17]);
        var phoneType = values.GetValue(_fieldNames[18]);
        var phoneNumber = values.GetValue(_fieldNames[19]);
        var emailType = values.GetValue(_fieldNames[20]);
        var emailAddress = values.GetValue(_fieldNames[21]);
        var protectecPopulationFlagStr = values.GetValue(_fieldNames[22]);
        var protectecPopulationType = values.GetValue(_fieldNames[23]);


        if (sourceSystemId.IsEmpty())
            SetErrorMessage(errorMessages, "Source System Id value is missing");

        if (sourceSystemLastUpdatedStr.IsEmpty() || !DateTime.TryParse(sourceSystemLastUpdatedStr, culture, DateTimeStyles.None, out DateTime sourceSystemLastUpdated))
            SetErrorMessage(errorMessages, "File Created Date value is missing or invalid");

        if (dobStr.IsEmpty() || !DateOnly.TryParse(dobStr, culture, DateTimeStyles.None, out DateOnly dob))
            SetErrorMessage(errorMessages, "File Created Date value is missing or invalid");

        if (protectecPopulationFlagStr.IsEmpty() || bool.TryParse(protectecPopulationFlagStr, out bool protectecPopulationFlag))
            SetErrorMessage(errorMessages, "Protectec Population Flage value is missing or invalid");

        DateTime.TryParse(sourceSystemLastUpdatedStr, culture, DateTimeStyles.None, out sourceSystemLastUpdated); //?Todo: update later
        _ = bool.TryParse(protectecPopulationFlagStr, out protectecPopulationFlag); //?Todo: update later

        data.MpiLinkId = mpiLinkId;
        data.SourceSystemId = sourceSystemId!;
        data.SourceSystemUpdated = sourceSystemLastUpdated;
        data.FirstName = firstName!;
        data.MiddleName = middleName;
        data.LastName = lastName!;
        data.NameSuffix = suffix;
        data.Dob = dob;
        data.Gender = gender!;
        data.Ssn = ssn!;

        data.AddressType = addressType;
        data.AddressLine1 = line1!;
        data.AddressLine2 = line2;
        data.AddressLine3 = line3;
        data.City = city ?? "";
        data.State = state ?? "";
        data.ZipCode = zipCode ?? "";
        data.ZipFour = zipPlusFour ?? "";

        data.EmailAddress = emailAddress;
        data.PhoneNumber = phoneNumber!;
        data.EmailType = emailType;
        data.PhoneType = phoneType;

        data.ProtectedPopulationFlag = protectecPopulationFlag;
        data.ProtectedPopulationType = protectecPopulationType;

        return (data, GetParsingErrors(errorMessages));
    }
}

