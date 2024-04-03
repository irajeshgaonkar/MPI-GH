using System.Diagnostics.CodeAnalysis;
using HCA.Models.Request;

namespace HCA.Infrastructure.Comparer;

public class ClientIdentityRequestSourceComparer : IEqualityComparer<ClientIdentityRequest>
{
    public bool Equals(ClientIdentityRequest? x, ClientIdentityRequest? y)
    {
        if (null == x || null == y)
            return false;

        return x.SourceSystemName == y.SourceSystemName && x.SourceSystemId == y.SourceSystemId;
    }

    public int GetHashCode([DisallowNull] ClientIdentityRequest obj)
    {
        return obj.SourceSystemName.GetHashCode() ^ obj.SourceSystemId.GetHashCode();
    }
}