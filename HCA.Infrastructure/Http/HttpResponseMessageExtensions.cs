using System.Net;
using System.Text.Json;

namespace HCA.Infrastructure.Http;

public static class HttpResponseMessageExtensions
{
    public static async Task<T?> Deserialize<T>(this HttpResponseMessage responseMessage)
    {
        var serializeOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };

        var jsonString = await responseMessage.Content.ReadAsStringAsync();
        T? result = JsonSerializer.Deserialize<T>(jsonString, serializeOptions);
        return result;
    }

    public static async Task<bool> EnsureSuccess(this HttpResponseMessage responseMessage)
    {
        var isSuccess = responseMessage.StatusCode == HttpStatusCode.OK;
        if (isSuccess)
            return true;

        var responseMessageStr = await responseMessage.Content.ReadAsStringAsync();
        throw new HCAHttpException(responseMessage.StatusCode.ToString(), responseMessageStr);
    }
}

