using System;
using HCA.Core.Mapper;
using HCA.Core.Processors;
using HCA.Core.Processors.File;
using HCA.Core.Processors.Sftp;
using HCA.Core.Services;
using HCA.Data;
using HCA.Data.Entities;
using HCA.Infrastructure;
using HCA.Infrastructure.Sqs;
using HCA.Models;
using HCA.Models.Request;
using HCA.MuleSoft;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HCA.Core;

public static class Startup
{
    public static IServiceCollection AddHca(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddConsoleLogging();
        services.AddDbContext(configuration);
        services.AddRepositories();
        services.AddMuleSoft(configuration);
        services.AddServices();
        services.AddAutoMapper();
        services.AddFileProcessors();
        services.AddSqs(configuration);
        services.AddSftp(configuration);
        return services;
    }

    public static IServiceCollection AddServices(this IServiceCollection services)
    {
        services
            .AddScoped<IClientIdentityService, ClientIdentityService>()
            .AddScoped<IFileRequestService, FileRequestService>()
            .AddScoped<IUserRequestService, UserRequestService>()
            .AddScoped<IUserModifyRecordsService, UserModifyRecordsService>(); 

        return services.AddProcessors();
    }

    public static IServiceCollection AddProcessors(this IServiceCollection services)
    {
        services
            .AddScoped<IMuleSoftRequestExecuter, MuleSoftRequestExecuter>()
            .AddScoped<IClientIdentityRequestExecutor, ClientIdentityRequestExecutor>()
            .AddScoped<IBatchRequestProcessor, BatchRequestProcessor>()
            .AddScoped<IFileWriter, CsvFileWriter>()
            .AddScoped<ISftpProcessor, SftpProcessor>()
            .AddScoped<IOutputFileWriter, OutputFileWriter>();

        return services;
    }

    public static IServiceCollection AddAutoMapper(this IServiceCollection services)
    {
        return services
            .AddScoped<IClientIdentityRequestMapper, ClientIdentityRequestMapper>()
            .AddScoped<IFileClientIdentityMapper, FileClientIdentityMapper>()
            .AddScoped<IFileRequestMapper, FileRequestMapper>()
            .AddScoped<IUserRequestMapper, UserRequestMapper>()
            .AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
    }

    public static IServiceCollection AddSqs(this IServiceCollection services, IConfiguration configuration)
    {
        var sqsOptions = configuration.GetSection("SqsOptions").Get<SqsOptions>();
        var outputBucketName = configuration["OutputBucketName"];
        var inputBucketName = configuration["InputBucketName"];

        return services
            .AddSingleton(sqsOptions)
            .AddSingleton(new S3Options { OutputBucketName = outputBucketName, InputBucketName = inputBucketName})
            .AddScoped<ISqsPublisher, SqsPublisher>()
            .AddScoped<IClientIdentitySQSPublisher, ClientIdentitySQSPublisher>();
    }
}

