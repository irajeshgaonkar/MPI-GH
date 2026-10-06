using System.Collections.Concurrent;
using System.Net.Http.Headers;
using HCA.Infrastructure.Configurations;

namespace HCA.Infrastructure.Http;

public class VeratoHttpClientFactory(AppSettings appSettings)
    : IVeratoHttpClientFactory, IDisposable
{
    private readonly AppSettings _appSettings = appSettings;
    private readonly ConcurrentDictionary<string, Lazy<HttpClient>> _clients = new(StringComparer.Ordinal);

    public HttpClient Create(bool useNonCoalitionTenant, bool isEnrich)
    {
        var key = $"{(useNonCoalitionTenant ? "noncoalition" : "coalition")}:{(isEnrich ? "enrich" : "standard")}";
        var client = _clients.GetOrAdd(
            key,
            _ => new Lazy<HttpClient>(
                () => BuildClient(useNonCoalitionTenant, isEnrich),
                LazyThreadSafetyMode.ExecutionAndPublication));

        return client.Value;
    }

    public void Dispose()
    {
        foreach (var client in _clients.Values)
        {
            if (client.IsValueCreated)
            {
                client.Value.Dispose();
            }
        }
        GC.SuppressFinalize(this);
    }

    private HttpClient BuildClient(bool useNonCoalitionTenant, bool isEnrich)
    {
        var configuration = VeratoTenantConfigurationResolver.Resolve(_appSettings, useNonCoalitionTenant, isEnrich);

        var handler = new HttpClientHandler();
        handler.ClientCertificates.Add(configuration.Certificate);
        handler.ClientCertificateOptions = ClientCertificateOption.Manual;

        var client = new HttpClient(handler, disposeHandler: true)
        {
            BaseAddress = new Uri(configuration.BaseUrl),
            Timeout = configuration.Timeout
        };

        client.DefaultRequestHeaders.Authorization = AuthenticationHeaderValue.Parse(configuration.AuthorizationHeader);
        return client;
    }
}
