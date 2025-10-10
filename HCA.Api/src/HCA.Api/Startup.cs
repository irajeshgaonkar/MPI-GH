using HCA.Api.Constants;
using HCA.Api.Filters;
using HCA.Api.Middleware;
using HCA.Api.Options;
using HCA.Core;
using Microsoft.Extensions.PlatformAbstractions;
using Microsoft.OpenApi.Models;
using System.Reflection;

namespace HCA.Api;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
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
            // TODO: check if can be removed?
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
                Name = RequestHeaders.Authorization,
                Type = SecuritySchemeType.ApiKey,
                Scheme = RequestHeaders.Bearer,
                BearerFormat = RequestHeaders.JWT,
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
                            Id = RequestHeaders.Bearer
                        }
                    },
                    new string[] {}
                }
            });
        });

        services.AddCors();

        services.AddHca(Configuration);

        services.AddHttpContextAccessor();

        services.AddScoped<IPValidationFilter>();
        services.AddScoped<ValidateIdentityFilter>();
        //?Todo: !Refactor
        //services
        //        .AddScoped<ISessionService, SessionService>()
        //        .AddScoped<ISourceSystemValidator, SourceSystemValidator>();
    }


    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member

        if( env.IsDevelopment())
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

        if (env.IsDevelopment())
        {
            
        }
        
        app.UseSwagger();
        app.UseSwaggerUI();
        app.UseHttpsRedirection();
        app.UseRouting();
        app.UseAuthorization();
        app.UseJwtMiddleware();
        app.UseMiddleware<ResponseHeaderMiddleware>();

        //Todo: ;write use source system middleware


        app.UseEndpoints(endpoints =>
        {
           
            endpoints.MapControllers();
            endpoints.MapGet("/", async context =>
            {
                await context.Response.WriteAsync("Mpi Coallation is running!");
            });
        });
    }
}
