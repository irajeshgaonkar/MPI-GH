namespace HCA.Data;

public interface IHcaDbContextAccessor
{
    bool HasNonCoalitionDatabase { get; }
    HcaDbContext Current { get; }
    HcaDbContext Coalition { get; }
    HcaDbContext NonCoalition { get; }
    HcaDbContext GetDbContext(TenantDatabaseKind tenantDatabase);
}
