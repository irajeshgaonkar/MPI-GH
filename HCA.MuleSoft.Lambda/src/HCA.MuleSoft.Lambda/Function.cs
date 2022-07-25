using System.Text.Json;
using Amazon.Lambda.Core;
using Amazon.S3;
using HCA.Core;
using HCA.Core.Processors;
using HCA.Core.Processors.File;
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
    public async Task<ResponseModel> FunctionHandler(RequestModel request, ILambdaContext context)
    {
        var configuration = ConfigureSettings();

        var serviceProvider = ConfigureServices(context, new ServiceCollection(), configuration);
        var logger = serviceProvider.GetRequiredService<IAppLogger>();
        logger.LogInformation($"Started Processing event {request}");

        if (null == request)
        {
            logger.LogInformation("Unable to Process the Request, Deserialization error");
            return null;
        }

        logger.LogInformation($"Started Processing event {request.BucketName} | {request.FileName}| {request.OperationType} | {request.RequestId} ");

        logger.LogInformation($"Started Processing event for operation type {request.OperationType}");

        if (request.OperationType == Constants.FileDataLoadOperation)
        {
            var requestId = await ProcessFileDataLoadRequest(serviceProvider, request);
            var response = new ResponseModel()
            {
                bucketName = request.BucketName,
                fileName = request.FileName,
                operationType = Constants.FileDataProcess,
                requestId = requestId
            };
            return response;
        }

        else if (request.OperationType == Constants.FileDataProcess)
        {
            await ProcessFileDataRequest(serviceProvider, request);
            await WriteFileToS3(serviceProvider, configuration, request);
        }

        return null;
    }

    private async Task ProcessFileDataRequest(ServiceProvider serviceProvider, RequestModel request)
    {
        var logger = serviceProvider.GetRequiredService<IAppLogger>();
        logger.LogInformation($"Started processing request {request.RequestId}");
        var requestId = request.RequestId;
        var fileRequestProcessor = serviceProvider.GetRequiredService<IFileRequestProcessor>();
        await fileRequestProcessor.ProcessRequest(requestId);
        logger.LogInformation($"Completed processing request {request.RequestId}");
    }

    private async Task<int> ProcessFileDataLoadRequest(ServiceProvider serviceProvider, RequestModel request)
    {
        var logger = serviceProvider.GetRequiredService<IAppLogger>();
        var fileProcessor = serviceProvider.GetRequiredService<IFileProcessor>();
        var streamReader = await GetStreamReader(request.BucketName, request.FileName);
        var requestId = await fileProcessor.ProcessFile(request.FileName, streamReader);
        return requestId;
    }

    private async Task<StreamReader> GetStreamReader(string bucketName, string fileName)
    {
        var fileContent = await S3Client.GetObjectAsync(bucketName, fileName);
        var fileStream = fileContent.ResponseStream;
        var streamReader = new StreamReader(fileStream);
        return streamReader;
    }

    private async Task WriteFileToS3(ServiceProvider serviceProvider, IConfiguration configuration, RequestModel request)
    {
        var streamReader = await GetStreamReader(request.BucketName, request.FileName);
        var outputBucketName = configuration["OputBucketName"];
        var logger = serviceProvider.GetRequiredService<IAppLogger>();
        logger.LogInformation("started uplodated file");
        var fileWriter = serviceProvider.GetRequiredService<IFileWriter>();
        MemoryStream memoryStream = await fileWriter.WriteFile(request.RequestId, streamReader);
        await S3Client.UploadObjectFromStreamAsync(outputBucketName, "output_" + request.FileName, memoryStream, new Dictionary<string, object>());
        logger.LogInformation("Successfully uplodated file");
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

    private string Serialize<T>(T payLoad)
    {
        var serializeOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };

        var result = JsonSerializer.Serialize(payLoad, serializeOptions);
        return result;
    }

    public ServiceProvider ConfigureServices(ILambdaContext context, IServiceCollection services, IConfiguration configuration)
    {
        var serviceProvider = services
                                .AddScoped((s) => S3Client)
                                //.AddFileWriterReader()
                                .AddScoped<FileWriter>()
                                .AddAppLogging(context)
                                .AddDbContext(configuration)
                                .AddRepositories()
                                .AddMuleSoft(configuration)
                                .AddServices()
                                .AddAutoMapper()
                                .AddProcessors()
                                .AddFileProcessors()
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

