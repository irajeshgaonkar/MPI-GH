using System;
using HCA.Infrastructure.Http;
using HCA.MuleSoft.RequestBuilder;
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

        services.AddSingleton(muleSoftOptions);
        services.AddSingleton(httpOptions);

        return services
            .AddScoped<IPostIdentityRequestBuilder, PostIdentityRequestBuilder>()
            .AddScoped<ILinkIdentitiesRequestBuilder, LinkIdentityRequestBuilder>()
            .AddScoped<IUnLinkIdentitiesRequestBuilder, UnLinkIdentityRequestBuilder>()
            .AddScoped<IMergeIdentitiesRequestBuilder, MergeIdentitiesRequestBuilder>()
            .AddScoped<IUnMergeIdentitiesRequestBuilder, UnMergeIdentitiesRequestBuilder>()
            .AddScoped<IPostIdentityRequestBuilder, PostIdentityRequestBuilder>()
            .AddScoped<IDemographicSearchRequestBuilder, DemographicSearchRequestBuilder>()
            .AddScoped<IHttpAdapter, HttpAdapter>()
            .AddScoped<IMuleSoftRepository, MuleSoftRepository>();
    }
}
