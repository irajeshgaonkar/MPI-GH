using System.Net;
using System.Text.Json;
using HCA.Infrastructure.Extensions;

namespace HCA.Infrastructure.Http;

public static class HttpResponseMessageExtensions
{
    public static async Task<T?> Deserialize<T>(this HttpResponseMessage responseMessage)
    {
        var jsonString = await responseMessage.Content.ReadAsStringAsync();
        T? result = jsonString.DeSerialize<T>();
        return result;
    }

    public static async Task<bool> EnsureSuccess(this HttpResponseMessage responseMessage)
    {
        var isSuccess = responseMessage.StatusCode == HttpStatusCode.OK;
        if (isSuccess)
            return true;

        var responseMessageStr = await responseMessage.Content.ReadAsStringAsync();

        // TODO: This error is not getting logged properly;
        // V - I've adjusted the logging but the final error is still not getting reported correctly
        throw new HcaHttpException(responseMessage.StatusCode, responseMessageStr);
    }
}

