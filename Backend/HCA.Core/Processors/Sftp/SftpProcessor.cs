using HCA.Data.Entities;
using HCA.Data.Repository;
using HCA.Infrastructure.Configurations;
using HCA.Infrastructure.Extensions;
using HCA.Infrastructure.sftp;
using HCA.Models.Enums;
using HCA.Models.Sftp;

namespace HCA.Core.Processors.Sftp;

public interface ISftpProcessor
{
    Task TransferFilesForProcessing();
}

public class SftpProcessor : ISftpProcessor
{
    private readonly AppSettings _appSettings;

    private readonly IHcaSftpClient _sftpClient;

    private readonly ISftpFileTransferRepository _sftpFileTransferRepository;

    private readonly ISftpToS3FileTransferClient _sftpToS3FileTransferClient;

    public SftpProcessor(AppSettings appSettings, IHcaSftpClient sftpClient, ISftpFileTransferRepository sftpFileTransferRepository, ISftpToS3FileTransferClient sftpToS3FileTransferClient)
    {
        _appSettings = appSettings;
        _sftpClient = sftpClient;
        _sftpFileTransferRepository = sftpFileTransferRepository;
        _sftpToS3FileTransferClient = sftpToS3FileTransferClient;
    }

    public async Task TransferFilesForProcessing()
    {
        foreach(var path in _appSettings.SftpOptions.Paths)
        {
            await TransferFilesForProcessing(path);
        }
    }

    public async Task TransferFilesForProcessing(string path)
    {
        var filesToTransfer = new List<HcaSftpFile>();
        var filesInPath = _sftpClient.ListDirectory($"{path}/{_appSettings.SftpOptions.SourceFolder}");
        var filesInDatabase = (await _sftpFileTransferRepository.GetTransferedFiles(path)).ToList();

        foreach(var file in filesInPath)
        {
            var fileExtension = GetFileExtension(file.Name);
            if( !_appSettings.SftpOptions.AllowedFileTypes.Any( c => c == fileExtension ) )
            {
                continue;
            }

            if (!filesInDatabase.Any(f => f.FileName == file.Name))
            {
                filesToTransfer.Add(file);
            }
        }

        await TransferFilesForProcessing(path, filesToTransfer);
    }

    public async Task TransferFilesForProcessing(string path, IEnumerable<HcaSftpFile> files)
    {
        foreach(var file in files)
        {
            await _sftpToS3FileTransferClient.TransferFile(file.FullName, _appSettings.InputBucketName, file.Name);
            CreateFileTransferRequest(path, file);
        }
    }

    private void CreateFileTransferRequest(string path, HcaSftpFile file)
    {
        var sftpTransferEntity = new SftpFileTransferEntity()
        {
            Path = path,
            FileName = file.Name,
            LastModified = file.LastModified,
            TransferDateTime = DateTime.Now,
            Status = RequestStatus.Success.GetStringValue()
        };

        _sftpFileTransferRepository.AddAsync(sftpTransferEntity);
    }

    private string GetFileExtension(string fileName)
    {
        return Path.GetExtension(fileName).ToLower();
    }
}
