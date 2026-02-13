using System.Net;
using System.Net.Http.Headers;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Collections.Generic;
using Amazon.Lambda.Core;
using HCA.Infrastructure.Configurations;
using HCA.Infrastructure.Http;
using HCA.Infrastructure.JObjectHelper;
using HCA.Infrastructure.Logger;
using HCA.Infrastructure.S3;
using HCA.Infrastructure.SecretsManager;
using HCA.Infrastructure.Security.Contracts;
using HCA.Infrastructure.Security.Hashing;
using HCA.Infrastructure.Security.Tokens;
using HCA.Infrastructure.sftp;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Polly;
using Polly.Retry;

namespace HCA.Infrastructure
{
    public static class Startup
    {
        public static IServiceCollection AddAppLogging(this IServiceCollection services, ILambdaContext context)
        {
            return services
                    .AddScoped(p => context.Logger)
                    .AddScoped<IAppLogger, AppLogger>();
        }

        public static IServiceCollection AddConsoleLogging(this IServiceCollection services)
        {
            return services
                    .AddScoped<IAppLogger, ConsoleAppAppLogger>();
        }

        public static IServiceCollection AddAppSettings(this IServiceCollection services, IConfiguration configuration)
        {
            return services.AddSingleton(AmazonSecretsManager.GetAppSettings());
        }

        public static IServiceCollection AddHttpClients(this IServiceCollection services, IConfiguration configuration)
        {
            //Register client for Verato with Basic Auth and Client Cert
            services.AddHttpClient<VeratoHttpClient>((sp, client) =>
            {
                var appSettings = sp.GetRequiredService<AppSettings>();

                client.BaseAddress = new Uri(appSettings.VeratoOptions.BaseUrl);
                client.Timeout = TimeSpan.FromSeconds(appSettings.VeratoOptions.RequestTimeoutInSec);

                var byteArray = Encoding.ASCII.GetBytes($"{appSettings.VeratoOptions.Username}:{appSettings.VeratoOptions.Password}");
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));

            }).ConfigurePrimaryHttpMessageHandler(sp =>
            {
                var appSettings = sp.GetRequiredService<AppSettings>();

                var handler = new HttpClientHandler();

                var certBytes = Convert.FromBase64String(appSettings.VeratoOptions.ClientCert);
                var certificate = new X509Certificate2(certBytes, appSettings.VeratoOptions.ClientCertPassword, X509KeyStorageFlags.MachineKeySet);

                handler.ClientCertificates.Add(certificate);
                handler.ClientCertificateOptions = ClientCertificateOption.Manual;

                return handler;

            }).AddPolicyHandler((sp, _) => GetRetryPolicy(sp));

            //Register client for Verato Enrich with Basic Auth and Client Cert
            services.AddHttpClient<VeratoEnrichHttpClient>((sp, client) =>
            {
                var appSettings = sp.GetRequiredService<AppSettings>();

                client.BaseAddress = new Uri(appSettings.VeratoOptions.EnrichBaseUrl);
                client.Timeout = TimeSpan.FromSeconds(appSettings.VeratoOptions.RequestTimeoutInSec);

                var byteArray = Encoding.ASCII.GetBytes($"{appSettings.VeratoOptions.EnrichUsername}:{appSettings.VeratoOptions.EnrichPassword}");
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));

            }).ConfigurePrimaryHttpMessageHandler(sp =>
            {
                var appSettings = sp.GetRequiredService<AppSettings>();

                var handler = new HttpClientHandler();

                var certBytes = Convert.FromBase64String(appSettings.VeratoOptions.ClientCert);
                var certificate = new X509Certificate2(certBytes, appSettings.VeratoOptions.ClientCertPassword, X509KeyStorageFlags.MachineKeySet);

                handler.ClientCertificates.Add(certificate);
                handler.ClientCertificateOptions = ClientCertificateOption.Manual;

                return handler;

            }).AddPolicyHandler((sp, _) => GetRetryPolicy(sp));

            return services;
        }

        public static IServiceCollection AddSftp( this IServiceCollection services )
        {
            services.AddScoped<ISftpToS3FileTransferClient, SftpToS3FileTransferClient>();
            services.AddScoped<IS3ToSftpFileTransferClient, S3ToSftpFileTransferClient>();
            services.AddScoped<IHcaSftpClient, HcaSftpClient>();
            services.AddScoped<IHcaS3Client, HcaS3Client>();
            services.AddScoped<IJObjectCreator,JObjectCreator>();
            return services;
        }

        public static IServiceCollection AddSecurity(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<TokenOptions>(configuration.GetSection("TokenOptions"));
            var tokenOptions = configuration.GetSection("TokenOptions").Get<TokenOptions>();

            var signingConfigurations = new SigningConfigurations(tokenOptions.Secret);
            services.AddSingleton(signingConfigurations);

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                    .AddJwtBearer(jwtBearerOptions =>
                    {
                        jwtBearerOptions.TokenValidationParameters = new TokenValidationParameters()
                        {
                            ValidateAudience = true,
                            ValidateLifetime = true,
                            ValidateIssuerSigningKey = true,
                            ValidIssuer = tokenOptions.Issuer,
                            ValidAudience = tokenOptions.Audience,
                            IssuerSigningKey = signingConfigurations.SecurityKey,
                            ClockSkew = TimeSpan.Zero
                        };
                    });

            services.AddSingleton(tokenOptions);
            services.AddScoped<IPasswordHasher, PasswordHasher>();
            services.AddScoped<ITokenHandler, Security.Tokens.TokenHandler>();

            return services;
        }

        /// <summary>
        /// Defines and returns a retry policy for HTTP requests, with exponential backoff.
        /// This policy retries requests in case of HttpRequestException or when the response status code matches
        /// the configured retryable codes, using an exponential backoff strategy.
        /// </summary>
        private static AsyncRetryPolicy<HttpResponseMessage> GetRetryPolicy(IServiceProvider sp)
        {
            var appSettings = sp.GetRequiredService<AppSettings>();

            var retryOptions = appSettings.VeratoOptions?.RetryOptions;
            var retryStatusCodes = retryOptions?.ReTriableStatusCode?.Length > 0
                ? [.. retryOptions.ReTriableStatusCode]
                : new HashSet<HttpStatusCode>
                {
                    HttpStatusCode.NotFound,
                    HttpStatusCode.InternalServerError,
                    HttpStatusCode.GatewayTimeout,
                    HttpStatusCode.RequestTimeout
                };

            var maxRetries = retryOptions?.MaxRetries > 0 ? retryOptions.MaxRetries : 3;

            return Polly.Policy.Handle<HttpRequestException>()  // Handle any HttpRequestException (e.g., network errors)
                .OrResult<HttpResponseMessage>(r => retryStatusCodes.Contains(r.StatusCode)) // Retry on configured status codes
                .WaitAndRetryAsync(
                    maxRetries,  // Retry up to configured max times
                    attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)) // Exponential backoff: 2^attempt seconds (e.g., 2, 4, 8 seconds, etc.)
                );
        }

    }

}
