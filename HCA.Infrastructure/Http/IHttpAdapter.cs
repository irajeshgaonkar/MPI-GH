
namespace HCA.Infrastructure.Http;

/// <summary>
/// Http adapter for perming calls over network
/// </summary>
public interface IHttpAdapter
{
    /// <summary>
    /// Send request async
    /// </summary>
    /// <param name="request">Http Request message</param>
    /// <returns>Http Response message</returns>
    Task<HttpResponseMessage> SendAsync(HttpRequestMessage request);


    /// <summary>
    /// Get request
    /// </summary>
    /// <typeparam name="T">response from get</typeparam>
    /// <param name="url">get url</param>
    /// <returns>Response from the request</returns>
    Task<T?> Get<T>(string url);

    /// <summary>
    /// Post request
    /// </summary>
    /// <typeparam name="T"><Response from post/typeparam>
    /// <param name="url">Post url</param>
    /// <param name="content">Post request body</param>
    /// <returns>Response from the post request</returns>
    Task<T?> Post<T>(string url, HttpContent content);

    /// <summary>
    /// Put request
    /// </summary>
    /// <typeparam name="T">Response from Put</typeparam>
    /// <param name="url">Put url</param>
    /// <param name="content">Post request body</param>
    /// <returns>Response from the put request</returns>
    Task<T?> Put<T>(string url, HttpContent content);

    /// <summary>
    /// Delete request
    /// </summary>
    /// <typeparam name="T">Response from Delete</typeparam>
    /// <param name="url">Delete url</param>
    /// <returns>Response from the delete request</returns>
    Task<T?> Delete<T>(string url);
}