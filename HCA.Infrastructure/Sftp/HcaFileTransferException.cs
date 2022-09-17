namespace HCA.Infrastructure.sftp
{
    public class HcaFileTransferException : Exception
    {
        public HcaFileTransferException(string message) : base(message)
        { }
    }
}
