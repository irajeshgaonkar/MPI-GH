using System;
using HCA.Core.Mapper;
using HCA.Core.Processors;
using HCA.Core.Processors.CsvFileProcessor;
using HCA.Core.Services;
using HCA.Data.Entities;
using HCA.Models;
using HCA.Models.Request;
using Microsoft.Extensions.DependencyInjection;

namespace HCA.Core;

public static class Startup
{
    public static IServiceCollection AddServices(this IServiceCollection services)
    {
        //return services
        //    .AddScoped<IPostIdentityService, PostIdentityService>();

        services
            .AddScoped<IAuthenticationService, AuthenticationService>()
            .AddScoped<IClientIdentityService, ClientIdentityService>()
            .AddScoped<IUserService, UserService>();

        return services.AddProcessors();
    }

    public static IServiceCollection AddProcessors(this IServiceCollection services)
    {
        //return services
        //    .AddScoped<IPostIdentityService, PostIdentityService>();

        services.AddScoped<FileRequestProcessor>()
            .AddScoped<UserRequestProcessor>()
            .AddScoped<IFileReader, CsvFileReader>()
            .AddScoped<BaseParser<FileHeaderDataModel>, FileHeaderParser>()
            .AddScoped<BaseParser<ClientIdentityRequest>, ClientIdentityParser>()
            .AddScoped<IFileParser, CsvFileParser>()
            .AddScoped<IFileDataLoader, FileDataLoader>()
            .AddScoped<IProcessorProvider, ProcessorProvider>()
            .AddScoped<IPostIdentityProcessor, PostIdentityProcessor>()
            .AddScoped<ILinkIdentityProcessor, LinkIdentityProcessor>()
            .AddScoped<IUnLinkIdentityProcessor, UnLinkIdentityProcessor>()
            .AddScoped<IMergeIdentityProcessor, MergeIdentityProcessor>()
            .AddScoped<IUnMergeIdentityProcessor, UnMergeIdentityProcessor>()
            .AddScoped<IDemographicSearchProcessor, DemographicSearchProcessor>();

        return services;
    }

    public static IServiceCollection AddAutoMapper(this IServiceCollection services)
    {
        return services
            .AddScoped<IMapper<ClientIdentityRequestEntity, ClientIdentityRequest>, ClientIdentityRequestMapper>()
            .AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
    }
}

