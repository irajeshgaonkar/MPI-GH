using HCA.Infrastructure.Configurations;

namespace HCA.Data;

public static class AppSettingsTenantExtensions
{
    public static string GetInputBucketName(this AppSettings appSettings, TenantDatabaseKind tenantDatabase)
        => tenantDatabase == TenantDatabaseKind.NonCoalition
            ? ValidateRequiredBucketName(appSettings.NonCoalitionInputBucketName, "non-coalition input")
            : appSettings.InputBucketName;

    public static string GetOutputBucketName(this AppSettings appSettings, TenantDatabaseKind tenantDatabase)
        => tenantDatabase == TenantDatabaseKind.NonCoalition
            ? ValidateRequiredBucketName(appSettings.NonCoalitionOutputBucketName, "non-coalition output")
            : appSettings.OutputBucketName;

    private static string ValidateRequiredBucketName(string? bucketName, string bucketDescription)
    {
        if( string.IsNullOrWhiteSpace( bucketName ) )
        {
            throw new InvalidOperationException($"The {bucketDescription} S3 bucket name is missing.");
        }

        return bucketName;
    }
}
