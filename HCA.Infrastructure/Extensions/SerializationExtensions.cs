using System.Text.Json;

namespace HCA.Infrastructure.Extensions;

public static class SerializationExtensions
{
    public static string Serialize(dynamic obj)
        => JsonSerializer.Serialize(obj, GetSerializationOptions());

    public static T? DeSerialize<T>(this string jsonString)
        => JsonSerializer.Deserialize<T>(jsonString, GetSerializationOptions());

    private static JsonSerializerOptions GetSerializationOptions()
        => new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };
}

