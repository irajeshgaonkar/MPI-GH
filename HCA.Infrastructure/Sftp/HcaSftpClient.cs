using Amazon.S3;
using HCA.Infrastructure.Logger;
using HCA.Infrastructure.Sftp;
using Renci.SshNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HCA.Infrastructure.sftp
{
    public class HcaSftpClient : IHcaSftpClient
    {
        private readonly SftpOptions _sftpOptions;

        private readonly IAppLogger _appLogger;

        public HcaSftpClient(SftpOptions sftpOptions, IAppLogger appLogger)
        {
            _sftpOptions = sftpOptions;
            _appLogger = appLogger;
        }

        public async Task<MemoryStream> DownloadFileAsync(string path)
        {
            MemoryStream memoryStream = new MemoryStream();
            using SftpClient sftpClient = CreateSftpClient();
            await DownloadFileAsync(sftpClient, path, memoryStream);
            return memoryStream;
        }

        public async Task UploadFileAsync(MemoryStream stream, string path)
        {
            using SftpClient sftpClient = CreateSftpClient();
            await UploadFileAsync(sftpClient, path, stream);
        }


        private Task DownloadFileAsync(SftpClient client, string path, MemoryStream memoryStream)
        {
            var task = Task.Factory.FromAsync(
                client.BeginDownloadFile(path, memoryStream, (l) => Console.WriteLine("Downlaod completed")),
                ar =>
                {
                    try
                    {
                        client.EndDownloadFile(ar);
                    }
                    catch (Exception ex)
                    {
                        _appLogger.LogError(ex);
                    }
                });
            return task;
        }

        private Task UploadFileAsync(SftpClient client, string path, MemoryStream memoryStream)
        {
            var task = Task.Factory.FromAsync(
                client.BeginUploadFile(memoryStream, path, (l) => _appLogger.LogInformation($"Upload file completed to path {path}")),
                ar =>
                {
                    try
                    {
                        client.EndUploadFile(ar);
                    }
                    catch (Exception ex)
                    {
                        _appLogger.LogError(ex);
                    }
                });
            return task;
        }

        private SftpClient CreateSftpClient()
        {
            _appLogger.LogInformation($"Connecting to Sftp {_sftpOptions.Host}, {_sftpOptions.UserName}, {_sftpOptions.Password}");
            SftpClient sftpClient = new SftpClient(new PasswordConnectionInfo(_sftpOptions.Host, _sftpOptions.UserName, _sftpOptions.Password));
            sftpClient.Connect();
            _appLogger.LogInformation("Successfully connected to sftp");
            return sftpClient;
        }
    }
}
