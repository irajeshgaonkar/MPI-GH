using HCA.Models.MuleSoft;
using HCA.Models.MuleSoft.Request;
using HCA.Models.Request;
using HCA.MuleSoft.Extensions;

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
        return new (request.TrackingId, content);
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public LinkIdentitiesRequest BuildLinkIdentitisRequest(LinkClientIdentityRequest request)
     => new (request.TrackingId, request.Content);

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public UnLinkIdentitiesRequest BuildUnLinkIdentitiesRequest(UnLinkClientIdentityRequest request)
        => new (request.TrackingId, request.Content);

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public MergeIdentitiesRequest BuildMergeIdentitiesRequest(MergeClientIdentityRequest request)
        => new (request.TrackingId, request.Content);

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

    private PostIdentityRequestContent BuildPostIdentityContent(DemographicSearchClientIdentityRequest request)
    {
        var identity = BuildIdentity(request);
        return new(identity);
    }

    private Identity BuildIdentity(DemographicSearchClientIdentityRequest demographicSearchRequest)
    {
        var request = demographicSearchRequest.Content;
        return request;
    }

    private PostIdentityRequestContent BuildPostIdentityContent(IEnumerable<ClientIdentityRequest> clientIdentities)
    {
        var identity = BuildIdentity(clientIdentities);
        return new(identity);
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