namespace HCA.Infrastructure.sftp
{
    public interface ISftpToS3FileTransferClient
    {
        Task TransferFile(string sftpPath, string bucket, string fileName);
    }
}
