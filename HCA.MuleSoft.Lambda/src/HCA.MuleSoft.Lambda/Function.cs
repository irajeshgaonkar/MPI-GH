using Amazon.Lambda.Core;
using Amazon.Lambda.SQSEvents;
using Amazon.S3;
using HCA.Core;
using HCA.Core.Processors;
using HCA.Core.Processors.File;
using HCA.Infrastructure.Extensions;
using HCA.Infrastructure.Logger;
using HCA.Models.SQS;
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
    public async Task FunctionHandler(SQSEvent evnt, ILambdaContext context)
    {
        var configuration = ConfigureSettings();
        var serviceProvider = ConfigureServices(context, new ServiceCollection(), configuration);

        foreach(var message in evnt.Records)
        {
            await ProcessMessage(serviceProvider, message);
        }
    }

    private async Task ProcessMessage(ServiceProvider serviceProvider, SQSEvent.SQSMessage message)
    {
        var logger = serviceProvider.GetRequiredService<IAppLogger>();

        var sqsMessage = SerializationExtensions.DeSerializeWithoutCasing<SqsMessage>(message.Body);

        if(sqsMessage == null)
        {
            logger.LogInformation($"Sqs message is empty");
            return;
        }

        logger.LogInformation($"Started Processing FunctionHandler {sqsMessage.MessageType}");

        if (sqsMessage.MessageType == MessageType.BatchProcess)
        {
            await ProcessBatchRequest(serviceProvider, sqsMessage);
            return;
        }

        if(sqsMessage.MessageType == MessageType.GenerateOutput)
        {
            await GenerateOutputFile(serviceProvider, sqsMessage);
            return;
        }
    }

    private async Task GenerateOutputFile(ServiceProvider serviceProvider, SqsMessage request)
    {
        var logger = serviceProvider.GetRequiredService<IAppLogger>();
        logger.LogInformation($"started processing request {request.MessageType}");
        var outputFileWriter = serviceProvider.GetRequiredService<IOutputFileWriter>();
        var requestData = SerializationExtensions.DeSerializeWithoutCasing<OuputFileGenerationMessage>(request.Payload);

        if (requestData == null)
        {
            logger.LogInformation($"request data is null for {request.MessageType}");
            return;
        }

        await outputFileWriter.WriteFile(requestData.RequestId);
    }

    private async Task ProcessBatchRequest(ServiceProvider serviceProvider, SqsMessage request)
    {
        var logger = serviceProvider.GetRequiredService<IAppLogger>();
        logger.LogInformation($"started processing request {request.MessageType}");
        var batchRequestProcessor = serviceProvider.GetRequiredService<IBatchRequestProcessor>();
        var requestData = SerializationExtensions.DeSerializeWithoutCasing<BatchProcessMessage>(request.Payload);

        if(requestData == null)
        {
            logger.LogInformation($"request data is null for {request.MessageType}");
            return;
        }

        await batchRequestProcessor.ProcessRequest(requestData);
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

