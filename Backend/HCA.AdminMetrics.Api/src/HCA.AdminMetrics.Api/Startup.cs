using Amazon.CloudWatch;
using Amazon.CloudWatchLogs;
using Amazon.Extensions.NETCore.Setup;
using HCA.Data;
using HCA.AdminMetrics.Api.Filters;
using HCA.AdminMetrics.Api.Middleware;
using HCA.AdminMetrics.Api.Options;
using HCA.AdminMetrics.Api.Services;
using HCA.Infrastructure;
using HCA.Infrastructure.Configurations;
using HCA.Infrastructure.Logger;
using Microsoft.Extensions.PlatformAbstractions;
using Microsoft.OpenApi.Models;
using System.Reflection;

namespace HCA.AdminMetrics.Api;

/// <summary>
/// Configures services and middleware for the Admin Metrics API application.
/// </summary>
/// <param name="configuration">The application configuration.</param>
public class Startup( IConfiguration configuration )
{

    /// <summary>
    /// Gets the application configuration.
    /// </summary>
    public IConfiguration Configuration { get; } = configuration;

    static string BasePath
    {
        get
        {
            var basePath = PlatformServices.Default.Application.ApplicationBasePath;
            return basePath;
        }
    }

    /// <summary>
    /// Configures the services used by the Admin Metrics API.
    /// </summary>
    /// <param name="services">The service collection.</param>
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddControllers(o => o.Filters.Add<GlobalExceptionFilter>());
        services.AddSwaggerGen(c =>
        {
            var basePath = BasePath;
            c.IncludeXmlComments(Path.Combine(basePath, "HCA.AdminMetrics.Api.xml"));
            c.IncludeXmlComments(Path.Combine(basePath, "HCA.Models.xml"));

            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "HCA MPI Admin Metrics",
                Version = "v1"
            });
            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
            {
                Name = Constants.RequestHeaders.Authorization,
                Type = SecuritySchemeType.ApiKey,
                Scheme = Constants.RequestHeaders.Bearer,
                BearerFormat = Constants.RequestHeaders.JWT,
                In = ParameterLocation.Header,
                Description = "JWT Authorization header using the Bearer scheme."
            });
            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = Constants.RequestHeaders.Bearer
                        }
                    },
                    Array.Empty<string>()
                }
            });
        });

        services.AddCors();
        services.AddHttpContextAccessor();
        services.AddConsoleLogging();
        services.AddDefaultAWSOptions(Configuration.GetAWSOptions());
        services.AddAWSService<IAmazonCloudWatch>();
        services.AddAWSService<IAmazonCloudWatchLogs>();
        services.Configure<AdminMetricsOptions>(Configuration.GetSection(AdminMetricsOptions.SectionName));
        services.AddAppSettings(Configuration);
        services.AddDbContext(Configuration);
        services.AddScoped<IAdminMetricsService, AdminMetricsService>();
        services.AddScoped<IAdminUsageMetricsService, AdminUsageMetricsService>();
        services.AddScoped<IAdminApiTrafficService, AdminApiTrafficService>();
        services.AddScoped<IAdminReportsService, AdminReportsService>();
        services.AddScoped<IAdminOnboardedSystemService, AdminOnboardedSystemService>();
    }

    /// <summary>
    /// Configures the HTTP request pipeline.
    /// </summary>
    /// <param name="app">The application builder.</param>
    /// <param name="env">The hosting environment.</param>
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        app.UseCors(builder =>
        {
            builder
                .AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader();
        });

        app.UseSwagger();
        app.UseSwaggerUI();
        app.UseHttpsRedirection();
        app.UseRouting();
        app.UseAuthorization();
        app.UseJwtMiddleware();
        app.UseMiddleware<ResponseHeaderMiddleware>();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
            endpoints.MapGet("/", async context =>
            {
                await context.Response.WriteAsync("MPI Admin Metrics API is running!");
            });
        });
    }
}
