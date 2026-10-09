using System.Text.Json;

namespace HCA.Infrastructure.Extensions;

public static class SerializationExtensions
{
    public static string Serialize(dynamic obj)
        => JsonSerializer.Serialize(obj, GetSerializationOptions(JsonNamingPolicy.CamelCase));

    public static string SerializeWithoutCasing(dynamic obj)
        => JsonSerializer.Serialize(obj, GetSerializationOptions(null));

    public static T? DeSerialize<T>(this string jsonString)
        => JsonSerializer.Deserialize<T>(jsonString, GetSerializationOptions(JsonNamingPolicy.CamelCase));

    public static T? DeSerializeWithoutCasing<T>(this string jsonString)
        => JsonSerializer.Deserialize<T>(jsonString, GetSerializationOptions(null));

    private static JsonSerializerOptions GetSerializationOptions(JsonNamingPolicy? jsonNamingPolicy)
        => new()
        {
            PropertyNamingPolicy = jsonNamingPolicy ,
            WriteIndented = true
        };
}

