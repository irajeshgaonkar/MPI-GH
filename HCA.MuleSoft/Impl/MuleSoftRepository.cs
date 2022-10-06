using HCA.Infrastructure.Exceptions;
using HCA.Infrastructure.Extensions;
using HCA.Infrastructure.Http;
using HCA.Infrastructure.Logger;
using HCA.Models.MuleSoft.Request;
using HCA.Models.MuleSoft.Response;
using System.Diagnostics;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json.Serialization;

namespace HCA.MuleSoft;

public class AzureAdToken
{
    [JsonPropertyName("token_type")]
    public string TokenType { get; set; }

    [JsonPropertyName("expires_in")]
    public int ExpiresIn { get; set; }

    [JsonPropertyName("ext_expires_in")]
    public int ExtensionExpiresIN { get; set; }

    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; }

    public DateTime ExpirationDateTime { get; set; }
}

///<inheritdoc />
public class MuleSoftRepository : IMuleSoftRepository
{
    private readonly IHttpAdapter _httpAdapter;

    private readonly IAppLogger _appLogger;

    private readonly MuleSoftOptions _muleSoftOptions;

    private AzureAdToken _token;

    private bool IsAuthenticated => null != _token && DateTime.Now < _token.ExpirationDateTime;

    private string TokenUrl => $"https://login.microsoftonline.com/{_muleSoftOptions.AdOptions.Tenant}/oauth2/v2.0/token";

    /// <summary>
    /// <see cref="MuleSoftRepository"/>
    /// </summary>
    /// <param name="httpAdapter">Http adapter for doing http(s) calls</param>
    public MuleSoftRepository(IHttpAdapter httpAdapter, IAppLogger appLogger, MuleSoftOptions muleSoftOptions)
    {
        _httpAdapter = httpAdapter;
        _appLogger = appLogger;
        _muleSoftOptions = muleSoftOptions;
    }

    ///<inheritdoc />
    public async Task<DemoGraphicSearchResponse> DemographicSearch(PostIdentityRequest request)
        => await Execute<DemoGraphicSearchResponse>(MuleSoftUrls.DemographicSearch, request);

    ///<inheritdoc />
    public async Task<LinkIdentitiesResponse> LinkIdentities(LinkIdentitiesRequest request)
        => await Execute<LinkIdentitiesResponse>(MuleSoftUrls.LinkIdntities, request);

    ///<inheritdoc />
    public async Task<UnLinkIdentitiesResponse> UnLinkIdentities(UnLinkIdentitiesRequest request)
        => await Execute<UnLinkIdentitiesResponse>(MuleSoftUrls.UnLinkIdentities, request);

    ///<inheritdoc />
    public async Task<MergeIdentitiesResponse> MergeIdentities(MergeIdentitiesRequest request)
        => await Execute<MergeIdentitiesResponse>(MuleSoftUrls.MergeIdentities, request);

    ///<inheritdoc />
    public async Task<UnMergeIdentitiesResponse> UnMergeIdentities(UnMergeIdentitiesRequest request)
        => await Execute<UnMergeIdentitiesResponse>(MuleSoftUrls.UnMergeIdentitie, request);

    ///<inheritdoc />
    public async Task<PostIdentityResponse> PostIdentity(PostIdentityRequest request)
        => await Execute<PostIdentityResponse>(MuleSoftUrls.PostIdentities, request);

    private async Task<T> Execute<T>(string requestUrl, MuleSoftRequest request)
    {
        var sw = new Stopwatch();
        _appLogger.LogInformation($"started processing mulesoft request {request.TrackingId}");
        var httpRequestMessage = await GetHttpRequestMessage(requestUrl, request);
        sw.Start();
        var httpesponse = await _httpAdapter.SendAsync(httpRequestMessage);
        sw.Stop();
        _appLogger.LogInformation($"completed processing mulesoft request {request.TrackingId}, Elapsed Time: {sw.ElapsedMilliseconds}");
        var response = await httpesponse.Deserialize<T>();

        if (null == response)
            throw new HcaMuleSoftException("Error occured while posting request to MuleSoft");

        return response;
    }

    private async Task Authenticate()
    {
        _appLogger.LogInformation($"started generating the token for mulesoft");

        for (int i = 0; i < 3; ++i)
        {
            try
            {
                _appLogger.LogInformation($"started generating the token for mulesoft for iteration {i}");
                var httpRequest = new HttpRequestMessage(HttpMethod.Post, TokenUrl);
                var requestBody = new Dictionary<string, string>()
                {
                    { "grant_type", _muleSoftOptions.AdOptions.GrantType },
                    { "client_id", _muleSoftOptions.AdOptions.ClientId },
                    { "client_secret", _muleSoftOptions.AdOptions.ClientSecret },
                    { "scope", _muleSoftOptions.AdOptions.Scope }
                };

                httpRequest.Content = new FormUrlEncodedContent(requestBody);
                var httpResponse = await _httpAdapter.SendAsync(httpRequest);
                var responseString = await httpResponse.Content.ReadAsStringAsync();
                var token = responseString.DeSerialize<AzureAdToken>();

                if (null != token)
                {
                    _token = token;
                    _token.ExpirationDateTime = DateTime.Now.AddSeconds(_token.ExpiresIn - 60);
                    return;
                }
            }
            catch (HcaHttpException e)
            {
                _appLogger.LogInformation($"Retrying for the exception HcaHttpException {e.StatusCode}");
                _appLogger.LogError(e);
            }
        }
    }

    private async Task<HttpRequestMessage> GetHttpRequestMessage(string requestUrl, MuleSoftRequest request)
    {
        if (null == request) throw new ArgumentNullException("GetHttpContent request is null");
        if (!IsAuthenticated) await Authenticate();

        var httpRequest = new HttpRequestMessage(HttpMethod.Post, GetFullUri(requestUrl));
        httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _token.AccessToken);
        var jsonString = SerializationExtensions.Serialize(request);
        httpRequest.Content = new StringContent(jsonString, Encoding.UTF8, ContentType.ApplicationJson);
        return httpRequest;
    }

    private Uri GetFullUri(string uri)
    {
        var fullUrl = new Uri(_muleSoftOptions.BaseUrl);
        return new Uri(fullUrl, uri);
    }
}
