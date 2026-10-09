using HCA.Api.Constants;

namespace HCA.Api.Providers
{
    public interface ISourceSystemProvider
    {
        IEnumerable<string> GetSourceSystem(dynamic request);
    }

    public static class SourceSystemProviderFactory
    { 
        private static  IDictionary<string, ISourceSystemProvider> _sourceSystemProvider;

        private static ISourceSystemProvider _emptySystemProvider;

        static SourceSystemProviderFactory()
        {
            _emptySystemProvider = new EmptySourceSystemProvider();

            _sourceSystemProvider = new Dictionary<string, ISourceSystemProvider>()
            {
                { EndPoints.PostIdentities, new PostSourceSystemProvider()},
                { EndPoints.LinkIdentities, new LinkSourceSystemProvider()},
                { EndPoints.UnLinkIdentities, new UnLinkSourceSystemProvider()},
                { EndPoints.MergeIdentities, new MergeSourceSystemProvider()},
                { EndPoints.UnMergeIdentities, new UnMergeSourceSystemProvider()},
            };
        }

        public static ISourceSystemProvider GetSourceSystemProvider(string endPoint)
        {
            if (string.IsNullOrWhiteSpace(endPoint))
                return _emptySystemProvider;

            if (_sourceSystemProvider.ContainsKey(endPoint))
                return _sourceSystemProvider[endPoint];

            return _emptySystemProvider;


        }
    }
}
