using System;
using Amazon.SQS;
using Amazon.SQS.Model;
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
        private readonly SqsOptions _sqsOptions;

        private readonly IAmazonSQS _sqsClient;

        public SqsPublisher(SqsOptions sqsOptions)
        {
            _sqsOptions = sqsOptions;
            _sqsClient = new AmazonSQSClient();
        }

        public async Task PublishMessage(SqsMessage message)
        {
            if (message == null) return;
            var messageBody = SerializationExtensions.SerializeWithoutCasing(message);

            await _sqsClient.SendMessageAsync(_sqsOptions.QueueUrl, messageBody);
        }
    }
}

