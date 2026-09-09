using HCA.Models.Verato;

namespace HCA.Api.Providers
{
    public class UnLinkSourceSystemProvider : ISourceSystemProvider
    {
        public IEnumerable<string> GetSourceSystem(dynamic request)
        {
            var req = request as IEnumerable<UnLinkingSources>;

            if (req == null)
                return Enumerable.Empty<string>();

            var unLinkSource = req.Select(x => x.UnlinkFromSource.Name).ToList();
            var sourceName = req.Select(x => x.Source.Name).ToList();

            unLinkSource.AddRange(sourceName);
            return unLinkSource;
        }
    }
}
