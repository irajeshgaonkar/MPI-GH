using HCA.Infrastructure.Extensions;
using HCA.Models.Verato;
using HCA.Models.Verato.Request;
using HCA.Models.Request;
using Newtonsoft.Json.Linq;
using HCA.Infrastructure.JObjectHelper;
using HCA.Verato.Extensions;
using System.Text.Json;

namespace HCA.Verato;

/// <summary>
/// <inheritdoc/>
/// </summary>
public class VeratoRequestBuilder : IVeratoRequestBuilder
{
    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public PostIdentityRequest BuildPostIdentityRequest(PostClientIdentityRequest request)
    {
        var content = BuildPostIdentityContent(request.Content);
        return new(request.TrackingId, content);
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public PostIdentityRequest BuildDOH_PostIdentityRequest(DOH_PostClientIdentityRequest request)
    {
        PostIdentityRequestContent postIdentityRequestContent = new PostIdentityRequestContent(NormalizeDynamicIdentityAddresses(request.Content.Identity));
        postIdentityRequestContent.ResponseIdentityFormatNames = request.Content.ResponseIdentityFormatNames;

        return new(request.TrackingId, postIdentityRequestContent);
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public LinkIdentitiesRequest BuildLinkIdentitisRequest(LinkClientIdentityRequest request)
     => new(request.TrackingId, request.Content);

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public DeleteIdentyRequest BuildDeleteIdentityRequest(DeleteClientIdentityRequest request)
     => new(request.TrackingId, new DeleteIdentyRequestContent(request.Content));

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public UnLinkIdentitiesRequest BuildUnLinkIdentitiesRequest(UnLinkClientIdentityRequest request)
        => new(request.TrackingId, request.Content);

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public MergeIdentitiesRequest BuildMergeIdentitiesRequest(MergeClientIdentityRequest request)
        => new(request.TrackingId, request.Content);

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public UnMergeIdentitiesRequest BuildUnMergeIdentitiesRequest(UnMergeClientIdentityRequest request)
        => new(request.TrackingId, request.Content);

    public PostIdentityRequest BuildDemographicSearchRequest(DemographicSearchClientIdentityRequest request)
    {
        var content = BuildPostIdentityContent(request);
        return new PostIdentityRequest(request.TrackingId, content);
    }

    public DemographicSearchRequest BuildDOH_DemographicSearchRequest(DOH_DemographicSearchClientIdentityRequest request)
    {
        DemographicSearchRequest demographicsSearchRequestContent = new DemographicSearchRequest(request.TrackingId, request.Content);
        demographicsSearchRequestContent.Content.responseIdentityFormatNames = request.Content.responseIdentityFormatNames;
        demographicsSearchRequestContent.Content.matchScoreThreshold = request.Content.matchScoreThreshold;
        demographicsSearchRequestContent.Content.maxSearchResults = request.Content.maxSearchResults;
        demographicsSearchRequestContent.Content.identity = NormalizeDynamicIdentityAddresses(request.Content.identity);
        return demographicsSearchRequestContent;
    }

    public PostIdentityRequest BuildDemographicQueryRequest(DemographicQueryClientIdentityRequest request)
    {
        var content = BuildPostIdentityContent(request);
        return new PostIdentityRequest(request.TrackingId, content);
    }

    public PostIdentityRequest BuildDOH_DemographicQueryRequest(DOH_DemographicQueryClientIdentityRequest request)
    {
        PostIdentityRequestContent postIdentityRequestContent = new PostIdentityRequestContent(NormalizeDynamicIdentityAddresses(request.Content.identity));
        postIdentityRequestContent.ResponseIdentityFormatNames = request.Content.responseIdentityFormatNames;
        return new PostIdentityRequest(request.TrackingId, postIdentityRequestContent);
    }

    public NativeIdQueryRequest Build_NativeIdQueryRequest(NativeIdQueryClientIdentityRequest request)
    {
        NativeIdRequestContent nativeIdQueryContent = new()
        {
            ResponseIdentityFormatNames = request.Content.ResponseIdentityFormatNames,
            Source = new Models.Verato.Request.NativeIdRequestSource() { Id = request.Content.Source.Id, Name = request.Content.Source.Name }
        };

        return new NativeIdQueryRequest(request.TrackingId, nativeIdQueryContent);
    }

    public SearchNotificationsRequest Build_SearchNotificationsRequest(SearchClientIdentityNotificationsRequest request)
    {
        SearchNotificationsRequestContent searchNotificationsRequestContent = new()
        {
            PageNumber = request.Content.PageNumber,
            PageSize = request.Content.PageSize,
            StartDate = request.Content.StartDate,
            EndDate = request.Content.EndDate
        };

        return new SearchNotificationsRequest(request.TrackingId, searchNotificationsRequestContent);
    }

    public PostIdentityRequest BuildDOH_EnrichDemographicQueryRequest(DOH_EnrichDemographicQueryClientIdentityRequest request)
    {
        PostIdentityRequestContent postIdentityRequestContent = new PostIdentityRequestContent(NormalizeDynamicIdentityAddresses(request.Content.Identity));
        postIdentityRequestContent.ResponseIdentityFormatNames = request.Content.ResponseIdentityFormatNames;
        return new PostIdentityRequest(request.TrackingId, postIdentityRequestContent);
    }

    private PostIdentityRequestContent BuildPostIdentityContent(DemographicSearchClientIdentityRequest request)
    {
        var identity = NormalizeIdentityAddresses(BuildIdentity(request));
        return new(identity);
    }

    private PostIdentityRequestContent BuildPostIdentityContent(DemographicQueryClientIdentityRequest request)
    {
        var identity = NormalizeIdentityAddresses(BuildIdentity(request));
        return new(identity);
    }

    private Identity BuildIdentity(DemographicSearchClientIdentityRequest demographicSearchRequest)
    {
        var request = demographicSearchRequest.Content;
        return request;
    }

    private Identity BuildIdentity(DemographicQueryClientIdentityRequest demographicSearchRequest)
    {
        var request = demographicSearchRequest.Content;
        return request;
    }

    private PostIdentityRequestContent BuildPostIdentityContent(IEnumerable<ClientIdentityRequest> clientIdentities)
    {
        var clientIdentityList = clientIdentities.ToList();
        var identityJObject = JObject.FromObject(BuildIdentity(clientIdentityList));
        identityJObject = NormalizeIdentityAddresses(identityJObject);

        var mergedObject = VeratoHelper.MergedObjects(clientIdentityList);

        identityJObject.Merge(mergedObject);
        identityJObject = VeratoHelper.ConvertPropertyNames(identityJObject);
        var objectData = identityJObject.ToString();
        var result = SerializationExtensions.DeSerialize<dynamic>(objectData);
        return new(result);
    }

    private Identity BuildIdentity(IEnumerable<ClientIdentityRequest> clientIdentities)
    {
        var identity = new Identity();

        if (clientIdentities.Count() == 0) throw new ArgumentException("Client identities are required to post the data");
        identity.Sources.Add(clientIdentities.First().GetSource());
        foreach (var clientIdentity in clientIdentities)
        {
            identity.Names.Add(clientIdentity.GetName());
            identity.Addresses.Add(clientIdentity.GetAddress());
            identity.Emails.Add(clientIdentity.GetEmailAddress());
            identity.PhoneNumbers.Add(clientIdentity.GetPhoneNumber());
            identity.Ssns.Add(clientIdentity.GetSsns());
            identity.Genders.Add(clientIdentity.GetGender());
            identity.DatesOfBirth.Add(clientIdentity.GetDob());
        }

        return identity;
    }

    private static Identity NormalizeIdentityAddresses(Identity identity)
    {
        foreach (var address in identity.Addresses)
        {
            address.PostalCode = CombinePostalCode(address.PostalCode, address.ZipFour);
            address.ZipFour = string.Empty;
        }

        return identity;
    }

    private static dynamic NormalizeDynamicIdentityAddresses(dynamic identity)
    {
        var identityJObject = ToJObject(identity);
        identityJObject = NormalizeIdentityAddresses(identityJObject);
        return SerializationExtensions.DeSerialize<dynamic>(identityJObject.ToString());
    }

    private static JObject ToJObject(dynamic payload)
    {
        if( payload is JObject jObject )
        {
            return jObject;
        }

        if( payload is JsonElement jsonElement )
        {
            return JObject.Parse(jsonElement.GetRawText());
        }

        if( payload is string jsonString )
        {
            return JObject.Parse(jsonString);
        }

        return JObject.FromObject(payload);
    }

    private static JObject NormalizeIdentityAddresses(JObject identityJObject)
    {
        var addressesProperty = FindPropertyCaseInsensitive(identityJObject, "addresses");
        var addresses = addressesProperty?.Value as JArray;
        if( addresses == null )
        {
            return identityJObject;
        }

        foreach (var addressToken in addresses)
        {
            if( addressToken is not JObject address )
            {
                continue;
            }

            var postalCodeProperty = FindPropertyCaseInsensitive(address, "postalCode");
            var postalCode = postalCodeProperty?.Value?.Value<string>() ?? string.Empty;
            var zipFourProperty = FindPropertyCaseInsensitive(address, "zipFour");
            var zipFour = zipFourProperty?.Value?.Value<string>();

            if (postalCodeProperty != null)
            {
                postalCodeProperty.Value = CombinePostalCode(postalCode, zipFour);
            }

            RemovePropertyCaseInsensitive(address, "zipFour");
        }

        return identityJObject;
    }

    private static JProperty? FindPropertyCaseInsensitive(JObject jObject, string propertyName)
        => jObject.Properties().FirstOrDefault(p => string.Equals(p.Name, propertyName, StringComparison.OrdinalIgnoreCase));

    private static void RemovePropertyCaseInsensitive(JObject jObject, string propertyName)
    {
        var property = FindPropertyCaseInsensitive(jObject, propertyName);
        property?.Remove();
    }

    private static string CombinePostalCode(string zipCode, string? zipFour)
    {
        if( string.IsNullOrWhiteSpace( zipCode ) )
        {
            return zipCode;
        }

        if( string.IsNullOrWhiteSpace( zipFour ) )
        {
            return zipCode;
        }

        if( PostalCodeAlreadyContainsZipFour( zipCode, zipFour ) )
        {
            return zipCode;
        }

        return $"{zipCode}-{zipFour}";
    }

    private static bool PostalCodeAlreadyContainsZipFour(string zipCode, string zipFour)
    {
        if (string.IsNullOrWhiteSpace(zipCode))
            return false;

        var normalizedZipCode = zipCode.Replace("-", "");
        var normalizedZipFour = zipFour.Replace("-", "");

        if (normalizedZipCode.Length <= normalizedZipFour.Length)
            return false;

        return normalizedZipCode.EndsWith(normalizedZipFour, StringComparison.Ordinal);
    }

}
