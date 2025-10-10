using System.Diagnostics;
using System.Net.Http.Headers;
using System.Text;
using HCA.Infrastructure.Exceptions;
using HCA.Infrastructure.Extensions;
using HCA.Infrastructure.Http;
using HCA.Infrastructure.Logger;
using HCA.Models.MuleSoft.Request;
using HCA.Models.MuleSoft.Response;

namespace HCA.MuleSoft;

/// <summary>
/// Represents a repository for interacting with MuleSoft APIs.
/// This class is responsible for making HTTP requests to MuleSoft services and 
/// handling the necessary data transformations or logic for communication with MuleSoft.
/// </summary>
public class MuleSoftRepository(IAppLogger appLogger, MuleSoftHttpClient muleSoftHttpClient, TokenHttpClient tokenHttpClient) : IMuleSoftRepository
{
    private readonly IAppLogger _appLogger = appLogger;
    private readonly MuleSoftHttpClient _muleSoftHttpClient = muleSoftHttpClient;
    private readonly TokenHttpClient _tokenHttpClient = tokenHttpClient;

    ///<inheritdoc />
    public async Task<DemoGraphicSearchResponse> DemographicSearch(PostIdentityRequest request)
        => await Execute<DemoGraphicSearchResponse>(MuleSoftUrls.DemographicSearch, request);

    ///<inheritdoc />
    public async Task<DOH_DemoGraphicSearchResponse> DOH_DemographicSearch(DemographicSearchRequest request)
        => await Execute<DOH_DemoGraphicSearchResponse>(MuleSoftUrls.DemographicSearch, request);

    public async Task<DemoGraphicQueryResponse> DemographicQuery(PostIdentityRequest request)
       => await Execute<DemoGraphicQueryResponse>(MuleSoftUrls.DemographicQuery, request);

    public async Task<DOH_DemoGraphicQueryResponse> DOH_DemographicQuery(PostIdentityRequest request)
      => await Execute<DOH_DemoGraphicQueryResponse>(MuleSoftUrls.DemographicQuery, request);

    public async Task<DOH_DemoGraphicQueryResponse> DOH_EnrichDemographicQuery(PostIdentityRequest request)
      => await Execute<DOH_DemoGraphicQueryResponse>(MuleSoftUrls.EnrichDemographicQuery, request);

    ///<inheritdoc />
    public async Task<LinkIdentitiesResponse> LinkIdentities(LinkIdentitiesRequest request)
        => await Execute<LinkIdentitiesResponse>(MuleSoftUrls.LinkIdntities, request);

    public async Task<DOH_LinkIdentitiesResponse> DOH_LinkIdentities(LinkIdentitiesRequest request)
       => await Execute<DOH_LinkIdentitiesResponse>(MuleSoftUrls.LinkIdntities, request);

    ///<inheritdoc />
    public async Task<UnLinkIdentitiesResponse> UnLinkIdentities(UnLinkIdentitiesRequest request)
        => await Execute<UnLinkIdentitiesResponse>(MuleSoftUrls.UnLinkIdentities, request);

    public async Task<DOH_UnLinkIdentitiesResponse> DOH_UnLinkIdentities(UnLinkIdentitiesRequest request)
       => await Execute<DOH_UnLinkIdentitiesResponse>(MuleSoftUrls.UnLinkIdentities, request);

    public async Task<DeleteIdentityResponse> DeleteIdentity(DeleteIdentyRequest request)
        => await Execute<DeleteIdentityResponse>(MuleSoftUrls.DeleteIdentity, request);

    public async Task<DOH_DeleteIdentityResponse> DOH_DeleteSourceIdentities(DeleteIdentyRequest request)
      => await Execute<DOH_DeleteIdentityResponse>(MuleSoftUrls.DeleteIdentity, request);

    ///<inheritdoc />
    public async Task<MergeIdentitiesResponse> MergeIdentities(MergeIdentitiesRequest request)
        => await Execute<MergeIdentitiesResponse>(MuleSoftUrls.MergeIdentities, request);

    public async Task<DOH_MergeIdentitiesResponse> DOH_MergeIdentities(MergeIdentitiesRequest request)
       => await Execute<DOH_MergeIdentitiesResponse>(MuleSoftUrls.MergeIdentities, request);

    ///<inheritdoc />
    public async Task<UnMergeIdentitiesResponse> UnMergeIdentities(UnMergeIdentitiesRequest request)
        => await Execute<UnMergeIdentitiesResponse>(MuleSoftUrls.UnMergeIdentitie, request);

    public async Task<DOH_UnMergeIdentitiesResponse> DOH_UnMergeIdentities(UnMergeIdentitiesRequest request)
       => await Execute<DOH_UnMergeIdentitiesResponse>(MuleSoftUrls.UnMergeIdentitie, request);

    ///<inheritdoc />
    public async Task<PostIdentityResponse> PostIdentity(PostIdentityRequest request)
        => await Execute<PostIdentityResponse>(MuleSoftUrls.PostIdentities, request);

    public async Task<NativeIdQueryResponse> NativeIdQuery( NativeIdQueryRequest request)
        => await Execute<NativeIdQueryResponse>(MuleSoftUrls.NativeIdQuery, request);

    public async Task<SearchNotificationsResponse> SearchNotifications(SearchNotificationsRequest request)
        => await Execute<SearchNotificationsResponse>(MuleSoftUrls.SearchNotifications, request);

    ///<inheritdoc />
    public async Task<DOH_PostIdentityResponse> DOH_PostIdentity(PostIdentityRequest request)
        => await Execute<DOH_PostIdentityResponse>(MuleSoftUrls.PostIdentities, request);

    public async Task<T> CallMulesoft<T>(string requestUrl, MuleSoftRequest? request)
       => await Execute<T>(requestUrl, request);

    private async Task<T> Execute<T>(string requestUrl, MuleSoftRequest? request)
    {
        var sw = new Stopwatch();
        _appLogger.LogInformation($"started processing mulesoft request {request?.TrackingId}");
        var httpRequestMessage = await GetHttpRequestMessage(requestUrl, request);
        sw.Start();
        var httpResponse = await _muleSoftHttpClient.SendAsync(httpRequestMessage);
        sw.Stop();
        _appLogger.LogInformation($"completed processing mulesoft request {request?.TrackingId}, Elapsed Time: {sw.ElapsedMilliseconds}");
        var response = await httpResponse.Deserialize<T>();

        if( null == response ) {
            throw new HcaMuleSoftException("Error occurred while posting request to MuleSoft");
        }

        return response;
    }

    private async Task<HttpRequestMessage> GetHttpRequestMessage(string requestUrl, MuleSoftRequest? request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var httpRequest = new HttpRequestMessage(HttpMethod.Post, requestUrl);

        var token = await _tokenHttpClient.GetTokenAsync();
        httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var jsonString = SerializationExtensions.Serialize(request);
        httpRequest.Content = new StringContent(jsonString, Encoding.UTF8, ContentType.ApplicationJson);

        return httpRequest;
    }
}
