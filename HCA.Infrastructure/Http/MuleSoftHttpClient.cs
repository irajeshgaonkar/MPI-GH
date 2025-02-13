using System.Diagnostics;
using HCA.Infrastructure.Logger;

namespace HCA.Infrastructure.Http
{
    public class MuleSoftHttpClient(HttpClient httpClient, IAppLogger appLogger)
    {
        private readonly HttpClient _httpClient = httpClient;
        private readonly IAppLogger _logger = appLogger;

        /// <summary>
        /// Asynchronously sends the provided HTTP request and returns the HTTP response if successful.
        /// </summary>
        /// <param name="request">The HTTP request message to send.</param>
        /// <returns>A task that represents the asynchronous operation, with the result being the HTTP response message.</returns>
        /// <exception cref="ArgumentNullException">Thrown if the provided request is null.</exception>
        /// <exception cref="HttpRequestException">Thrown if the HTTP request fails or does not return a successful status code.</exception>
        public async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request)
        {
            _logger.LogInformation($"Started sending HTTP request to {request.RequestUri}");

            ArgumentNullException.ThrowIfNull(request);

            Stopwatch stopWatch = new();
            stopWatch.Start();

            var response = await _httpClient.SendAsync(request);
            stopWatch.Stop();

            _logger.LogInformation($"Completed sending request to {request.RequestUri} with Status Code {response.StatusCode}, Elapsed time {stopWatch.ElapsedMilliseconds}ms");

            await response.EnsureSuccess();

            return response;
        }

        /// <summary>
        /// Asynchronously sends a DELETE request to the specified URL and returns the deserialized response.
        /// </summary>
        /// <typeparam name="T">The type of the response to deserialize into.</typeparam>
        /// <param name="url">The URL to send the DELETE request to.</param>
        /// <returns>A task that represents the asynchronous operation, with a result of type T, or null if no content is returned.</returns>
        /// <exception cref="HttpRequestException">Thrown if the HTTP request fails or does not return a successful status code.</exception>
        /// <exception cref="JsonSerializationException">Thrown if the response body cannot be deserialized into the specified type T.</exception>
        public async Task<T?> Delete<T>(string url)
        {
            _logger.LogInformation($"Started DELETE request to {url}");

            Stopwatch stopWatch = new();
            stopWatch.Start();

            HttpResponseMessage httpResponseMessage = await _httpClient.DeleteAsync(url);
            stopWatch.Stop();

            _logger.LogInformation($"Completed DELETE request to {url} with Status Code {httpResponseMessage.StatusCode}, Elapsed time {stopWatch.ElapsedMilliseconds}ms");

            await httpResponseMessage.EnsureSuccess();

            return await httpResponseMessage.Deserialize<T>();
        }

        /// <summary>
        /// Asynchronously sends a GET request to the specified URL and returns the deserialized response content.
        /// </summary>
        /// <typeparam name="T">The type to deserialize the response body into.</typeparam>
        /// <param name="url">The URL to send the GET request to.</param>
        /// <returns>A task that represents the asynchronous operation, with a result of type T, or null if no content is returned.</returns>
        /// <exception cref="HttpRequestException">Thrown if the HTTP request fails or does not return a successful status code.</exception>
        /// <exception cref="JsonSerializationException">Thrown if the response body cannot be deserialized into the specified type T.</exception>
        public async Task<T?> Get<T>(string url)
        {
            _logger.LogInformation($"Started GET request to {url}");

            Stopwatch stopWatch = new();
            stopWatch.Start();

            HttpResponseMessage httpResponseMessage = await _httpClient.GetAsync(url);
            stopWatch.Stop();

            _logger.LogInformation($"Completed GET request to {url} with Status Code {httpResponseMessage.StatusCode}, Elapsed time {stopWatch.ElapsedMilliseconds}ms");

            await httpResponseMessage.EnsureSuccess();

            return await httpResponseMessage.Deserialize<T>();
        }

        /// <summary>
        /// Asynchronously sends a POST request with the specified content to the given URL and returns the deserialized response.
        /// </summary>
        /// <typeparam name="T">The type to deserialize the response body into.</typeparam>
        /// <param name="url">The URL to send the POST request to.</param>
        /// <param name="content">The content to send with the POST request.</param>
        /// <returns>A task that represents the asynchronous operation, with a result of type T, or null if no content is returned.</returns>
        /// <exception cref="HttpRequestException">Thrown if the HTTP request fails or does not return a successful status code.</exception>
        /// <exception cref="JsonSerializationException">Thrown if the response body cannot be deserialized into the specified type T.</exception>
        public async Task<T?> Post<T>(string url, HttpContent content)
        {
            _logger.LogInformation($"Started posting data to {url}");

            Stopwatch stopWatch = new();
            stopWatch.Start();

            HttpResponseMessage httpResponseMessage = await _httpClient.PostAsync(url, content);
            stopWatch.Stop();

            _logger.LogInformation($"Completed posting data to {url} with Status Code {httpResponseMessage.StatusCode}, Elapsed time {stopWatch.ElapsedMilliseconds}ms");

            // Ensure the response was successful
            await httpResponseMessage.EnsureSuccess();

            return await httpResponseMessage.Deserialize<T>();
        }

        /// <summary>
        /// Asynchronously sends a PUT request with the specified content to the given URL and returns the deserialized response.
        /// </summary>
        /// <typeparam name="T">The type to deserialize the response body into.</typeparam>
        /// <param name="url">The URL to send the PUT request to.</param>
        /// <param name="content">The content to send with the PUT request.</param>
        /// <returns>A task that represents the asynchronous operation, with a result of type T, or null if no content is returned.</returns>
        /// <exception cref="HttpRequestException">Thrown if the HTTP request fails or does not return a successful status code.</exception>
        /// <exception cref="JsonSerializationException">Thrown if the response body cannot be deserialized into the specified type T.</exception>
        public async Task<T?> Put<T>(string url, HttpContent content)
        {
            _logger.LogInformation($"Started sending PUT data to {url}");

            Stopwatch stopWatch = new();
            stopWatch.Start();

            HttpResponseMessage httpResponseMessage = await _httpClient.PutAsync(url, content);
            stopWatch.Stop();

            _logger.LogInformation($"Completed PUT data to {url} with Status Code {httpResponseMessage.StatusCode}, Elapsed time {stopWatch.ElapsedMilliseconds}ms");

            await httpResponseMessage.EnsureSuccess();

            return await httpResponseMessage.Deserialize<T>();
        }
    }
}