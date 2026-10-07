using Microsoft.AspNetCore.Http;

namespace HCA.Data;

public sealed class TenantContext(IHttpContextAccessor httpContextAccessor) : ITenantContext
{
    private static readonly AsyncLocal<TenantDatabaseKind?> CurrentTenantDatabaseValue = new();
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

    public TenantDatabaseKind CurrentTenantDatabase
    {
        get
        {
            var value = _httpContextAccessor.HttpContext?.Items[TenantContextItemKeys.TenantDatabase]?.ToString();
            return TenantDatabaseKindExtensions.TryParseTenantValue(value, out var tenantDatabase)
                ? tenantDatabase
                : CurrentTenantDatabaseValue.Value ?? TenantDatabaseKind.Coalition;
        }
    }

    public void SetTenantDatabase(TenantDatabaseKind tenantDatabase)
    {
        CurrentTenantDatabaseValue.Value = tenantDatabase;
        _httpContextAccessor.HttpContext ??= new DefaultHttpContext();
        _httpContextAccessor.HttpContext.Items[TenantContextItemKeys.TenantDatabase] = tenantDatabase.ToString();
        _httpContextAccessor.HttpContext.Items[TenantContextItemKeys.Tenant] = tenantDatabase.ToString();
    }
}
