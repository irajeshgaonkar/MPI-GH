using Microsoft.Extensions.DependencyInjection;
using HCA.Infrastructure.Logger;
using Amazon.Lambda.Core;
using HCA.Infrastructure.Security.Tokens;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using HCA.Infrastructure.Security.Contracts;
using HCA.Infrastructure.Security.Hashing;
using Microsoft.Extensions.Configuration;
using HCA.Infrastructure.sftp;
using HCA.Infrastructure.Sftp;
using HCA.Infrastructure.S3;
using HCA.Infrastructure.JObjectHelper;
using System.Text.Json;
using HCA.Infrastructure.SecretsManager;

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


        public static IServiceCollection AddSftp(this IServiceCollection services, IConfiguration configuration)
        {
            // TODO: change to secrets load
            //SftpOptions sftpOptions = configuration.GetSection("sftp").Get<SftpOptions>();
            string sftpSecret = AmazonSecretsManager.GetSecret().Result;
            SftpOptions sftpOptions = JsonSerializer.Deserialize<SftpOptions>(sftpSecret)
                                      ?? throw new ArgumentException("Sftp secret load issue");

            services.AddSingleton(sftpOptions);
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
    }

}

