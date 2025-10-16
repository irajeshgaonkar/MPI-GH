using HCA.Infrastructure.Http;
using HCA.Verato.Impl;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HCA.Verato;

public static class Startup
{
    public static IServiceCollection AddVerato(this IServiceCollection services, IConfiguration configuration)
    {
        return services
            .AddScoped<IVeratoRequestBuilder, VeratoRequestBuilder>()
            .AddScoped<IVeratoRepository, VeratoRepository>()
            .AddScoped<IDelayCaculator, ExponentialDelayCalculator>();
    }
}
