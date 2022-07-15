using System;
using System.Text;
using System.Text.Json;
using HCA.Infrastructure.Logger;

namespace HCA.Infrastructure.Http;

public class HttpOptions
{
    public string BaseUrl { get; set; }

    public Dictionary<string, string> CommonHeaders { get; set; }
}

public interface IHttpAdapter
{
    Task<T?> Get<T>(string url);

    Task<T?> Post<T, U>(string url, U requestBody);

    Task<T?> Put<T, U>(string url, U requestBody);

    Task<T?> Delete<T>(string url);
}

public class HttpAdapter : IHttpAdapter
{
    private readonly ILogger _logger;

    private readonly HttpOptions _httpOptions;

    public HttpAdapter(ILogger logger, HttpOptions httpOptions)
    {
        _logger = logger;
        _httpOptions = httpOptions;
    }

    public async Task<T?> Delete<T>(string url)
    {
        using var client = GetHttpClient();
        var uri = GetFullUri(url);
        HttpResponseMessage httpResponseMessage = await client.DeleteAsync(uri);
        await httpResponseMessage.EnsureSuccess();
        var response = await httpResponseMessage.Deserialize<T>();
        return response;
    }

    public async Task<T?> Get<T>(string url)
    {
        using var client = GetHttpClient();
        var uri = GetFullUri(url);
        HttpResponseMessage httpResponseMessage = await client.GetAsync(uri);
        await httpResponseMessage.EnsureSuccess();
        var response = await httpResponseMessage.Deserialize<T>();
        return response;
    }

    public async Task<T?> Post<T, U>(string url, U requestBody)
    {
        LogInfo($"Post: {url} started");
        using var client = GetHttpClient();
        var uri = GetFullUri(url);
        var content = GetHttpContent(requestBody);
        HttpResponseMessage httpResponseMessage = await client.PostAsync(uri, content);
        LogInfo($"Post: {url} completed with Status Code {httpResponseMessage.StatusCode}");
        await httpResponseMessage.EnsureSuccess();
        var response = await httpResponseMessage.Deserialize<T>();
        return response;
    }

    public async Task<T?> Put<T, U>(string url, U requestBody)
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
        foreach(var header in _httpOptions.CommonHeaders)
        {
            httpClient.DefaultRequestHeaders.Add(header.Key, header.Value);
        }
    }

    private HttpContent GetHttpContent<T>(T content)
    {
        var serializeOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };

        string jsonString = JsonSerializer.Serialize(content, serializeOptions);
        var httpContent = new StringContent(jsonString, Encoding.UTF8, "application/json");
        return httpContent;
    }

    private Uri GetFullUri(string uri) {
        var fullUrl = new Uri(_httpOptions.BaseUrl);
        return new Uri(fullUrl, uri);
    }

    private void LogInfo(string message) =>
        _logger.LogInformation($"Http Adapter => {message}");

}

