namespace HCA.Infrastructure.Configurations
{
    public class AppSettings
    {
        public string DbConnectionStr { get; set; }

        public LoggingOptions Logging { get; set; }

        public string InputBucketName { get; set; }

        public string OutputBucketName { get; set; }

        public SqsOptions SqsOptions { get; set; }

        public SftpOptions SftpOptions { get; set; }

        public SecurityOptions SecurityOptions { get; set; }

        public VeratoOptions VeratoOptions { get; set; }
    }
}
