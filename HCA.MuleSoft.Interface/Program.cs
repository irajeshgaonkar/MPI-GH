// See https://aka.ms/new-console-template for more information
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using HCA.Data;
using HCA.MuleSoft;
using HCA.Infrastructure;
using HCA.Core;
using HCA.Core.Processors;
using System.Text.Json;
using HCA.Infrastructure.Logger;
using HCA.Core.Processors.File;

Console.WriteLine("Started Process for request Id: ");

var operationType = 'f';

var configuration = ConfigureSettings();
var serviceProvider = ConfigureServices(new ServiceCollection(), configuration);

switch (operationType)
{
    case 'L':
    case 'l':
        await LoadFileData(serviceProvider, "C:\\Data\\", "CSV_test_data_1.csv");
        break;

    case 'F':
    case 'f':
        await ProcessFileDataRequest(serviceProvider, 10);
        break;

    default:
        break;
}

//var fileRequestProcessor = serviceProvider.GetRequiredService<FileRequestProcessor>();
//await fileRequestProcessor.ProcessRequest(new Guid("50fb76f7-e786-48ad-a7f4-650413c284fe"));

async Task LoadFileData(ServiceProvider serviceProvider, string fileLocation, string fileName)
{
    var fileProcessor = serviceProvider.GetRequiredService<IFileProcessor>();


    using (var sr = new StreamReader($"{fileLocation}{fileName}"))
    {
        await fileProcessor.ProcessFile(fileName, sr);
    }
}

async Task ProcessFileDataRequest(ServiceProvider serviceProvider, int requestId)
{
    var logger = serviceProvider.GetRequiredService<IAppLogger>();
    logger.LogInformation($"Started processing request {requestId}");
    var fileRequestProcessor = serviceProvider.GetRequiredService<IFileRequestProcessor>();
    await fileRequestProcessor.ProcessRequest(requestId);
    logger.LogInformation($"Completed processing request {requestId}");
}

ServiceProvider ConfigureServices(IServiceCollection services, IConfiguration configuration)
{
    var serviceProvider = services
                            .AddConsoleLogging()
                            .AddDbContext(configuration)
                            .AddRepositories()
                            .AddMuleSoft(configuration)
                            .AddServices()
                            .AddAutoMapper()
                            .AddFileProcessors()
                            .BuildServiceProvider();

    return serviceProvider;
}

IConfiguration ConfigureSettings()
{
    var configuration = new ConfigurationBuilder()
        .AddJsonFile("appSettings.json", optional: true)
        .Build();

    return configuration;
}


public class RequestModel
{
    public string OperationType { get; set; }

    public string PayLoad { get; set; }
}

public class DataLoadRequestPayload
{
    public string BucketName { get; set; }

    public string FileName { get; set; }
}

