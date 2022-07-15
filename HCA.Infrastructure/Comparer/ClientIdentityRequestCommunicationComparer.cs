using System.Diagnostics.CodeAnalysis;
using HCA.Models;
using HCA.Models.Request;

namespace HCA.Infrastructure.Comparer;

public class ClientIdentityRequestCommunicationComparer : IEqualityComparer<ClientIdentityRequest>
{
    public bool Equals(ClientIdentityRequest? x, ClientIdentityRequest? y)
    {
        if (null == x || null == y)
            return false;

        return x.PhoneType == y.PhoneType && x.EmailType == y.EmailType && x.PhoneNumber == y.PhoneNumber
            && x.EmailAddress == y.EmailAddress;
    }

    public int GetHashCode([DisallowNull] ClientIdentityRequest obj)
    {
        int hasCode = 0;

        if (null != obj.PhoneType)
        {
            hasCode ^= obj.PhoneType!.GetHashCode();
        }

        if (null != obj.EmailType)
        {
            hasCode ^= obj.EmailType!.GetHashCode();
        }

        if (null != obj.PhoneNumber)
        {
            hasCode ^= obj.PhoneNumber!.GetHashCode();
        }

        if (null != obj.EmailAddress)
        {
            hasCode ^= obj.EmailAddress!.GetHashCode();
        }

        return hasCode;
    }
}

