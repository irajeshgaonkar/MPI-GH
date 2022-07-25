using System.Text;
using HCA.Infrastructure.Extensions;
using HCA.Infrastructure.Logger;

namespace HCA.Infrastructure.Http;

/// <inheritdoc/>
public class HttpAdapter : IHttpAdapter
{
    private readonly IAppLogger _logger;

    private readonly HttpOptions _httpOptions;

    public HttpAdapter(IAppLogger logger, HttpOptions httpOptions)
    {
        _logger = logger;
        _httpOptions = httpOptions;
    }

    /// <inheritdoc/>
    public async Task<T?> Delete<T>(string url)
    {
        using var client = GetHttpClient();
        var uri = GetFullUri(url);
        HttpResponseMessage httpResponseMessage = await client.DeleteAsync(uri);
        await httpResponseMessage.EnsureSuccess();
        var response = await httpResponseMessage.Deserialize<T>();
        return response;
    }

    /// <inheritdoc/>
    public async Task<T?> Get<T>(string url)
    {
        using var client = GetHttpClient();
        var uri = GetFullUri(url);
        HttpResponseMessage httpResponseMessage = await client.GetAsync(uri);
        await httpResponseMessage.EnsureSuccess();
        var response = await httpResponseMessage.Deserialize<T>();
        return response;
    }

    /// <inheritdoc/>
    public async Task<T?> Post<T>(string url, dynamic requestBody)
    {
        _logger.LogInformation($"posting data to {url} started");
        using var client = GetHttpClient();
        var uri = GetFullUri(url);
        var content = GetHttpContent(requestBody);
        HttpResponseMessage httpResponseMessage = await client.PostAsync(uri, content);
        _logger.LogInformation($"Completed posting data to {url} with Status Code {httpResponseMessage.StatusCode}");
        await httpResponseMessage.EnsureSuccess();
        var response = await httpResponseMessage.Deserialize<T>();
        return response;
    }

    /// <inheritdoc/>
    public async Task<T?> Put<T>(string url, dynamic requestBody)
    {
        using var client = GetHttpClient();
        var uri = GetFullUri(url);
        var content = GetHttpContent(requestBody);
        HttpResponseMessage httpResponseMessage = await client.PutAsync(uri, content);
        await httpResponseMessage.EnsureSuccess();
        var response = await httpResponseMessage.Deserialize<T>();
        return response;
    }

    private HttpClient GetHttpClient()
    {
        var handler = new HttpClientHandler();
        var client = new HttpClient(handler);
        AddCommonHeaders(client);
        return client;
    }

    private void AddCommonHeaders(HttpClient httpClient)
    {
        foreach (var header in _httpOptions.CommonHeaders)
            httpClient.DefaultRequestHeaders.Add(header.Key, header.Value);
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

