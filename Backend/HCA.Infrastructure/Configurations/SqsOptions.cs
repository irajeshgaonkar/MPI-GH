namespace HCA.Infrastructure.Configurations
{
    public class SqsOptions
    {
        public string QueueUrl { get; set; }
        public string MessageGroupId { get; set; }
    }
}
