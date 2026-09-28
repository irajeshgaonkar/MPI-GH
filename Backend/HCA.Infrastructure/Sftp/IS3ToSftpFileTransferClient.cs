namespace HCA.Infrastructure.sftp
{
    public interface IS3ToSftpFileTransferClient
    {
        Task TransferFile(string bucket, string fileName, string sftpPath);
    }
}
