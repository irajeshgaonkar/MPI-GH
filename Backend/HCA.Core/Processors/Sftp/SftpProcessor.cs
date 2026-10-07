using HCA.Data.Entities;
using HCA.Data.Repository;
using HCA.Data;
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

public class SftpProcessor( AppSettings appSettings, IHcaSftpClient sftpClient, ISftpFileTransferRepository sftpFileTransferRepository, ISftpToS3FileTransferClient sftpToS3FileTransferClient, ITenantContext tenantContext ) : ISftpProcessor
{
    public async Task TransferFilesForProcessing()
    {
        foreach(var path in appSettings.SftpOptions.Paths)
        {
            await TransferFilesForProcessing(path);
        }
    }

    public async Task TransferFilesForProcessing(string path)
    {
        var tenantDatabase = GetTenantDatabaseFromPath(path);
        tenantContext.SetTenantDatabase(tenantDatabase);

        var filesToTransfer = new List<HcaSftpFile>();
        var filesInPath = sftpClient.ListDirectory($"{path}/{appSettings.SftpOptions.SourceFolder}");
        var filesInDatabase = (await sftpFileTransferRepository.GetTransferedFiles(path)).ToList();

        foreach(var file in filesInPath)
        {
            var fileExtension = GetFileExtension(file.Name);
            if( !appSettings.SftpOptions.AllowedFileTypes.Any( c => c == fileExtension ) )
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
        var tenantDatabase = GetTenantDatabaseFromPath(path);
        tenantContext.SetTenantDatabase(tenantDatabase);

        foreach(var file in files)
        {
            await sftpToS3FileTransferClient.TransferFile(file.FullName, appSettings.GetInputBucketName(tenantDatabase), file.Name, tenantDatabase.ToTenantValue());
            CreateFileTransferRequest(path, file, tenantDatabase);
        }
    }

    private void CreateFileTransferRequest(string path, HcaSftpFile file, TenantDatabaseKind tenantDatabase)
    {
        var sftpTransferEntity = new SftpFileTransferEntity()
        {
            Path = path,
            FileName = file.Name,
            TenantDatabase = tenantDatabase.ToTenantValue(),
            LastModified = file.LastModified,
            TransferDateTime = DateTime.Now,
            Status = RequestStatus.Success.GetStringValue()
        };

        sftpFileTransferRepository.AddAsync(sftpTransferEntity);
    }

    private static string GetFileExtension(string fileName)
    {
        return Path.GetExtension(fileName).ToLower();
    }

    private TenantDatabaseKind GetTenantDatabaseFromPath(string path)
    {
        var isConfiguredPath = StartsWithConfiguredPathPrefix(path, appSettings.SftpOptions.Paths);
        if (!isConfiguredPath)
        {
            throw new InvalidOperationException($"SFTP path '{path}' is not configured.");
        }

        var isNonCoalitionPath = StartsWithConfiguredPathPrefix(path, appSettings.SftpOptions.NonCoalitionPathPrefixes);
        return isNonCoalitionPath ? TenantDatabaseKind.NonCoalition : TenantDatabaseKind.Coalition;
    }

    private static bool StartsWithConfiguredPathPrefix(string path, IEnumerable<string>? configuredPrefixes)
    {
        var normalizedPath = NormalizePath(path);

        return configuredPrefixes?
            .Where(prefix => !string.IsNullOrWhiteSpace(prefix))
            .Select(NormalizePath)
            .Any(prefix => string.Equals(normalizedPath, prefix, StringComparison.OrdinalIgnoreCase)
                || normalizedPath.StartsWith($"{prefix}{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase)) == true;
    }

    private static string NormalizePath(string path)
    {
        return Path.TrimEndingDirectorySeparator(path
            .Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar)
            .TrimStart(Path.DirectorySeparatorChar)
            .Trim());
    }
}
