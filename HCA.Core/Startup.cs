using HCA.Core.Mapper;
using HCA.Core.Processors;
using HCA.Core.Processors.File;
using HCA.Core.Processors.MPIDBSync;
using HCA.Core.Processors.Sftp;
using HCA.Core.Services;
using HCA.Data;
using HCA.Infrastructure;
using HCA.Infrastructure.Sqs;
using HCA.Verato;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HCA.Core;

public static class Startup
{
    public static IServiceCollection AddHca(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddAppSettings(configuration);
        services.AddConsoleLogging();
        services.AddDbContext(configuration);
        services.AddRepositories();
        services.AddVerato(configuration);
        services.AddHttpClients(configuration);
        services.AddServices();
        services.AddAutoMapper();
        services.AddFileProcessors();
        services.AddSqs(configuration);
        services.AddSftp();
        return services;
    }

    public static IServiceCollection AddServices(this IServiceCollection services)
    {
        services
            .AddScoped<IClientIdentityService, ClientIdentityService>()
            .AddScoped<IFileRequestService, FileRequestService>()
            .AddScoped<IUserRequestService, UserRequestService>()
            .AddScoped<IUserModifyRecordsService, UserModifyRecordsService>()
            .AddScoped<ICustomDataMappingService, CustomDataMappingService>()
            .AddScoped<IReportsService, ReportService>()
            .AddScoped<IUserModifyRecordsService, UserModifyRecordsService>()
            .AddScoped<IServiceAccountService, ServiceAccountService>(); 

        return services.AddProcessors();
    }

    public static IServiceCollection AddProcessors(this IServiceCollection services)
    {
        services
            .AddScoped<IVeratoRequestExecuter, VeratoRequestExecuter>()
            .AddScoped<IClientIdentityRequestExecutor, ClientIdentityRequestExecutor>()
            .AddScoped<IBatchRequestProcessor, BatchRequestProcessor>()
            .AddScoped<IFileWriter, CsvFileWriter>()
            .AddScoped<ISftpProcessor, SftpProcessor>()
            .AddScoped<IOutputFileWriter, OutputFileWriter>()
            .AddScoped<IMPIDBSynchronizer, MPIDBSynchronizer>();

        return services;
    }

    public static IServiceCollection AddAutoMapper(this IServiceCollection services)
    {
        return services
            .AddScoped<IClientIdentityRequestMapper, ClientIdentityRequestMapper>()
            .AddScoped<IFileClientIdentityMapper, FileClientIdentityMapper>()
            .AddScoped<IFileRequestMapper, FileRequestMapper>()
            .AddScoped<IUserRequestMapper, UserRequestMapper>()
            .AddScoped<ICustomDataMappingMapper, CustomDataMappingMapper>()
            .AddScoped<IServiceAccountMapper, ServiceAccountMapper>()
            .AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
    }

    public static IServiceCollection AddSqs(this IServiceCollection services, IConfiguration configuration)
    {
        return services
            .AddScoped<ISqsPublisher, SqsPublisher>()
            .AddScoped<IClientIdentitySQSPublisher, ClientIdentitySQSPublisher>();
    }
}

