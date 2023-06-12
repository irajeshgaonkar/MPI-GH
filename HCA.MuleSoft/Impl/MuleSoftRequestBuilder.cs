using HCA.Infrastructure.Extensions;
using HCA.Models.MuleSoft;
using HCA.Models.MuleSoft.Request;
using HCA.Models.Request;
using HCA.MuleSoft.Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace HCA.MuleSoft;

/// <summary>
/// <inheritdoc/>
/// </summary>
public class MuleSoftRequestBuilder : IMuleSoftRequestBuilder
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
    public LinkIdentitiesRequest BuildLinkIdentitisRequest(LinkClientIdentityRequest request)
     => new(request.TrackingId, request.Content);

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

    public PostIdentityRequest BuildDemographicQueryRequest(DemographicQueryClientIdentityRequest request)
    {
        var content = BuildPostIdentityContent(request);
        return new PostIdentityRequest(request.TrackingId, content);
    }

    private PostIdentityRequestContent BuildPostIdentityContent(DemographicSearchClientIdentityRequest request)
    {
        var identity = BuildIdentity(request);
        return new(identity);
    }

    private PostIdentityRequestContent BuildPostIdentityContent(DemographicQueryClientIdentityRequest request)
    {
        var identity = BuildIdentity(request);
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
        var identity = BuildIdentity(clientIdentities);
        var identityJObject = JObject.FromObject(identity);
        JArray jsonArray = new JArray();

        JObject mergedObject = new JObject();

        foreach (var clientIdentity in clientIdentities)
        {
            if (clientIdentity.CustomJson == null)
                continue;

            var jsonObject = JObject.Parse(clientIdentity.CustomJson);

            foreach (JProperty property in jsonObject.Properties())
            {
                string propertyName = property.Name;

                if (mergedObject[propertyName] == null)
                {
                    mergedObject.Add(propertyName, new JArray());
                }

                (mergedObject[propertyName] as JArray).Add(property.Value);
            }
        }

        identityJObject.Merge(mergedObject);
        identityJObject = ConvertPropertyNames(identityJObject);
        var objectData = identityJObject.ToString();
        var result = SerializationExtensions.DeSerialize<dynamic>(objectData);
        return new(result);
    }

    private static JObject ConvertPropertyNames(JObject inputObject)
    {
        JObject convertedObject = new JObject();

        foreach (var property in inputObject.Properties())
        {
            string oldName = property.Name;
            string newName = ConvertPropertyName(oldName);
            JToken value = property.Value;

            if (value.Type == JTokenType.Object)
            {
                value = ConvertPropertyNames((JObject)value); // Recursively convert nested objects
            }
            else if (value.Type == JTokenType.Array)
            {
                var array = new JArray();
                foreach (var item in value)
                {
                    if (item.Type == JTokenType.Object)
                    {
                        array.Add(ConvertPropertyNames((JObject)item)); // Recursively convert objects in array
                    }
                    else
                    {
                        array.Add(item);
                    }
                }
                value = array;
            }

            convertedObject.Add(newName, value);
        }

        return convertedObject;
    }

    private static string ConvertPropertyName(string oldName)
    {
        return oldName.Substring(0, 1).ToLower() + oldName.Substring(1);
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
}