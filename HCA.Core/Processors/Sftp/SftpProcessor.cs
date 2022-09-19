using HCA.Data.Entities;
using HCA.Data.Repository;
using HCA.Infrastructure.Extensions;
using HCA.Infrastructure.sftp;
using HCA.Infrastructure.Sftp;
using HCA.Models;
using HCA.Models.Enums;
using HCA.Models.Sftp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HCA.Core.Processors.Sftp;

public interface ISftpProcessor
{
    Task TransferFilesForProcessing();
}

public class SftpProcessor : ISftpProcessor
{
    private readonly SftpOptions _sftpOptions;

    private readonly S3Options _s3Options;

    private readonly IHcaSftpClient _sftpClient;

    private readonly ISftpFileTransferRepository _sftpFileTransferRepository;

    private readonly ISftpToS3FileTransferClient _sftpToS3FileTransferClient;

    public SftpProcessor(SftpOptions sftpOptions, S3Options s3Options, IHcaSftpClient sftpClient, ISftpFileTransferRepository sftpFileTransferRepository, ISftpToS3FileTransferClient sftpToS3FileTransferClient)
    {
        _sftpOptions = sftpOptions;
        _sftpClient = sftpClient;
        _sftpFileTransferRepository = sftpFileTransferRepository;
        _sftpToS3FileTransferClient = sftpToS3FileTransferClient;
        _s3Options = s3Options;
    }

    public async Task TransferFilesForProcessing()
    {
        foreach(var path in _sftpOptions.Paths)
        {
            await TransferFilesForProcessing(path);
        }
    }

    public async Task TransferFilesForProcessing(string path)
    {
        var filesToTransfer = new List<HcaSftpFile>();
        var filesInPath = _sftpClient.ListDirectory($"{path}/{_sftpOptions.SourceFolder}");
        var filesInDatabase = (await _sftpFileTransferRepository.GetTransferedFiles(path)).ToList();

        foreach(var file in filesInPath)
        {
            var fileExtension = GetFileExtension(file.Name);
            if (!_sftpOptions.AllowedFileTypes.Any(c => c == fileExtension)) continue;
            
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
            await _sftpToS3FileTransferClient.TransferFile(file.FullName, _s3Options.InputBucketName, file.Name);
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
