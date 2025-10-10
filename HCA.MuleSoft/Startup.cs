using HCA.Infrastructure.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HCA.MuleSoft;

public static class Startup
{
    public static IServiceCollection AddMuleSoft(this IServiceCollection services, IConfiguration configuration)
    {
        return services
            .AddScoped<IMuleSoftRequestBuilder, MuleSoftRequestBuilder>()
            .AddScoped<IMuleSoftRepository, MuleSoftRepository>()
            .AddScoped<IDelayCaculator, ExponentialDelayCalculator>();
    }
}
