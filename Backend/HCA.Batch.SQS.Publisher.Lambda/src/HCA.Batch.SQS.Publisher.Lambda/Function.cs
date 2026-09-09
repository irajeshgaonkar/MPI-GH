using Amazon.Lambda.Core;
using Amazon.S3;
using HCA.Batch.SQS.Publisher.Lambda.Models;
using HCA.Core;
using HCA.Core.Processors;
using HCA.Infrastructure.Logger;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace HCA.Batch.SQS.Publisher;

public class Function
{
    IAmazonS3 S3Client { get; set; }

    /// <summary>
    /// Default constructor. This constructor is used by Lambda to construct the instance. When invoked in a Lambda environment
    /// the AWS credentials will come from the IAM role associated with the function and the AWS region will be set to the
    /// region the Lambda function is executed in.
    /// </summary>
    public Function()
    {
        S3Client = new AmazonS3Client();
    }

    /// <summary>
    /// Constructs an instance with a preconfigured S3 client. This can be used for testing the outside of the Lambda environment.
    /// </summary>
    /// <param name="s3Client"></param>
    public Function(IAmazonS3 s3Client)
    {
        S3Client = s3Client;
    }

    /// <summary>
    /// A simple function that takes a string and does a ToUpper
    /// </summary>
    /// <param name="input"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    public async Task<RequestModel> FunctionHandler(RequestModel request, ILambdaContext context)
    {
        var configuration = ConfigureSettings();

        var serviceProvider = ConfigureServices(context, new ServiceCollection(), configuration);
        var logger = serviceProvider.GetRequiredService<IAppLogger>();
        logger.LogInformation($"Started Processing FunctionHandler {request.RequestId}");
        var sqsPublisher = serviceProvider.GetRequiredService<IClientIdentitySQSPublisher>();
        await sqsPublisher.Publish(request.RequestId);
        return request;
    }

    public ServiceProvider ConfigureServices(ILambdaContext context, IServiceCollection services, IConfiguration configuration)
    {
        var serviceProvider = services.AddHca(configuration).BuildServiceProvider();
        return serviceProvider;
    }

    IConfiguration ConfigureSettings()
    {
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: true)
            .Build();

        return configuration;
    }
}

