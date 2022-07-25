using System;
using HCA.Infrastructure.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HCA.MuleSoft;

public static class Startup
{
    public static IServiceCollection AddMuleSoft(this IServiceCollection services, IConfiguration configuration)
    {
        var muleSoftOptions = configuration.GetSection("MuleSoft").Get<MuleSoftOptions>();
        var httpOptions = new HttpOptions()
        {
            BaseUrl = muleSoftOptions.BaseUrl,
            CommonHeaders = new Dictionary<string, string>
            {
                { "client_id",  muleSoftOptions.ClientId },
                { "client_secret", muleSoftOptions.ClientSecret }
            }
        };

        var mulSoftReTryOptions = muleSoftOptions.RetryOptions;

        services.AddSingleton(muleSoftOptions);
        services.AddSingleton(mulSoftReTryOptions);
        services.AddSingleton(httpOptions);

        return services
            .AddScoped<IMuleSoftRequestBuilder, MuleSoftRequestBuilder>()
            .AddScoped<IMuleSoftRepository, MuleSoftRepository>()
            .AddScoped<IHttpAdapter, HttpAdapter>()
            .AddScoped<IDelayCaculator, ExponentialDelayCalculator>();
    }
}
