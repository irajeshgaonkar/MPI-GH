using System;
using HCA.Data.Entities;
using HCA.FileProcessor.FileReaders;
using HCA.Models;
using HCA.Models.Request;
using Microsoft.Extensions.DependencyInjection;

namespace HCA.Core;

public static class Startup
{
    public static IServiceCollection AddFileProcessors(this IServiceCollection services)
    {
        //return services
        //    .AddScoped<IPostIdentityService, PostIdentityService>();

        services
            .AddScoped<IFileReader, CsvFileReader>();

        return services;
    }
}

