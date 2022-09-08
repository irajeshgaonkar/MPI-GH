namespace HCA.Core.Processors;

public interface IClientIdentitySQSPublisher
{
    Task Publish(string requestId);
}
