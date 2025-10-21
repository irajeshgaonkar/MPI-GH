using System.Diagnostics;
using System.Text;
using HCA.Infrastructure.Exceptions;
using HCA.Infrastructure.Extensions;
using HCA.Infrastructure.Http;
using HCA.Infrastructure.Logger;
using HCA.Models.Verato.Request;
using HCA.Models.Verato.Response;
using HCA.Verato.Options;

namespace HCA.Verato.Impl;

/// <summary>
/// Represents a repository for interacting with Verato APIs.
/// This class is responsible for making HTTP requests to Verato services and 
/// handling the necessary data transformations or logic for communication with Verato.
/// </summary>
public class VeratoRepository(IAppLogger appLogger, VeratoHttpClient veratoHttpClient) : IVeratoRepository
{
    private readonly IAppLogger _appLogger = appLogger;
    private readonly VeratoHttpClient _veratoHttpClient = veratoHttpClient;

    ///<inheritdoc />
    public async Task<DemographicSearchResponse> DemographicSearch(PostIdentityRequest request)
        => await Execute<DemographicSearchResponse>(VeratoEndpoint.DemographicSearch, request);

    ///<inheritdoc />
    public async Task<DOH_DemographicSearchResponse> DOH_DemographicSearch(DemographicSearchRequest request)
        => await Execute<DOH_DemographicSearchResponse>(VeratoEndpoint.DemographicSearch, request);

    public async Task<DemographicQueryResponse> DemographicQuery(PostIdentityRequest request)
       => await Execute<DemographicQueryResponse>(VeratoEndpoint.DemographicQuery, request);

    public async Task<DOH_DemographicQueryResponse> DOH_DemographicQuery(PostIdentityRequest request)
      => await Execute<DOH_DemographicQueryResponse>(VeratoEndpoint.DemographicQuery, request);

    public async Task<DOH_DemographicQueryResponse> DOH_EnrichDemographicQuery(PostIdentityRequest request)
      => await Execute<DOH_DemographicQueryResponse>(VeratoEndpoint.EnrichDemographicQuery, request);

    ///<inheritdoc />
    public async Task<LinkIdentitiesResponse> LinkIdentities(LinkIdentitiesRequest request)
        => await Execute<LinkIdentitiesResponse>(VeratoEndpoint.LinkIdntities, request);

    public async Task<DOH_LinkIdentitiesResponse> DOH_LinkIdentities(LinkIdentitiesRequest request)
       => await Execute<DOH_LinkIdentitiesResponse>(VeratoEndpoint.LinkIdntities, request);

    ///<inheritdoc />
    public async Task<UnLinkIdentitiesResponse> UnLinkIdentities(UnLinkIdentitiesRequest request)
        => await Execute<UnLinkIdentitiesResponse>(VeratoEndpoint.UnLinkIdentities, request);

    public async Task<DOH_UnLinkIdentitiesResponse> DOH_UnLinkIdentities(UnLinkIdentitiesRequest request)
       => await Execute<DOH_UnLinkIdentitiesResponse>(VeratoEndpoint.UnLinkIdentities, request);

    public async Task<DeleteIdentityResponse> DeleteIdentity(DeleteIdentyRequest request)
        => await Execute<DeleteIdentityResponse>(VeratoEndpoint.DeleteIdentity, request);

    public async Task<DOH_DeleteIdentityResponse> DOH_DeleteSourceIdentities(DeleteIdentyRequest request)
      => await Execute<DOH_DeleteIdentityResponse>(VeratoEndpoint.DeleteIdentity, request);

    ///<inheritdoc />
    public async Task<MergeIdentitiesResponse> MergeIdentities(MergeIdentitiesRequest request)
        => await Execute<MergeIdentitiesResponse>(VeratoEndpoint.MergeIdentities, request);

    public async Task<DOH_MergeIdentitiesResponse> DOH_MergeIdentities(MergeIdentitiesRequest request)
       => await Execute<DOH_MergeIdentitiesResponse>(VeratoEndpoint.MergeIdentities, request);

    ///<inheritdoc />
    public async Task<UnMergeIdentitiesResponse> UnMergeIdentities(UnMergeIdentitiesRequest request)
        => await Execute<UnMergeIdentitiesResponse>(VeratoEndpoint.UnMergeIdentitie, request);

    public async Task<DOH_UnMergeIdentitiesResponse> DOH_UnMergeIdentities(UnMergeIdentitiesRequest request)
       => await Execute<DOH_UnMergeIdentitiesResponse>(VeratoEndpoint.UnMergeIdentitie, request);

    ///<inheritdoc />
    public async Task<PostIdentityResponse> PostIdentity(PostIdentityRequest request)
        => await Execute<PostIdentityResponse>(VeratoEndpoint.PostIdentities, request);

    public async Task<NativeIdQueryResponse> NativeIdQuery( NativeIdQueryRequest request)
        => await Execute<NativeIdQueryResponse>(VeratoEndpoint.NativeIdQuery, request);

    public async Task<SearchNotificationsResponse> SearchNotifications(SearchNotificationsRequest request)
        => await Execute<SearchNotificationsResponse>(VeratoEndpoint.SearchNotifications, request);

    ///<inheritdoc />
    public async Task<DOH_PostIdentityResponse> DOH_PostIdentity(PostIdentityRequest request)
        => await Execute<DOH_PostIdentityResponse>(VeratoEndpoint.PostIdentities, request);

    public async Task<T> CallVerato<T>(string requestUrl, VeratoRequest? request)
       => await Execute<T>(requestUrl, request);

    private async Task<T> Execute<T>(string requestUrl, VeratoRequest? request)
    {
        var sw = new Stopwatch();
        _appLogger.LogInformation($"started processing verato request {request?.TrackingId}");
        var httpRequestMessage = GetHttpRequestMessage(requestUrl, request);
        sw.Start();
        var httpResponse = await _veratoHttpClient.SendAsync(httpRequestMessage);
        sw.Stop();
        _appLogger.LogInformation($"completed processing verato request {request?.TrackingId}, Elapsed Time: {sw.ElapsedMilliseconds}");
        var response = await httpResponse.Deserialize<T>();

        if( null == response ) {
            throw new HcaVeratoException("Error occurred while posting request to Verato");
        }

        return response;
    }

    private static HttpRequestMessage GetHttpRequestMessage(string requestUrl, VeratoRequest? request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var httpRequest = new HttpRequestMessage(HttpMethod.Post, requestUrl);

        var jsonString = SerializationExtensions.Serialize(request);
        httpRequest.Content = new StringContent(jsonString, Encoding.UTF8, ContentType.ApplicationJson);

        return httpRequest;
    }
}
