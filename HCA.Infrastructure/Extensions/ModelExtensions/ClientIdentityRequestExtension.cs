using System;
using HCA.Infrastructure.Comparer;
using HCA.Models;
using HCA.Models.MuleSoft;
using HCA.Models.Request;

namespace HCA.Infrastructure.Extensions.ModelExtensions;

public static class ClientIdentityRequestExtension
{
    public static IEnumerable<IEnumerable<ClientIdentityRequest>> GroupBySourceNameAndId(this IEnumerable<ClientIdentityRequest> requests)
    {
        var result = new List<List<ClientIdentityRequest>>();
        var groupBysourceNameAndId = requests.GroupBy(r => r, new ClientIdentityRequestSourceComparer());

        foreach (var group in groupBysourceNameAndId)
        {
            var items = group.ToList();
            result.Add(items);
        }

        return result;
    }

    public static (IEnumerable<ClientIdentityRequest>, IEnumerable<ClientIdentityRequest>) GetDuplicateRecords(this IEnumerable<ClientIdentityRequest> requests)
    {
        var records = new List<ClientIdentityRequest>();
        var duplicateRecords = new List<ClientIdentityRequest>();

        var groupBysourceNameAndId = requests.GroupBy(r => r, new ClientIdentityRequestComparer());

        foreach (var group in groupBysourceNameAndId)
        {
            int i = 0;

            foreach (var item in group)
            {
                if (i == 0)
                    records.Add(item);
                else
                    duplicateRecords.Add(item);

                i++;
            }
        }

        return (records, duplicateRecords);
    }

    public static string GetTrackingId(string sourceName, string sourceSystemId)
    {
        return $"{sourceName}-{sourceSystemId}-{DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss")}";
    }

    public static string GetTrackingId(this Source source)
    {
        return GetTrackingId(source.Name, source.Id);
    }

    public static string GetTrackingId(this Source source, Source source2)
    {
        return $"{source.Name}-{source.Id}-{source2.Name}-{source2.Id}-{DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss")}";
    }

    public static string GetTrackingId()
    {
        return DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss");
    }
}

