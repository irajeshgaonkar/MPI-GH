namespace HCA.Core.Processors;

public interface IProcessorProvider
{
    IPostIdentityProcessor PostIdentityProcessor { get; }
}
