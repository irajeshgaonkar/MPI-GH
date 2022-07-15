using System.Diagnostics.CodeAnalysis;
using HCA.Models;
using HCA.Models.Request;

namespace HCA.Infrastructure.Comparer;

public class ClientIdentityRequestAddressComparer : IEqualityComparer<ClientIdentityRequest>
{
    public bool Equals(ClientIdentityRequest? x, ClientIdentityRequest? y)
    {
        if (null == x || null == y)
            return false;

        return x.AddressType == y.AddressType && x.AddressLine1 == y.AddressLine1 && x.AddressLine2 == y.AddressLine2
            && x.AddressLine3 == y.AddressLine3 && x.City == y.City && x.ZipCode == y.ZipCode && x.ZipFour == y.ZipFour
            && x.State == y.State;
    }

    public int GetHashCode([DisallowNull] ClientIdentityRequest obj)
    {
        int hasCode = 0;

        if (null != obj.AddressType)
        {
            hasCode ^= obj.AddressType!.GetHashCode();
        }

        if (null != obj.AddressLine1)
        {
            hasCode ^= obj.AddressLine1!.GetHashCode();
        }

        if (null != obj.AddressLine2)
        {
            hasCode ^= obj.AddressLine2!.GetHashCode();
        }

        if (null != obj.AddressLine3)
        {
            hasCode ^= obj.AddressLine3!.GetHashCode();
        }

        if (null != obj.City)
        {
            hasCode ^= obj.City!.GetHashCode();
        }

        if (null != obj.ZipCode)
        {
            hasCode ^= obj.ZipCode!.GetHashCode();
        }

        if (null != obj.ZipFour)
        {
            hasCode ^= obj.ZipFour!.GetHashCode();
        }

        if (null != obj.State)
        {
            hasCode ^= obj.State!.GetHashCode();
        }

        return hasCode;
    }
}

