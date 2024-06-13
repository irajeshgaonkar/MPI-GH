using System.Diagnostics.CodeAnalysis;
using HCA.Models.Request;

namespace HCA.Infrastructure.Comparer;

public class ClientIdentityRequestDetailsComparer : IEqualityComparer<ClientIdentityRequest>
{
    public bool Equals(ClientIdentityRequest? x, ClientIdentityRequest? y)
    {
        if (null == x || null == y)
            return false;

        return x.FirstName == y.FirstName && x.MiddleName == y.MiddleName && x.LastName == y.LastName
            && x.NameSuffix == y.NameSuffix && x.Ssn == y.Ssn && x.Dob == y.Dob
            && x.Gender == y.Gender && x.ProtectedPopulationFlag == y.ProtectedPopulationFlag
            && x.ProtectedPopulationType == y.ProtectedPopulationType;
    }

    public int GetHashCode([DisallowNull] ClientIdentityRequest obj)
    {
        int hasCode = 0;

        if (null != obj.FirstName)
        {
            hasCode ^= obj.FirstName!.GetHashCode();
        }

        if (null != obj.MiddleName)
        {
            hasCode ^= obj.MiddleName!.GetHashCode();
        }

        if (null != obj.LastName)
        {
            hasCode ^= obj.LastName!.GetHashCode();
        }

        if (null != obj.NameSuffix)
        {
            hasCode ^= obj.NameSuffix!.GetHashCode();
        }

        if (null != obj.Ssn)
        {
            hasCode ^= obj.Ssn!.GetHashCode();
        }

        if (null != obj.Dob)
        {
            hasCode ^= obj.Dob!.GetHashCode();
        }

        if (null != obj.Gender)
        {
            hasCode ^= obj.Gender!.GetHashCode();
        }

        hasCode ^= obj.ProtectedPopulationFlag!.GetHashCode();

        if (null != obj.ProtectedPopulationType)
        {
            hasCode ^= obj.ProtectedPopulationType!.GetHashCode();
        }

        return hasCode;
    }
}

