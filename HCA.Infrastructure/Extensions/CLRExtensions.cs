using System;
using System.Collections.Concurrent;
using System.Globalization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace HCA.Infrastructure.Extensions;

public static class CLRExtensions
{
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

    public static string RemoveDescription(this string value, char descChar = '(')
    {
        int descIndex = -1;
        for (int i = value.Length - 1; i >= 0; i--)
        {
            if (value[i] == descChar)
            {
                descIndex = i;
                break;
            }
        }

        return descIndex == -1 ? value : value[..descIndex];
    }

    public static string RemoveDescriptionAndTrim(this string str)
    {
        var value = RemoveDescription(str);
        return TrimValue(value);
    }

    public static string TrimValue(this string value)
    {
        var retValue = value.Trim();
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

    public static bool IsNotEmpty(this string? str) => !IsEmpty(str);

    public static bool StringEquals(this string s1, string s2, StringComparison stringComparison = StringComparison.OrdinalIgnoreCase)
    {
        return string.Equals(s1, s2, stringComparison);
    }
}

