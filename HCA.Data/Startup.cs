using System;
using HCA.Data.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HCA.Data;

public static class Startup
{
    public static IServiceCollection AddDbContext(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionDetails = new ConnectionDetails { ConnectionString = configuration["DbConnectionStr"] };
        return services
            .AddSingleton(connectionDetails)
            .AddDbContext<HcaDbContext>(options => options.UseNpgsql(connectionDetails.ConnectionString));
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
                .AddScoped<ISftpFileTransferRepository, SftpFileTransferRepository>();
    }
}