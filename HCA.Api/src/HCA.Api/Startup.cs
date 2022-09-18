using HCA.Api.Filters;
using HCA.Api.Middleware;
using HCA.Api.Options;
using HCA.Core;
using HCA.Data;
using HCA.Infrastructure;
using HCA.MuleSoft;
using Microsoft.Extensions.PlatformAbstractions;
using Microsoft.OpenApi.Models;
using System.Reflection;

namespace HCA.Api;

public class Startup
{
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    static string BasePath
    {
        get
        {
            var basePath = PlatformServices.Default.Application.ApplicationBasePath;
            var fileName = typeof(Startup).GetTypeInfo().Assembly.GetName().Name + ".xml";
            return basePath;
        }
    }

    // This method gets called by the runtime. Use this method to add services to the container
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddControllers(o => o.Filters.Add<HcaExceptionFilter>());
        services.AddSwaggerGen(c =>
        {
            var basePath = BasePath;
            c.IncludeXmlComments(Path.Combine(basePath, "HCA.Api.xml"));
            c.IncludeXmlComments(Path.Combine(basePath, "HCA.Models.xml"));

            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "HCA MPI Coalition",
                Version = "v1"
            });
            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
            {
                Name = "Authorization",
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer",
                BearerFormat = "JWT",
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
                            Id = "Bearer"
                        }
                    },
                    new string[] {}
                }
            });
        });

        services.AddCors();
        services.AddHca(Configuration);
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline
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

        //if (env.IsDevelopment())
        //{
        app.UseSwagger();
        app.UseSwaggerUI();
        app.UseHttpsRedirection();
        //}

        app.UseRouting();
        app.UseAuthorization();


        app.UseEndpoints(endpoints =>
        {
            var securityOptions = Configuration.GetSection("SecurityOptions").Get<SecurityOptions>();
            app.UseMiddleware<JwtMiddleware>(securityOptions);
            endpoints.MapControllers();
            endpoints.MapGet("/", async context =>
            {
                await context.Response.WriteAsync("Mpi Coallation is running!");
            });
        });
    }
}
