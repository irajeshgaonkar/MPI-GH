using HCA.Core.Services;
using HCA.Data;
using HCA.Data.Repository;
using HCA.Infrastructure.Configurations;
using HCA.Infrastructure.Logger;
using HCA.Infrastructure.S3;
using HCA.Infrastructure.sftp;
namespace HCA.Core.Processors.File;

public class OutputFileWriter(
    IAppLogger logger,
    IHcaS3Client s3Client,
    AppSettings appSettings,
    IFileWriter fileWriter,
    IFileRequestService fileRequestService,
    IS3ToSftpFileTransferClient s3ToSftpFileTransferClient,
    ISftpFileTransferRepository sftpFileTransferRepository ) : IOutputFileWriter
{
    public async Task WriteFile(string requestId)
    {
        var fileRequestEntity = await fileRequestService.UpdatefileRequestComplete(requestId);

        if (fileRequestEntity != null)
        {
            var tenantDatabase = GetTenantDatabase(fileRequestEntity.TenantDatabase);
            var outputBucketName = appSettings.GetOutputBucketName(tenantDatabase);
            var memoryStream = await fileWriter.WriteFile(fileRequestEntity);
            await WriteToS3(memoryStream, outputBucketName, fileRequestEntity.OutputFileName!);
            await TransferFileToSftp(fileRequestEntity.FileName, fileRequestEntity.OutputFileName!, outputBucketName);
        }
    }

    public async Task WriteToS3(MemoryStream memoryStream, string bucketName, string fileName)
    {
        try
        {
            logger.LogInformation($"Started writing file to s3 bucketName: {bucketName} fileName: {fileName}");
            await s3Client.UploadFileAsync(memoryStream, bucketName, fileName);
            logger.LogInformation($"Successfully completed writing file to s3 bucketName: {bucketName} fileName: {fileName}");
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error Processing the request");
            throw;
        }
    }

    public async Task TransferFileToSftp(string inputFileName, string fileName, string outputBucketName)
    {
        try
        {
            var path = sftpFileTransferRepository.GetSingle(t => t.FileName == inputFileName)?.Path;

            if (path == null)
            {
                logger.LogInformation($"Cannot transfer file to sftp, path is null for inputFile {inputFileName}");
                return;
            }

            var outputPath = $"{path}/{appSettings.SftpOptions.DestinationFolder}/{fileName}";
            await s3ToSftpFileTransferClient.TransferFile(outputBucketName, fileName, outputPath);
        }
        catch(Exception e)
        {
            logger.LogError(e);
            logger.LogInformation($"Error transferring the file {inputFileName}");
        }
    }

    private static TenantDatabaseKind GetTenantDatabase(string? tenantDatabase)
        => TenantDatabaseKindExtensions.TryParseTenantValue(tenantDatabase, out var parsedTenantDatabase)
            ? parsedTenantDatabase
            : TenantDatabaseKind.Coalition;
}
