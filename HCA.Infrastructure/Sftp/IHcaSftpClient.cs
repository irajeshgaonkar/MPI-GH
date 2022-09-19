using HCA.Models.Sftp;

namespace HCA.Infrastructure.sftp
{
    public interface IHcaSftpClient
    {
        Task<MemoryStream> DownloadFileAsync(string path);

        Task UploadFileAsync(MemoryStream stream, string path);

        IEnumerable<HcaSftpFile> ListDirectory(string path);
    }
}
