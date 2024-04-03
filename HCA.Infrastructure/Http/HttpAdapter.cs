using System.Diagnostics;
using HCA.Infrastructure.Logger;

namespace HCA.Infrastructure.Http;

/// <inheritdoc/>
public class HttpAdapter : IHttpAdapter
{
    private readonly IAppLogger _logger;

    private readonly HttpClient _httpClient;

    public HttpAdapter(IAppLogger logger, IDictionary<string, string>? commonHeaders = null)
    {
        _logger = logger;
        _httpClient = GetHttpClient();
        AddCommonHeaders(_httpClient, commonHeaders);
    }

    public async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request)
    {
        if (request == null) throw new ArgumentNullException("SendAsync: Request cannot be null");
        var response = await _httpClient.SendAsync(request);
        await response.EnsureSuccess();
        return response;
    }

    /// <inheritdoc/>
    public async Task<T?> Delete<T>(string url)
    {
        HttpResponseMessage httpResponseMessage = await _httpClient.DeleteAsync(url);
        await httpResponseMessage.EnsureSuccess();
        var response = await httpResponseMessage.Deserialize<T>();
        return response;
    }

    /// <inheritdoc/>
    public async Task<T?> Get<T>(string url)
    {
        HttpResponseMessage httpResponseMessage = await _httpClient.GetAsync(url);
        await httpResponseMessage.EnsureSuccess();
        var response = await httpResponseMessage.Deserialize<T>();
        return response;
    }

    /// <inheritdoc/>
    public async Task<T?> Post<T>(string url, HttpContent content)
    {
        _logger.LogInformation($"posting data to {url} started");
        Stopwatch stopWatch = new Stopwatch();
        stopWatch.Start();
        HttpResponseMessage httpResponseMessage = await _httpClient.PostAsync(url, content);
        stopWatch.Stop();
        _logger.LogInformation($"Completed posting data to {url} with Status Code {httpResponseMessage.StatusCode}, Elapsed time {stopWatch.ElapsedMilliseconds}");
        await httpResponseMessage.EnsureSuccess();
        var response = await httpResponseMessage.Deserialize<T>();
        return response;
    }

    /// <inheritdoc/>
    public async Task<T?> Put<T>(string url, HttpContent content)
    {
        HttpResponseMessage httpResponseMessage = await _httpClient.PutAsync(url, content);
        await httpResponseMessage.EnsureSuccess();
        var response = await httpResponseMessage.Deserialize<T>();
        return response;
    }

    private HttpClient GetHttpClient()
    {
        var client = new HttpClient();
        AddCommonHeaders(client);
        return client;
    }

    private void AddCommonHeaders(HttpClient httpClient, IDictionary<string, string>? commonHeaders = null)
    {
        if (commonHeaders == null) return;

        foreach (var header in commonHeaders)
            httpClient.DefaultRequestHeaders.Add(header.Key, header.Value);
    }

}

