using System;
using HCA.Core.Mapper;
using HCA.Core.Processors;
using HCA.Core.Processors.File;
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

        services
            .AddScoped<IFileRequestProcessor, FileRequestProcessor>()
            .AddScoped<IRequestUpdater, RequestUpdater>()
            .AddScoped<IFileProcessor, FilProcessor>()
            .AddScoped<IMuleSoftRequestExecuter, MuleSoftRequestExecuter>()
            .AddScoped<IClientIdentityRequestExecutor, ClientIdentityRequestExecutor>()
            .AddScoped<IFileWriter, FileWriter>();

        return services;
    }

    public static IServiceCollection AddAutoMapper(this IServiceCollection services)
    {
        return services
            .AddScoped<IClientIdentityRequestMapper, ClientIdentityRequestMapper>()
            .AddScoped<IFileClientIdentityMapper, FileClientIdentityMapper>()
            .AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
    }
}

