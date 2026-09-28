using HCA.Core.Services;
using HCA.Data.Repository;
using HCA.Infrastructure.Configurations;
using HCA.Infrastructure.Logger;
using HCA.Infrastructure.S3;
using HCA.Infrastructure.sftp;
namespace HCA.Core.Processors.File;

public class OutputFileWriter : IOutputFileWriter
{
    private readonly IAppLogger _logger;
    private readonly AppSettings _appSettings;
    private readonly IFileWriter _fileWriter;
    private readonly IFileRequestService _fileRequestService;
    private readonly IHcaS3Client _s3Client;
    private readonly IS3ToSftpFileTransferClient _s3ToSftpFileTransferClient;
    private readonly ISftpFileTransferRepository _sftpFileTransferRepository;

    public OutputFileWriter(
        IAppLogger logger,
        IHcaS3Client s3Client,
        AppSettings appSettings,
        IFileWriter fileWriter,
        IFileRequestService fileRequestService,
        IS3ToSftpFileTransferClient s3ToSftpFileTransferClient,
        ISftpFileTransferRepository sftpFileTransferRepository)
    {
        _logger = logger;
        _fileWriter = fileWriter;
        _fileRequestService = fileRequestService;
        _s3Client = s3Client;
        _s3ToSftpFileTransferClient = s3ToSftpFileTransferClient;
        _sftpFileTransferRepository = sftpFileTransferRepository;
        _appSettings = appSettings;
    }

    public async Task WriteFile(string requestId)
    {
        var fileRequestEntity = await _fileRequestService.UpdatefileRequestComplete(requestId);

        if (fileRequestEntity != null)
        {
            var memoryStream = await _fileWriter.WriteFile(fileRequestEntity);
            await WriteToS3(memoryStream, _appSettings.OutputBucketName, fileRequestEntity.OutputFileName!);
            await TransferFileToSftp(fileRequestEntity.FileName, fileRequestEntity.OutputFileName!);
        }
    }

    public async Task WriteToS3(MemoryStream memoryStream, string bucketName, string fileName)
    {
        try
        {
            _logger.LogInformation($"Started writing file to s3 bucketName: {bucketName} fileName: {fileName}");
            await _s3Client.UploadFileAsync(memoryStream, _appSettings.OutputBucketName, fileName);
            _logger.LogInformation($"Successfully completed writing file to s3 bucketName: {bucketName} fileName: {fileName}");
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error Processing the request");
            throw;
        }
    }

    public async Task TransferFileToSftp(string inputFileName, string fileName)
    {
        try
        {
            var path = _sftpFileTransferRepository.GetSingle(t => t.FileName == inputFileName)?.Path;

            if (path == null)
            {
                _logger.LogInformation($"Cannot transfer file to sftp, path is null for inputFile {inputFileName}");
                return;
            }

            var outputPath = $"{path}/{_appSettings.SftpOptions.DestinationFolder}/{fileName}";
            await _s3ToSftpFileTransferClient.TransferFile(_appSettings.OutputBucketName, fileName, outputPath);
        }
        catch(Exception e)
        {
            _logger.LogError(e);
            _logger.LogInformation($"Error transferring the file {inputFileName}");
        }
    }
}
