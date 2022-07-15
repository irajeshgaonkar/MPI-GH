using System;
using System.Collections.Concurrent;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.Json;

namespace HCA.Infrastructure.Extensions;

public static class CLRExtensions
{
    public static string Serialize<T>(this T obj)
    {
        var serializeOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };


        var result = JsonSerializer.Serialize<T>(obj, serializeOptions);
        return result;
    }

    public static Task ParallelForEachAsync<T>(this IEnumerable<T> source, Func<T, Task> funcBody, int maxDop = 4)
    {
        async Task AwaitPartition(IEnumerator<T> partition)
        {
            using (partition)
            {
                while (partition.MoveNext())
                {
                    await funcBody(partition.Current);
                }
            }
        }

        return Task.WhenAll(Partitioner
            .Create(source)
            .GetPartitions(maxDop)
            .AsParallel()
            .Select(p => AwaitPartition(p)));
    }

    public static string JoinBy(this IEnumerable<string?> items, string joinChar)
    {
        return items.Aggregate((s1, s2) => $"{s1}{joinChar}{s2}") ?? "";
    }

    public static string Now()
    {
        return DateTime.Now.ToString("dddd, dd MMMM yyyy HH:mm:ss");
    }

    public static string RemoveDescription(this string headerFieldValue, char descChar = '(')
    {
        int descIndex = -1;

        for (int i = headerFieldValue.Length - 1; i >= 0; i--)
        {
            if (headerFieldValue[i] == descChar)
            {
                descIndex = i;
                break;
            }
        }

        var retValue = descIndex == -1 ? headerFieldValue : headerFieldValue[..descIndex];

        return retValue;
    }

    public static string? TrimValue(this string value)
    {
        var retValue = value.Trim();
        if (retValue.ToUpper() == "NULL")
            return null;

        return retValue;
    }

    public static string[] SplitByChar(this string str, char splitChar = ',')
    {
        var fields = str.Split(splitChar);
        return fields;
    }

    public static bool IsEmpty(this string? str)
    {
        return string.IsNullOrWhiteSpace(str);
    }

    public static string CombineToString(this List<string> lines, char combineChar = '|')
    {
        StringBuilder builder = new StringBuilder();

        foreach (var line in lines)
        {
            builder.Append($"{line} {combineChar}");
        }

        return builder.ToString();
    }

    public static string CombineToString(this Dictionary<int, string> lines, char combineChar = '|')
    {
        StringBuilder builder = new StringBuilder();

        foreach (var line in lines.Values)
        {
            builder.Append($"{line} {combineChar}");
        }

        return builder.ToString();
    }

    public static string? GetValue(this Dictionary<string, string> values, string key)
    {
        return values.ContainsKey(key) ? values[key] : null;
    }

    public static T? DeepClone<T>(this T obj)
    {
        var jsonString = JsonSerializer.Serialize(obj);
        T? result = JsonSerializer.Deserialize<T>(jsonString);
        return result;
    }
}

