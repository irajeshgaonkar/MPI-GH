using HCA.FileProcessor.FileReaders;
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

