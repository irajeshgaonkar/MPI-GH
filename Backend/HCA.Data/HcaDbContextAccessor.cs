using HCA.Infrastructure.Configurations;
using Microsoft.EntityFrameworkCore;

namespace HCA.Data;

public sealed class HcaDbContextAccessor(AppSettings appSettings, ITenantContext tenantContext) : IHcaDbContextAccessor, IDisposable, IAsyncDisposable
{
    private readonly AppSettings _appSettings = appSettings;
    private readonly ITenantContext _tenantContext = tenantContext;

    private HcaDbContext? _coalitionContext;
    private HcaDbContext? _nonCoalitionContext;

    public bool HasNonCoalitionDatabase => !string.IsNullOrWhiteSpace(_appSettings.NonCoalitionDbConnectionStr);

    public HcaDbContext Current => GetDbContext(_tenantContext.CurrentTenantDatabase);

    public HcaDbContext Coalition => _coalitionContext ??= CreateContext(_appSettings.DbConnectionStr, "coalition");

    public HcaDbContext NonCoalition => _nonCoalitionContext ??= CreateContext(_appSettings.NonCoalitionDbConnectionStr, "non-coalition");

    public HcaDbContext GetDbContext(TenantDatabaseKind tenantDatabase)
        => tenantDatabase == TenantDatabaseKind.NonCoalition ? NonCoalition : Coalition;

    public void Dispose()
    {
        _coalitionContext?.Dispose();
        _nonCoalitionContext?.Dispose();
    }

    public async ValueTask DisposeAsync()
    {
        if (_coalitionContext != null)
            await _coalitionContext.DisposeAsync();

        if (_nonCoalitionContext != null)
            await _nonCoalitionContext.DisposeAsync();
    }

    private static HcaDbContext CreateContext(string? connectionString, string databaseName)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException($"The {databaseName} database connection string is missing.");

        var optionsBuilder = new DbContextOptionsBuilder<HcaDbContext>();
        optionsBuilder.UseNpgsql(connectionString);
        return new HcaDbContext(optionsBuilder.Options);
    }
}
