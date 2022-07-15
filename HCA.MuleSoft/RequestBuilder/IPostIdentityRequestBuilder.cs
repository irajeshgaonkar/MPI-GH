using HCA.Models;
using HCA.Models.Request;
using HCA.MuleSoft.Models.Request.Link;
using HCA.MuleSoft.Models.Request.Merge;
using HCA.MuleSoft.Models.Request.Post;

namespace HCA.MuleSoft.RequestBuilder;

public interface IPostIdentityRequestBuilder : IRequestBuilder<PostIdentityRequest, IEnumerable<ClientIdentityRequest>>
{

}

public interface ILinkIdentitiesRequestBuilder : IRequestBuilder<LinkIdentitiesRequest, LinkSourcesRequest>
{

}

public interface IUnLinkIdentitiesRequestBuilder : IRequestBuilder<UnLinkIdentitiesRequest, UnLinkSourcesRequest>
{

}

public interface IMergeIdentitiesRequestBuilder : IRequestBuilder<MergeIdentitiesRequest, MergingSourcesRequest>
{

}

public interface IUnMergeIdentitiesRequestBuilder : IRequestBuilder<UnMergeIdentitiesRequest, UnMergingSourcesRequest>
{

}

public interface IDemographicSearchRequestBuilder : IRequestBuilder<PostIdentityRequest, DemographicSearchRequest>
{

}


