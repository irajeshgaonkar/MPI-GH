using HCA.Infrastructure.Logger;
using HCA.Infrastructure.Sftp;
using HCA.Models.Sftp;
using Renci.SshNet;
using Renci.SshNet.Sftp;

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
            _appLogger.LogInformation($"Started uploading file to path : {path}");
            using SftpClient sftpClient = CreateSftpClient();
            await UploadFileAsync(sftpClient, path, stream);
            _appLogger.LogInformation($"Completed uploading file to path : {path}");
        }

        public IEnumerable<HcaSftpFile> ListDirectory(string path)
        {
            var result = new List<HcaSftpFile>();
            using SftpClient sftpClient = CreateSftpClient();
            var files = sftpClient.ListDirectory(path);

            foreach(SftpFile file in files)
            {
                result.Add(new HcaSftpFile() { FullName = file.FullName, Name = file.Name, LastModified = file.LastWriteTime });
            }

            return result;
        }


        private Task DownloadFileAsync(SftpClient client, string path, MemoryStream memoryStream)
        {
            var task = Task.Factory.FromAsync(
                client.BeginDownloadFile(path, memoryStream, (l) => Console.WriteLine("Download completed")),
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
            _appLogger.LogInformation($"Connecting to Sftp {_sftpOptions.Host}, {_sftpOptions.UserName}");
            SftpClient sftpClient = new SftpClient(new PasswordConnectionInfo(_sftpOptions.Host, _sftpOptions.UserName, _sftpOptions.Password));
            sftpClient.Connect();
            _appLogger.LogInformation("Successfully connected to sftp");
            return sftpClient;
        }
    }
}
