namespace HCA.Data;

public sealed class OnboardedSystemIpLookupResult
{
    public TenantDatabaseKind TenantDatabase { get; init; }
    public IReadOnlyList<string> SourceSystems { get; init; } = [];
}
