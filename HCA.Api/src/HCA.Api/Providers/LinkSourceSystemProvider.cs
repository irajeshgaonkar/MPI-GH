using HCA.Models.MuleSoft;

namespace HCA.Api.Providers
{
    public class LinkSourceSystemProvider : ISourceSystemProvider
    {
        public IEnumerable<string> GetSourceSystem(dynamic request)
        {
            var req = request as IEnumerable<LinkingSources>;

            if (req == null)
                return Enumerable.Empty<string>();

            var linkToSource = req.Select(x => x.LinkToSource.Name).ToList();
            var SourceName = req.Select(x => x.Source.Name).ToList();

            linkToSource.AddRange(SourceName);
            return linkToSource;
        }
    }
}
