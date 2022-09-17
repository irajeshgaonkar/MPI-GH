namespace HCA.Infrastructure.sftp
{
    public class SftpToS3FileTransferClient : ISftpToS3FileTransferClient
    {
        private readonly IHcaS3Client _hcaS3Client;

        private readonly IHcaSftpClient _hcaSftpClient;

        public SftpToS3FileTransferClient(IHcaS3Client hcaS3Client, IHcaSftpClient hcaSftpClient)
        {
            _hcaS3Client = hcaS3Client;
            _hcaSftpClient = hcaSftpClient;
        }   

        public async Task TransferFile(string sftpPath, string bucket, string fileName)
        {
            var file = await _hcaSftpClient.DownloadFileAsync(sftpPath);
            if (file == null) throw new HcaFileTransferException($"Error downloading the file from sftp, path {sftpPath}");
            await _hcaS3Client.UploadFileAsync(file, bucket, fileName);
        }
    }
}
