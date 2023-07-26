using System.Linq.Expressions;
using HCA.Data.Entities;
using HCA.Data.Repository.Core;
using HCA.Models.MuleSoft;

namespace HCA.Data.Repository;

public interface IClientIdentityRepository : IRepositoryBase<ClientIdentityEntity>
{
    Task<(int, IEnumerable<ClientIdentityEntity>)> GetAll(Dictionary<string, string> searchFilter, List<int>? userModifyRecords = null, int pageNumber = 0, int recordsPerPage = 10, string orderBy = "");

    Task<IEnumerable<ClientIdentityEntity>> GetAllByQuery(Expression<Func<ClientIdentityEntity, bool>> query);

    Task UpdateMpiLinkId(string sourceSystemName, string sourceSystemId, string newMpiLinkId);

    void UpdateMpiLinkId(ClientIdentityEntity clientIdentityEntity, string newMpiLinkId);

    Task<ClientIdentityEntity?> GetBySource(string sourceSystemName, string sourceSystemId);

    Task<int?> GetIdBySource(string sourceSystemName, string sourceSystemId);

    Task<ClientIdentityEntity?> Upsert(ClientIdentityEntity entity);

    Task<(int, Dictionary<string, IEnumerable<ClientIdentityEntity>>)> GetClientIdentityGroupedByLinkId(string linkId, int pageNumber = 0, int recordsPerPage = 10, string orderBy = "");

    Task DeleteMpiLinkId(string mpiLinkId);

    void DeleteClientIdentity(ClientIdentityEntity clientIdentityEntity);
}