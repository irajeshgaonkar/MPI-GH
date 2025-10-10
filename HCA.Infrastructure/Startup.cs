using System.Net;
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
            services.AddHttpClient<MuleSoftHttpClient>((sp, client) =>
            {
                var appSettings = sp.GetRequiredService<AppSettings>();

                client.BaseAddress = new Uri(appSettings.MuleSoft.BaseUrl);
            }).AddPolicyHandler(GetRetryPolicy());

            services.AddHttpClient<TokenHttpClient>(client =>
            {
            }).AddPolicyHandler(GetRetryPolicy());

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
        /// This policy retries requests in case of HttpRequestException or when the response status is 404 (Not Found) 
        /// or 500 (Internal Server Error), using an exponential backoff strategy.
        /// </summary>
        private static AsyncRetryPolicy<HttpResponseMessage> GetRetryPolicy()
        {
            return Polly.Policy.Handle<HttpRequestException>()  // Handle any HttpRequestException (e.g., network errors)
                .OrResult<HttpResponseMessage>(r => r.StatusCode == HttpStatusCode.NotFound || r.StatusCode == HttpStatusCode.InternalServerError) // Retry on 404 or 500 status codes
                .WaitAndRetryAsync(
                    3,  // Retry up to 3 times
                    attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)) // Exponential backoff: 2^attempt seconds (e.g., 2, 4, 8 seconds, etc.)
                );
        }

    }

}

