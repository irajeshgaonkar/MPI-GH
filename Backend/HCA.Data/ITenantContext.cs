namespace HCA.Data;

public interface ITenantContext
{
    TenantDatabaseKind CurrentTenantDatabase { get; }

    void SetTenantDatabase(TenantDatabaseKind tenantDatabase);
}
