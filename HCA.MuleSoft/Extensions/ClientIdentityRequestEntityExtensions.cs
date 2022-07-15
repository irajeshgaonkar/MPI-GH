using HCA.Data.Entities;
using HCA.Models;
using HCA.Models.MuleSoft;
using HCA.Models.Request;
using HCA.MuleSoft.Models;

namespace HCA.MuleSoft.Extensions;

public static class ClientIdentityRequestEntityExtensions
{
    public static Source GetSource(this ClientIdentityRequest request) =>
        new(request.SourceSystemName, request.SourceSystemId);

    public static HCA.Models.MuleSoft.Name GetName(this ClientIdentityRequest request) =>
        new(request.FirstName, request.MiddleName ?? "", request.LastName, request.NameSuffix ?? "");

    public static HCA.Models.MuleSoft.Address GetAddress(this ClientIdentityRequest request)=>
       new(request.AddressLine1, request.GetAddressLine2(), request.City, request.State, request.GetZipCode());

    public static HCA.Models.MuleSoft.PhoneNumber GetPhoneNumber(this ClientIdentityRequest request)=>
         new(request.PhoneNumber ?? "", "", "", "");

    public static string GetEmailAddress(this ClientIdentityRequest request) =>
         request.EmailAddress ?? "";

    public static string GetSsns(this ClientIdentityRequest request) =>
        request.Ssn ?? "";

    public static string GetGender(this ClientIdentityRequest request) =>
        request.Gender ?? "";

    public static string GetDob(this ClientIdentityRequest request) =>
        request.Dob?.ToString() ?? "";

    private static string GetAddressLine2(this ClientIdentityRequest request)
        => (request.AddressLine2 ?? "") + (request.AddressLine3 ?? "");

    private static string GetZipCode(this ClientIdentityRequest request)
        => request.ZipCode + (request.ZipFour ?? "");
}

