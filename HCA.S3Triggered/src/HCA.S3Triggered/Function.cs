using Amazon.Lambda.Core;
using Amazon.Lambda.S3Events;
using Amazon.S3;
using Amazon.S3.Util;
using HCA.Core;
using HCA.Core.Processors;
using HCA.Data;
using HCA.Infrastructure;
using HCA.Infrastructure.Logger;
using HCA.MuleSoft;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace HCA.S3Triggered;

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
        this.S3Client = s3Client;
    }
    
    /// <summary>
    /// This method is called for every Lambda invocation. This method takes in an S3 event object and can be used 
    /// to respond to S3 notifications.
    /// </summary>
    /// <param name="evnt"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    public async Task<string?> FunctionHandler(S3Event evnt, ILambdaContext context)
    {
        var s3Event = evnt.Records?[0].S3;
        if(s3Event == null)
        {
            return null;
        }

        try
        {
            var response = await this.S3Client.GetObjectMetadataAsync(s3Event.Bucket.Name, s3Event.Object.Key);
            var configuration = ConfigureSettings();
            var serviceProvider = ConfigureServices(context, new ServiceCollection(), configuration);
            var logger = serviceProvider.GetRequiredService<ILogger>();
            logger.LogInformation($"Started Processing event {s3Event.Bucket.Name} {s3Event.Object.Key}");
            var fileContent = await S3Client.GetObjectAsync(s3Event.Bucket.Name, s3Event.Object.Key);
            var fileStream = fileContent.ResponseStream;
            var streamReader = new StreamReader(fileStream);
            var requestId = await ProcessFileDataLoadRequest(serviceProvider, s3Event.Object.Key, streamReader);
            await ProcessFileDataRequest(serviceProvider, requestId, streamReader, s3Event.Bucket.Name, s3Event.Object.Key);
            var outputBucketName = configuration["OputBucketName"];
            fileContent = await S3Client.GetObjectAsync(s3Event.Bucket.Name, s3Event.Object.Key);
            fileStream = fileContent.ResponseStream;
            streamReader = new StreamReader(fileStream);
            await WriteFileToS3(serviceProvider, streamReader, outputBucketName, "output_" + s3Event.Object.Key, requestId);
            return response.Headers.ContentType;
        }
        catch(Exception e)
        {
            context.Logger.LogInformation($"Error getting object {s3Event.Object.Key} from bucket {s3Event.Bucket.Name}. Make sure they exist and your bucket is in the same region as this function.");
            context.Logger.LogInformation(e.Message);
            context.Logger.LogInformation(e.StackTrace);
            throw;
        }
    }

    private async Task<Guid> ProcessFileDataLoadRequest(ServiceProvider serviceProvider, string fileName, StreamReader streamReader)
    {
        var logger = serviceProvider.GetRequiredService<ILogger>();
        logger.LogInformation($"Started processing request {fileName}");
        var fileDataLoader = serviceProvider.GetRequiredService<IFileDataLoader>();
        var requestId = await fileDataLoader.ProcessFile(fileName, streamReader);
        logger.LogInformation($"Completed processing request {fileName}");
        return requestId;
    }

    private async Task ProcessFileDataRequest(ServiceProvider serviceProvider, Guid requestId, StreamReader streamReader, string bucketName, string fileName)
    {
        var logger = serviceProvider.GetRequiredService<ILogger>();
        logger.LogInformation($"Started processing request {requestId}");
        var fileRequestProcessor = serviceProvider.GetRequiredService<FileRequestProcessor>();
        await fileRequestProcessor.ProcessRequest(requestId);
        logger.LogInformation($"Completed processing request {requestId}");
    }

    private async Task WriteFileToS3(ServiceProvider serviceProvider, StreamReader  streamReader, string bucketName, string fileName, Guid requestId)
    {
        var logger = serviceProvider.GetRequiredService<ILogger>();
        logger.LogInformation("started uplodated file");
        var fileWriter = serviceProvider.GetRequiredService<FileWriter>();
        MemoryStream memoryStream = await fileWriter.WriteFile(requestId, streamReader);
        await S3Client.UploadObjectFromStreamAsync(bucketName, "output_" + fileName, memoryStream, new Dictionary<string, object>());
        logger.LogInformation("Successfully uplodated file");
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
