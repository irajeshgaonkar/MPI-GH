using HCA.Data.Entities;
using HCA.Data.Repository.Core;

namespace HCA.Data.Repository;

public interface IUserRequestRepository : IRepositoryBase<UserRequestEntity>
{
    Task<(int, IEnumerable<UserRequestEntity>)> GetAll(string? userName, int skip = 0, int recordsPerPage = 20, string orderBy = "");
}