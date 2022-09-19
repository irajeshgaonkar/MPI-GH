using HCA.Core.Services;
using HCA.Data.Repository;
using HCA.Infrastructure.Logger;
using HCA.Infrastructure.S3;
using HCA.Infrastructure.sftp;
using HCA.Infrastructure.Sftp;
using HCA.Models;
namespace HCA.Core.Processors.File;

public class OutputFileWriter : IOutputFileWriter
{
    private readonly IAppLogger _logger;
    private readonly IFileWriter _fileWriter;
    private readonly IFileRequestService _fileRequestService;
    private readonly IHcaS3Client _s3Client;
    private readonly S3Options _s3Options;
    private readonly IHcaSftpClient _hcaSftpClient;
    private readonly SftpOptions _sftpOptions;
    private readonly ISftpFileTransferRepository _sftpFileTransferRepository;

    public OutputFileWriter(IAppLogger logger, IHcaS3Client s3Client, S3Options s3Options, SftpOptions sftpOptions,
        IFileWriter fileWriter, IFileRequestService fileRequestService, IHcaSftpClient hcaSftpClient, ISftpFileTransferRepository sftpFileTransferRepository)
    {
        _logger = logger;
        _fileWriter = fileWriter;
        _fileRequestService = fileRequestService;
        _s3Client = s3Client;
        _s3Options = s3Options;
        _hcaSftpClient = hcaSftpClient;
        _sftpOptions = sftpOptions;
        _sftpFileTransferRepository = sftpFileTransferRepository;
    }

    public async Task WriteFile(string requestId)
    {
        var fileRequestEntity = await _fileRequestService.UpdatefileRequestComplete(requestId);

        if (fileRequestEntity != null)
        {
            var memoryStream = await _fileWriter.WriteFile(fileRequestEntity);
            await WriteToS3(memoryStream, _s3Options.OutputBucketName, fileRequestEntity.OutputFileName!);
            await TransferFileToSftp(memoryStream, fileRequestEntity.FileName, fileRequestEntity.OutputFileName!);
        }
    }

    public async Task WriteToS3(MemoryStream memoryStream, string bucketName, string fileName)
    {
        try
        {
            _logger.LogInformation($"Started writing file to s3 bucketName: {bucketName} fileName: {fileName}");
            await _s3Client.UploadFileAsync(memoryStream, _s3Options.OutputBucketName, fileName);
            _logger.LogInformation($"Successfully completed writing file to s3 bucketName: {bucketName} fileName: {fileName}");
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error Processing the request");
            throw;
        }
    }

    public async Task TransferFileToSftp(MemoryStream memoryStream, string inputFileName, string fileName)
    {
        var path = _sftpFileTransferRepository.GetSingle(t => t.FileName == inputFileName)?.Path;
        
        if(path == null)
        {
            _logger.LogInformation($"Cannot transfer file to sftp, path is null for inputFile {inputFileName}");

        }
        memoryStream.Seek(0, SeekOrigin.Begin);
        var outputPath = $"{_sftpOptions.DestinationFolder}/{fileName}";
        await _hcaSftpClient.UploadFileAsync(memoryStream, outputPath);
    }
}


