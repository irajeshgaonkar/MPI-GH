using System.Diagnostics;
using System.Net.Http.Headers;
using HCA.Infrastructure.Logger;

namespace HCA.Infrastructure.Http
{
    public class VeratoHttpClient(IVeratoHttpClientFactory httpClientFactory, IVeratoTenantContext tenantContext, IAppLogger appLogger)
    {
        private readonly IVeratoHttpClientFactory _httpClientFactory = httpClientFactory;
        private readonly IVeratoTenantContext _tenantContext = tenantContext;
        private readonly IAppLogger _logger = appLogger;

        /// <summary>
        /// Send Async requests to Verato
        /// </summary>
        /// <param name="request"></param>
        /// <returns>HttpResponseMessage</returns>
        public async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request)
        {
            ArgumentNullException.ThrowIfNull(request);

            var requestUri = request.RequestUri?.ToString() ?? "unknown URI";
            _logger.LogInformation("Started sending HTTP request to {RequestUri}", requestUri);

            var stopwatch = Stopwatch.StartNew();
            HttpResponseMessage response;

            try
            {
                var client = CreateClient(isEnrich: false);
                response = await client.SendAsync(request);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, $"HTTP request to {requestUri} failed after {stopwatch.ElapsedMilliseconds}ms");
                throw;
            }

            stopwatch.Stop();

            _logger.LogInformation(
                $"Completed sending request to {requestUri} with status code {response.StatusCode}. Elapsed time: {stopwatch.ElapsedMilliseconds}ms");

            try
            {
                await response.EnsureSuccess();
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, $"HTTP request to {requestUri} returned non-success status code {response.StatusCode}");
                throw;
            }

            return response;
        }

        private HttpClient CreateClient(bool isEnrich)
        {
            return _httpClientFactory.Create(_tenantContext.UseNonCoalitionTenant, isEnrich);
        }
    }
}
