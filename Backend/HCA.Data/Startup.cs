using HCA.Data.Repository;
using HCA.Data.Repository.Impl;
using HCA.Infrastructure.Configurations;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace HCA.Data;

public static class Startup
{
    public static IServiceCollection AddDbContext(this IServiceCollection services, IConfiguration configuration)
    {
        services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        services.AddScoped<ITenantContext, TenantContext>();
        services.AddScoped<IHcaDbContextAccessor, HcaDbContextAccessor>();
        services.AddScoped<IOnboardedSystemTenantResolver, OnboardedSystemTenantResolver>();

        services.AddDbContext<HcaDbContext>((sp, options) =>
        {
            var appSettings = sp.GetRequiredService<AppSettings>();
            var tenantContext = sp.GetRequiredService<ITenantContext>();
            var connectionString = appSettings.DbConnectionStr;

            if (tenantContext.CurrentTenantDatabase == TenantDatabaseKind.NonCoalition)
            {
                connectionString = appSettings.NonCoalitionDbConnectionStr;
            }

            if (string.IsNullOrWhiteSpace(connectionString))
                throw new InvalidOperationException("Database connection string is missing.");

            options.UseNpgsql(connectionString);
        });

        return services;
    }

    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        return services
                .AddScoped<IClientIdentityRepository, ClientIdentityRepository>()
                .AddScoped<IClientIdentityRequestRepository, ClientIdentityRequestRepository>()
                .AddScoped<IFileRequestRepository, FileRequestRepository>()
                .AddScoped<IUserRequestRepository, UserRequestRepository>()
                .AddScoped<IRequestProcessLogRepository, RequestProcessLogRepository>()
                .AddScoped<IUserModifyRecordsRepository, UserModifyRecordsRepository>()
                .AddScoped<ISftpFileTransferRepository, SftpFileTransferRepository>()
                .AddScoped<ICustomDataMappingRepository, CustomDataMappingRepository>()
                .AddScoped<IServiceAccountRepository, ServiceAccountRepository>()
                .AddScoped<IOnboardedSystemRepository, OnboardedSystemRepository>()
                .AddScoped<IDataShareMappingRepository, DataShareMappingRepository>()
                .AddScoped<IAppRoleMappingRepository, AppRoleMappingRepository>();
    }
}
