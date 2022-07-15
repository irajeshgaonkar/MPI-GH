
namespace HCA.MuleSoft.Extensions;

public static class DomainExtensions
{
    public static string GetTrackingId()
    {
        return Guid.NewGuid().ToString();
    }
}

