using HCA.Models.MuleSoft;
using HCA.Models.Request;

namespace HCA.MuleSoft.Extensions;

public static class ClientIdentityRequestExtensions
{
    public static Source GetSource(this ClientIdentityRequest request)
    {
        return new(request.SourceSystemName, request.SourceSystemId);
    }

    public static Name GetName(this ClientIdentityRequest request)
    {
        return new(request.FirstName, request.MiddleName ?? "", request.LastName, request.NameSuffix ?? "");
    }

    public static Address GetAddress(this ClientIdentityRequest request)
    {
        return new(request.AddressLine1, request.GetAddressLine2(), request.City, request.State, request.GetZipCode(),request.ZipFour ?? "");
    }

    public static PhoneNumber GetPhoneNumber(this ClientIdentityRequest request)
    {
        return new(request.PhoneNumber ?? "", "", "", "");
    }

    public static string GetEmailAddress(this ClientIdentityRequest request)
    {
        return request.EmailAddress ?? "";
    }

    public static string GetSsns(this ClientIdentityRequest request)
    {
        return request.Ssn ?? "";
    }

    public static string GetGender(this ClientIdentityRequest request)
    {
        return request.Gender ?? "";
    }

    public static string GetDob(this ClientIdentityRequest request)
    {
        return request.Dob?.ToString() ?? "";
    }

    private static string GetAddressLine2(this ClientIdentityRequest request)
    { 
        return (request.AddressLine2 ?? "") + (request.AddressLine3 ?? "");
    }

    private static string GetZipCode(this ClientIdentityRequest request)
    { 
        return request.ZipCode + (request.ZipFour ?? "");
    }
}

