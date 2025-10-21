using HCA.Models.Verato.Request;
using HCA.Models.Verato.Response;

namespace HCA.Verato;

/// <summary>
/// Mule soft repository
/// </summary>
/// <remarks>Includes http calls to Verato</remarks>
public interface IVeratoRepository
{
    /// <summary>
    /// Verato demographic search api
    /// </summary>
    /// <param name="request">Demograph search request <see cref="PostIdentityRequest"/></param>
    /// <returns>demographc serach response <see cref="DemographicSearchResponse"/></returns>
    Task<DemographicSearchResponse> DemographicSearch(PostIdentityRequest request);

    Task<DOH_DemographicSearchResponse> DOH_DemographicSearch(DemographicSearchRequest request);

    /// <summary>
    /// Verato demographic search api
    /// </summary>
    /// <param name="request">Demograph search request <see cref="PostIdentityRequest"/></param>
    /// <returns>demographc serach response <see cref="DemographicQueryResponse"/></returns>
    Task<DemographicQueryResponse> DemographicQuery(PostIdentityRequest request);

    Task<DOH_DemographicQueryResponse> DOH_DemographicQuery(PostIdentityRequest request);

    Task<DOH_DemographicQueryResponse> DOH_EnrichDemographicQuery(PostIdentityRequest request);

    /// <summary>
    /// Verato link identities api call
    /// </summary>
    /// <param name="request">Link identities request <see cref="LinkIdentitiesRequest"/></param>
    /// <returns>Link identities response <see cref="LinkIdentitiesResponse"/></returns>
    Task<LinkIdentitiesResponse> LinkIdentities(LinkIdentitiesRequest request);

    Task<DOH_LinkIdentitiesResponse> DOH_LinkIdentities(LinkIdentitiesRequest request);

    Task<DOH_DeleteIdentityResponse> DOH_DeleteSourceIdentities(DeleteIdentyRequest request);

    /// <summary>
    /// Verato un link identities api call
    /// </summary>
    /// <param name="request">un link identities request <see cref="UnLinkIdentitiesRequest"/></param>
    /// <returns>Un link identities response <see cref="UnLinkIdentitiesResponse"/></returns>
    Task<UnLinkIdentitiesResponse> UnLinkIdentities(UnLinkIdentitiesRequest request);
    Task<DOH_UnLinkIdentitiesResponse> DOH_UnLinkIdentities(UnLinkIdentitiesRequest request);

    /// <summary>
    /// Verato merge identities api call
    /// </summary>
    /// <param name="request">Merge identities request <see cref="MergeIdentitiesRequest"/></param>
    /// <returns>Merge identities response <see cref="MergeIdentitiesResponse"/></returns>
    Task<MergeIdentitiesResponse> MergeIdentities(MergeIdentitiesRequest request);
    Task<DOH_MergeIdentitiesResponse> DOH_MergeIdentities(MergeIdentitiesRequest request);

    /// <summary>
    /// Verato un merge identities api call
    /// </summary>
    /// <param name="request">Un merge identities request <see cref="UnMergeIdentitiesRequest"/></param>
    /// <returns>un Merge identities response <see cref="UnMergeIdentitiesResponse"/></returns>
    Task<UnMergeIdentitiesResponse> UnMergeIdentities(UnMergeIdentitiesRequest request);
    Task<DOH_UnMergeIdentitiesResponse> DOH_UnMergeIdentities(UnMergeIdentitiesRequest request);

    /// <summary>
    /// Verato Post identity api call
    /// </summary>
    /// <param name="request">Post identity request <see cref="PostIdentityRequest"/></param>
    /// <returns>Post identities response <see cref="PostIdentityResponse"/></returns>
    Task<PostIdentityResponse> PostIdentity(PostIdentityRequest request);

    Task<DOH_PostIdentityResponse> DOH_PostIdentity(PostIdentityRequest request);

    Task<DeleteIdentityResponse> DeleteIdentity(DeleteIdentyRequest request);

    Task<NativeIdQueryResponse> NativeIdQuery(NativeIdQueryRequest request);

    Task<SearchNotificationsResponse> SearchNotifications(SearchNotificationsRequest request);

    Task<T> CallVerato<T>(string requestUrl, VeratoRequest? request);

}