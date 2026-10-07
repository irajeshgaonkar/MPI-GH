namespace HCA.Data;

public static class TenantDatabaseKindExtensions
{
    public const string CoalitionTenant = "HHS Coalition";
    public const string NonCoalitionTenant = "Non-Coalition";

    public static string ToTenantValue(this TenantDatabaseKind tenantDatabase)
        => tenantDatabase == TenantDatabaseKind.NonCoalition
            ? NonCoalitionTenant
            : CoalitionTenant;

    public static bool TryParseTenantValue(string? value, out TenantDatabaseKind tenantDatabase)
    {
        if (string.Equals(value, NonCoalitionTenant, StringComparison.OrdinalIgnoreCase)
            || string.Equals(value, TenantDatabaseKind.NonCoalition.ToString(), StringComparison.OrdinalIgnoreCase))
        {
            tenantDatabase = TenantDatabaseKind.NonCoalition;
            return true;
        }

        if (string.Equals(value, CoalitionTenant, StringComparison.OrdinalIgnoreCase)
            || string.Equals(value, TenantDatabaseKind.Coalition.ToString(), StringComparison.OrdinalIgnoreCase))
        {
            tenantDatabase = TenantDatabaseKind.Coalition;
            return true;
        }

        tenantDatabase = TenantDatabaseKind.Coalition;
        return false;
    }
}
