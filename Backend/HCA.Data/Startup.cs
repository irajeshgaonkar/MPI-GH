using HCA.Data.Repository;
using HCA.Data.Repository.Impl;
using HCA.Infrastructure.Configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HCA.Data;

public static class Startup
{
    public static IServiceCollection AddDbContext(this IServiceCollection services, IConfiguration configuration)
    {

        services.AddDbContext<HcaDbContext>((sp, options) =>
        {
            var appSettings = sp.GetRequiredService<AppSettings>();

            if (string.IsNullOrWhiteSpace(appSettings.DbConnectionStr))
                throw new InvalidOperationException("Database connection string is missing.");

            options.UseNpgsql(appSettings.DbConnectionStr);
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