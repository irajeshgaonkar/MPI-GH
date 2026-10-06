namespace HCA.Data;

public interface IOnboardedSystemTenantResolver
{
    Task<OnboardedSystemIpLookupResult> ResolveByIpAsync(string incomingIpAddress);
}
