namespace HCA.Infrastructure.sftp
{
    public class S3ToSftpFileTransferClient : IS3ToSftpFileTransferClient
    {
        private readonly IHcaS3Client _hcaS3Client;

        private readonly IHcaSftpClient _hcaSftpClient;

        public S3ToSftpFileTransferClient(IHcaS3Client hcaS3Client, IHcaSftpClient hcaSftpClient)
        {
            _hcaS3Client = hcaS3Client;
            _hcaSftpClient = hcaSftpClient;
        }

        public async Task TransferFile(string bucket, string fileName, string sftpPath)
        {
            var file = await _hcaS3Client.DownloadFileAsync(bucket, fileName);
            if (file == null) throw new HcaFileTransferException($"Error downloading the file from s3 bucket {bucket}, file name {fileName}");
            await _hcaSftpClient.UploadFileAsync(file, sftpPath);
        }
    }
}
