namespace HCA.Infrastructure.sftp
{
    public interface IHcaS3Client
    {
        Task<MemoryStream> DownloadFileAsync(string bucketName, string fileName);

        Task UploadFileAsync(MemoryStream stream, string bucketName, string fileName);
    }
}
