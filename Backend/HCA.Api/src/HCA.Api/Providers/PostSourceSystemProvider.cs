using HCA.Models.Request;

namespace HCA.Api.Providers
{
    public class PostSourceSystemProvider : ISourceSystemProvider
    {
        public IEnumerable<string> GetSourceSystem(dynamic request)
        {
            var req = request as IEnumerable<ClientIdentityRequest>;

            if(req == null)
                return Enumerable.Empty<string>();  

            return req.Select(x => x.SourceSystemName).ToList();
        }
    }

    public class EmptySourceSystemProvider : ISourceSystemProvider
    {
        public IEnumerable<string> GetSourceSystem(dynamic request)
        {
            return Enumerable.Empty<string>();
        }
    }
}
