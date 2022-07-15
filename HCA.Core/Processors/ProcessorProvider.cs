namespace HCA.Core.Processors;

public class ProcessorProvider : IProcessorProvider
{
    private readonly IPostIdentityProcessor _postIdentityProcessor;

    public ProcessorProvider(IPostIdentityProcessor postIdentityProcessor)
    {
        _postIdentityProcessor = postIdentityProcessor;
    }

    public IPostIdentityProcessor PostIdentityProcessor => _postIdentityProcessor;
}
