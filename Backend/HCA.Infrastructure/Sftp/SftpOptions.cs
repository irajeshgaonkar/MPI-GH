namespace HCA.Infrastructure.Sftp;

public class SftpOptions
{
    public string Host { get; set; }

    public string UserName { get; set; }

    public string Password { get; set; }

    public string[] Paths { get; set; }

    public string SourceFolder { get; set; }

    public string DestinationFolder { get; set; }

    public string[] AllowedFileTypes { get; set; }
}
