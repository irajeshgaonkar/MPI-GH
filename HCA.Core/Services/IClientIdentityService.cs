using HCA.Models;
using HCA.Models.MuleSoft;
using HCA.Models.MuleSoft.Response;

namespace HCA.Core.Services;

public interface IClientIdentityService
{
    Task<PagenatedCollection<ClientIdentity>> GetAll(int pageNumber, int recordsPerPage);

    Task<PagenatedCollection<ClientIdentity>> Search(int pageNumber, int recordsPerPage, IdentityFilter filter);

    Task<LinkIdentitiesResponseContent?> LinkIdentities(LinkingSources linkingSources);

    Task<UnLinkIdentitiesResponseContent?> UnLinkIdentities(UnLinkingSources unLinkingSources);

    Task<MergeIdentitiesResponseContent?> MergeIdentites(MergingSources mergingSources);

    Task<UnMergeIdentitiesResponseContent?> UnMergeIdentities(UnMergingSources unMergingSources);
}

