using HCA.Models.Verato;

namespace HCA.Api.Providers
{
    public class UnMergeSourceSystemProvider : ISourceSystemProvider
    {
        public IEnumerable<string> GetSourceSystem(dynamic request)
        {
            var req = request as IEnumerable<UnMergingSources>;
            if (req == null) return Enumerable.Empty<string>();
           
            var unmergeFromSource = req.Select(x => x.UnmergeFromSource.Name).ToList();
            var UnmergeSource = req.Select(x => x.UnmergeSource.Name).ToList();

            unmergeFromSource.AddRange(UnmergeSource);
            return unmergeFromSource;
        }
    }
}
