using System.Text.Json;
using Amazon.Lambda.Core;
using Amazon.S3;
using HCA.Core;
using HCA.Core.Processors;
using HCA.Data;
using HCA.Infrastructure;
using HCA.Infrastructure.Logger;
using HCA.MuleSoft.Lambda.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace HCA.MuleSoft.Lambda;

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
    public async Task<string> FunctionHandler(string input, ILambdaContext context)
    {
        var configuration = ConfigureSettings();

        var serviceProvider = ConfigureServices(context, new ServiceCollection(), configuration);
        var logger = serviceProvider.GetRequiredService<ILogger>();
        logger.LogInformation($"Started Processing event {input}");

        var request = DeSerialize<RequestModel>(input);

        if (null == request)
        {
            logger.LogInformation("Unable to Process the Request, Deserialization error");
            return string.Empty;
        }

        logger.LogInformation($"Started Processing event for operation type {request.OperationType}");

        if (request.OperationType == Constants.FileDataLoadOperation)
        {
            await ProcessFileDataLoadRequest(serviceProvider, request);
        }
        else if (request.OperationType == Constants.FileDataProcess)
        {
            await ProcessFileDataRequest(serviceProvider, request);
        }

        return input.ToUpper();

    }

    private async Task ProcessFileDataRequest(ServiceProvider serviceProvider, RequestModel request)
    {
        var logger = serviceProvider.GetRequiredService<ILogger>();
        logger.LogInformation($"Started processing request {request.RequestId}");
        var requestId = new Guid(request.RequestId);
        var fileRequestProcessor = serviceProvider.GetRequiredService<FileRequestProcessor>();
        await fileRequestProcessor.ProcessRequest(requestId);
        logger.LogInformation($"Completed processing request {request.RequestId}");
    }

        private async Task ProcessFileDataLoadRequest(ServiceProvider serviceProvider, RequestModel request)
    {
        var logger = serviceProvider.GetRequiredService<ILogger>();
        var fileDataLoader = serviceProvider.GetRequiredService<IFileDataLoader>();


        var response = await S3Client.GetObjectMetadataAsync(request.BucketName, request.FileName);
        logger.LogInformation(response.Headers.ContentType);
        logger.LogInformation(response.HttpStatusCode.ToString());
        var fileContent = await S3Client.GetObjectAsync(request.BucketName, request.FileName);
        var fileStream = fileContent.ResponseStream;
        await fileDataLoader.ProcessFile(request.FileName, new StreamReader(fileStream));
    }

    private T? DeSerialize<T>(string payLoad)
    {
        var serializeOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };

        T? result = JsonSerializer.Deserialize<T>(payLoad, serializeOptions);
        return result;
    }

    public ServiceProvider ConfigureServices(ILambdaContext context, IServiceCollection services, IConfiguration configuration)
    {
        var serviceProvider = services
                                .AddAppLogging(context)
                                .AddDbContext(configuration)
                                .AddRepositories()
                                .AddMuleSoft(configuration)
                                .AddServices()
                                .AddAutoMapper()
                                .BuildServiceProvider();

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

