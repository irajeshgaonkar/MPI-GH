using System.Diagnostics;
using System.Text;
using HCA.Infrastructure.Extensions;
using HCA.Infrastructure.Logger;

namespace HCA.Infrastructure.Http;

/// <inheritdoc/>
public class HttpAdapter : IHttpAdapter
{
    private readonly IAppLogger _logger;

    private readonly HttpOptions _httpOptions;

    private readonly HttpClient _httpClient;

    public HttpAdapter(IAppLogger logger, HttpOptions httpOptions)
    {
        _logger = logger;
        _httpOptions = httpOptions;
        _httpClient = GetHttpClient();
    }

    private HttpClient GetHttpClient()
    {
        var client = new HttpClient();
        AddCommonHeaders(client);
        return client;
    }

    private void AddCommonHeaders(HttpClient httpClient)
    {
        foreach (var header in _httpOptions.CommonHeaders)
            httpClient.DefaultRequestHeaders.Add(header.Key, header.Value);
    }

    /// <inheritdoc/>
    public async Task<T?> Delete<T>(string url)
    {
        var uri = GetFullUri(url);
        HttpResponseMessage httpResponseMessage = await _httpClient.DeleteAsync(uri);
        await httpResponseMessage.EnsureSuccess();
        var response = await httpResponseMessage.Deserialize<T>();
        return response;
    }

    /// <inheritdoc/>
    public async Task<T?> Get<T>(string url)
    {
        var uri = GetFullUri(url);
        HttpResponseMessage httpResponseMessage = await _httpClient.GetAsync(uri);
        await httpResponseMessage.EnsureSuccess();
        var response = await httpResponseMessage.Deserialize<T>();
        return response;
    }

    /// <inheritdoc/>
    public async Task<T?> Post<T>(string url, dynamic requestBody)
    {
        _logger.LogInformation($"posting data to {url} started");
        var uri = GetFullUri(url);
        var content = GetHttpContent(requestBody);
        Stopwatch stopWatch = new Stopwatch();
        stopWatch.Start();
        HttpResponseMessage httpResponseMessage = await _httpClient.PostAsync(uri, content);
        stopWatch.Stop();
        _logger.LogInformation($"Completed posting data to {url} with Status Code {httpResponseMessage.StatusCode}, Elapsed time {stopWatch.ElapsedMilliseconds}");
        await httpResponseMessage.EnsureSuccess();
        var response = await httpResponseMessage.Deserialize<T>();
        return response;
    }

    /// <inheritdoc/>
    public async Task<T?> Put<T>(string url, dynamic requestBody)
    {
        var uri = GetFullUri(url);
        var content = GetHttpContent(requestBody);
        HttpResponseMessage httpResponseMessage = await _httpClient.PutAsync(uri, content);
        await httpResponseMessage.EnsureSuccess();
        var response = await httpResponseMessage.Deserialize<T>();
        return response;
    }

    private HttpContent GetHttpContent(dynamic content)
    {
        var jsonString = SerializationExtensions.Serialize(content);
        var httpContent = new StringContent(jsonString, Encoding.UTF8, "application/json");
        return httpContent;
    }   

    private Uri GetFullUri(string uri)
    {
        var fullUrl = new Uri(_httpOptions.BaseUrl);
        return new Uri(fullUrl, uri);
    }
}

