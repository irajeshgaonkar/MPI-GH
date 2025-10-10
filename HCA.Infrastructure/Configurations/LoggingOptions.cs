namespace HCA.Infrastructure.Configurations
{
    public class LoggingOptions
    {
        public LogLevelOptions LogLevel { get; set; }
    }

    public class LogLevelOptions
    {
        public string Default { get; set; }
    }
}
