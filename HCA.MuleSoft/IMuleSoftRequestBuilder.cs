
using HCA.Models.MuleSoft.Request;
using HCA.Models.Request;

namespace HCA.MuleSoft;

/// <summary>
/// MuleSoft request builder
/// </summary>
public interface IMuleSoftRequestBuilder
{
    /// <summary>
    /// Post identity request builder
    /// </summary>
    /// <param name="request">post identity request <see cref="PostClientIdentityRequest"/></param>
    /// <returns>return Post identity request for mulesoft <see cref="PostIdentityRequest"></returns>
    PostIdentityRequest BuildPostIdentityRequest(PostClientIdentityRequest request);

    PostIdentityRequest BuildDOH_PostIdentityRequest(DOH_PostClientIdentityRequest request);

    /// <summary>
    /// Link identities request builder
    /// </summary>
    /// <param name="request">Link identities request <see cref="LinkClientIdentityRequest"/></param>
    /// <returns>return Link identities request for mulesoft <see cref="LinkIdentitiesRequest"></returns>
    LinkIdentitiesRequest BuildLinkIdentitisRequest(LinkClientIdentityRequest request);


    DeleteIdentyRequest BuildDeleteIdentityRequest(DeleteClientIdentityRequest request);

    /// <summary>
    /// Un Link identities request builder
    /// </summary>
    /// <param name="request">Un link identities request <see cref="UnLinkClientIdentityRequest"/></param>
    /// <returns>return Un link identities request for mulesoft <see cref="UnLinkIdentitiesRequest"></returns>
    UnLinkIdentitiesRequest BuildUnLinkIdentitiesRequest(UnLinkClientIdentityRequest request);

    /// <summary>
    /// Merge identities request builder
    /// </summary>
    /// <param name="request">Merge identities request <see cref="MergeClientIdentityRequest"/></param>
    /// <returns>return Merge identities request for mulesoft <see cref="MergeIdentitiesRequest"></returns>
    MergeIdentitiesRequest BuildMergeIdentitiesRequest(MergeClientIdentityRequest request);

    /// <summary>
    /// Un Merge identities request builder
    /// </summary>
    /// <param name="request">Un merge identities request <see cref="UnMergeClientIdentityRequest"/></param>
    /// <returns>return Un merge identities request for mulesoft <see cref="UnMergeIdentitiesRequest"></returns>
    UnMergeIdentitiesRequest BuildUnMergeIdentitiesRequest(UnMergeClientIdentityRequest request);

    /// <summary>
    /// Demographic search request builder
    /// </summary>
    /// <param name="request">Demographic search request <see cref="DemographicSearchClientIdentityRequest"/></param>
    /// <returns>return post identity request for mulesoft <see cref="PostIdentityRequest"/></returns>
    PostIdentityRequest BuildDemographicSearchRequest(DemographicSearchClientIdentityRequest request);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    DemographicSearchRequest BuildDOH_DemographicSearchRequest(DOH_DemographicSearchClientIdentityRequest request);

    /// <summary>
    /// Demographic query request builder
    /// </summary>
    /// <param name="request">Demographic search request <see cref="DemographicQueryClientIdentityRequest"/></param>
    /// <returns>return post identity request for mulesoft <see cref="PostIdentityRequest"/></returns>
    PostIdentityRequest BuildDemographicQueryRequest(DemographicQueryClientIdentityRequest request);

    /// <summary>
    /// Demographic query request builder
    /// </summary>
    /// <param name="request">Demographic search request <see cref="DemographicQueryClientIdentityRequest"/></param>
    /// <returns>return post identity request for mulesoft <see cref="PostIdentityRequest"/></returns>
    PostIdentityRequest BuildDOH_DemographicQueryRequest(DOH_DemographicQueryClientIdentityRequest request);

    /// <summary>
    /// Enrich Demographic query request builder
    /// </summary>
    /// <param name="request">Demographic search request <see cref="DOH_EnrichDemographicQueryClientIdentityRequest"/></param>
    /// <returns>return post identity request for mulesoft <see cref="PostIdentityRequest"/></returns>
    PostIdentityRequest BuildDOH_EnrichDemographicQueryRequest(DOH_EnrichDemographicQueryClientIdentityRequest request);
}
