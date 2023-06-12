using HCA.Models.MuleSoft;

namespace HCA.Api.Providers
{
    public class MergeSourceSystemProvider : ISourceSystemProvider
    {
        public IEnumerable<string> GetSourceSystem(dynamic request)
        {
            var req = request as IEnumerable<MergingSources>;
            if (req == null)
                return Enumerable.Empty<string>();

            var sources = req.Select(x => x.ToSurviveSource.Name).ToList();
            var retireSource = req.Select(x => x.ToRetireSource.Name).ToList();

            sources.AddRange(retireSource);
            return sources;
        }
    }
}
