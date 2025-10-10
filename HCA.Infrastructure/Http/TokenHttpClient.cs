using HCA.Infrastructure.Configurations;
using HCA.Infrastructure.Logger;
using HCA.Models.Response;
using Newtonsoft.Json;

namespace HCA.Infrastructure.Http
{
    public class TokenHttpClient(HttpClient httpClient, IAppLogger appLogger, AppSettings appSettings)
    {
        private readonly HttpClient _httpClient = httpClient;
        private readonly IAppLogger _logger = appLogger;
        private readonly AppSettings _appSettings = appSettings;
        private static string _token;
        private static DateTime _tokenExpiration;

        public async Task<string> GetTokenAsync()
        {
            _logger.LogInformation("Attempting to fetch a new token.");

            if (string.IsNullOrEmpty(_token) || DateTime.UtcNow >= _tokenExpiration)
            {
                if (string.IsNullOrEmpty(_token))
                {
                    _logger.LogInformation("Token is null or empty, requesting a new one.");
                }
                else
                {
                    _logger.LogInformation($"Token expired at {_tokenExpiration}, requesting a new one.");
                }

                try
                {
                    var requestData = new Dictionary<string, string>
                    {
                        { "grant_type", _appSettings.MuleSoft.AdOptions.GrantType },
                        { "client_id", _appSettings.MuleSoft.AdOptions.ClientId },
                        { "client_secret", _appSettings.MuleSoft.AdOptions.ClientSecret },
                        { "scope", _appSettings.MuleSoft.AdOptions.Scope }
                    };

                    var requestContent = new FormUrlEncodedContent(requestData);
                    var tokenUrl = $"https://login.microsoftonline.com/{_appSettings.MuleSoft.AdOptions.Tenant}/oauth2/v2.0/token";

                    _logger.LogInformation($"Sending POST request to {tokenUrl}");

                    var response = await _httpClient.PostAsync(tokenUrl, requestContent);

                    _logger.LogInformation($"Received response with status code: {response.StatusCode}");

                    if (!response.IsSuccessStatusCode)
                    {
                        var tokenException = new Exception("Error fetching token from Entra.");
                        _logger.LogError(tokenException);
                        throw tokenException;
                    }

                    var responseContent = await response.Content.ReadAsStringAsync();
                    var tokenResponse = JsonConvert.DeserializeObject<EntraToken>(responseContent) ??
                        throw new InvalidOperationException("Failed to deserialize the token response.");

                    _logger.LogInformation("Token successfully fetched and parsed.");

                    // Store the token and its expiration time, if AccessToken is not null
                    _token = tokenResponse.AccessToken ?? throw new InvalidOperationException("AccessToken is missing in the response.");
                    _tokenExpiration = DateTime.UtcNow.AddSeconds(tokenResponse.ExpiresIn);

                    // Log the expiration time of the new token
                    _logger.LogInformation($"Token will expire at {_tokenExpiration}");

                    return _token;
                }
                catch (Exception ex)
                {
                    // Log any exceptions during the process
                    _logger.LogError(ex, ex.Message);
                    throw;
                }
            }

            // Log the case where the existing token is still valid
            _logger.LogInformation("Using the existing valid token.");

            return _token;
        }
    }
}