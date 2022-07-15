using HCA.Models;
using HCA.Models.MuleSoft;
using HCA.Models.Request;
using HCA.MuleSoft.Extensions;
using HCA.MuleSoft.Models;
using HCA.MuleSoft.Models.Request.Link;
using HCA.MuleSoft.Models.Request.Merge;
using HCA.MuleSoft.Models.Request.Post;

namespace HCA.MuleSoft.RequestBuilder;

public class LinkIdentityRequestBuilder : ILinkIdentitiesRequestBuilder
{
    public LinkIdentitiesRequest Build(LinkSourcesRequest request)
    {
        var linkingSources = new LinkingSources(request.LinkToSource, request.Source);
        return new LinkIdentitiesRequest(request.TackingId, linkingSources);
    }
}

public class UnLinkIdentityRequestBuilder : IUnLinkIdentitiesRequestBuilder
{
    public UnLinkIdentitiesRequest Build(UnLinkSourcesRequest request)
    {
        var unLinkingSources = new UnLinkingSources(request.UnlinkFromSource, request.Source);
        return new UnLinkIdentitiesRequest(request.TrackingId, unLinkingSources);
    }
}

public class MergeIdentitiesRequestBuilder : IMergeIdentitiesRequestBuilder
{
    public MergeIdentitiesRequest Build(MergingSourcesRequest request)
    {
        var mergingSources = new MergingSources (request.ToSurviveSource, request.ToRetireSource);
        return new MergeIdentitiesRequest(request.TrackingId, mergingSources);
    }
}

public class UnMergeIdentitiesRequestBuilder : IUnMergeIdentitiesRequestBuilder
{
    public UnLinkIdentitiesRequest Build(UnLinkSourcesRequest request)
    {
        var unLinkingSources = new UnLinkingSources(request.UnlinkFromSource, request.Source);
        return new UnLinkIdentitiesRequest(request.TrackingId, unLinkingSources);
    }

    public UnMergeIdentitiesRequest Build(UnMergingSourcesRequest request)
    {
        var unMergingSources = new UnMergingSources(request.UnmergeFromSource, request.UnmergeSource);
        return new UnMergeIdentitiesRequest(request.TrackingId, unMergingSources);
    }
}

public class PostIdentityRequestBuilder : IPostIdentityRequestBuilder
{
    public PostIdentityRequest Build(IEnumerable<ClientIdentityRequest> request)
    {
        var content = BuildPostIdentityContent(request);
        var trackingId = DomainExtensions.GetTrackingId();
        return new PostIdentityRequest(trackingId, content);
    }

    private PostIdentityRequestContent BuildPostIdentityContent(IEnumerable<ClientIdentityRequest> requests)
    {
        var identity = BuildIdentity(requests);
        return new (identity);
    }

    private Identity BuildIdentity(IEnumerable<ClientIdentityRequest> requests)
    {
        var identity = new Identity();

        foreach(var request in requests)
        {
            identity.Sources.Add(request.GetSource());
            identity.Names.Add(request.GetName());
            identity.Addresses.Add(request.GetAddress());
            identity.Emails.Add(request.GetEmailAddress());
            identity.PhoneNumbers.Add(request.GetPhoneNumber());
            identity.Ssns.Add(request.GetSsns());
            identity.Genders.Add(request.GetGender());
            identity.DatesOfBirth.Add(request.GetDob());
        }

        return identity;
    }
}



