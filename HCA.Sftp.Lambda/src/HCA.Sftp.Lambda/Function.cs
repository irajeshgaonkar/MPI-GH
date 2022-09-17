using System.Text.Json;
using Amazon.Lambda.Core;
using Amazon.Lambda.SQSEvents;
using Amazon.S3;
using HCA.Core;
using HCA.Core.Processors;
using HCA.Core.Processors.File;
using HCA.Data;
using HCA.Infrastructure;
using HCA.Infrastructure.Extensions;
using HCA.Infrastructure.Logger;
using HCA.Infrastructure.sftp;
using HCA.Models.SQS;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace HCA.Sftp.Lambda;

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
    public async Task FunctionHandler(string message, ILambdaContext context)
    {
        var configuration = ConfigureSettings();
        var serviceProvider = ConfigureServices(context, new ServiceCollection(), configuration);
        await SftpTest(serviceProvider, configuration);
    }

    async Task SftpTest(ServiceProvider serviceProvider, IConfiguration configuration)
    {
        //var sftpToS3FileTransferClient = serviceProvider.GetRequiredService<ISftpToS3FileTransferClient>();
        //await sftpToS3FileTransferClient.TransferFile("HCA/ProviderOne/Outbound/GP_SFTP_Test.csv", "mpi-batch-output-bucket", "GP_SFTP_Test.csv");

        var s3ToSftpFileTransferClient = serviceProvider.GetRequiredService<IS3ToSftpFileTransferClient>();
        await s3ToSftpFileTransferClient.TransferFile("mpi-batch-output-bucket", "GP_SFTP_Test.csv", "HCA/ProviderOne/Outbound/GP_SFTP_Test2.csv");
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

