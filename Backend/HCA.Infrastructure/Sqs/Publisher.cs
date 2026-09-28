using Amazon.SQS;
using HCA.Infrastructure.Configurations;
using HCA.Infrastructure.Extensions;
using HCA.Models.SQS;

namespace HCA.Infrastructure.Sqs
{
    public class SqsOptions
    {
        public string QueueUrl { get; set; }

        public string MessageGroupId { get; set; }
    }

    public interface ISqsPublisher
    {
        Task PublishMessage(SqsMessage message);
    }

    public class SqsPublisher : ISqsPublisher
    {
        private readonly IAmazonSQS _sqsClient;
        private readonly AppSettings _appSettings;

        public SqsPublisher(AppSettings appSettings)
        {
            _appSettings = appSettings;
            _sqsClient = new AmazonSQSClient();
        }

        public async Task PublishMessage(SqsMessage message)
        {
            if (message == null) return;
            var messageBody = SerializationExtensions.SerializeWithoutCasing(message);

            await _sqsClient.SendMessageAsync(_appSettings.SqsOptions.QueueUrl, messageBody);
        }
    }
}

