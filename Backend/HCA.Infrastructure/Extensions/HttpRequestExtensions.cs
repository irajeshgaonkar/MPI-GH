    using System.Text;
using System.Text.Json;

namespace HCA.Infrastructure.Http;

public static class HttpRequestExtensions
{
    public static HttpContent GetHttpContent<T>(T content)
    {
        string jsonString = JsonSerializer.Serialize(content);
        var httpContent = new StringContent(jsonString, Encoding.UTF8, "application/json");
        return httpContent;
    }
}

