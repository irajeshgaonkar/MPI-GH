using System.Diagnostics.CodeAnalysis;
using HCA.Models;
using HCA.Models.Request;

namespace HCA.Infrastructure.Comparer;

public class ClientIdentityRequestComparer : IEqualityComparer<ClientIdentityRequest>
{
    public bool Equals(ClientIdentityRequest? x, ClientIdentityRequest? y)
    {
        if (null == x || null == y)
            return false;

        return new ClientIdentityRequestDetailsComparer().Equals(x, y) &&
            new ClientIdentityRequestAddressComparer().Equals(x, y) &&
            new ClientIdentityRequestCommunicationComparer().Equals(x, y);
    }

    public int GetHashCode([DisallowNull] ClientIdentityRequest obj)
    {
        return obj.SourceSystemName.GetHashCode() ^ obj.SourceSystemId.GetHashCode();
    }
}

