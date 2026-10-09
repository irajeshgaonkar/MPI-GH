using Amazon.Lambda.Core;
using HCA.Core;
using HCA.Core.Processors.MPIDBSync;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace HCA.MPI.DBSync.Lambda;

public class Function
{
    
    public async Task FunctionHandler(object input, ILambdaContext context)
    {
        var configuration = ConfigureSettings();
        var serviceProvider = ConfigureServices(context, new ServiceCollection(), configuration);

        var mpiDBSynchronizer = serviceProvider.GetRequiredService<IMPIDBSynchronizer>();

        await mpiDBSynchronizer.SyncMPIDB();

    }

    private static ServiceProvider ConfigureServices(ILambdaContext context, IServiceCollection services, IConfiguration configuration)
    {
        return services.AddHca(configuration).BuildServiceProvider();
    }

    private static IConfiguration ConfigureSettings()
    {
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: true)
            .Build();

        return configuration;
    }
}
