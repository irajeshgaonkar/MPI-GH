using HCA.Data.Entities;
using HCA.Data.Repository.Core;
using System.Linq.Expressions;

namespace HCA.Data.Repository;

public class UserRequestRepository : RepositoryBase<UserRequestEntity>, IUserRequestRepository
{
    public UserRequestRepository(HcaDbContext dbContext) : base(dbContext)
    {
    }

    public async Task<(int, IEnumerable<UserRequestEntity>)> GetAll(string? userName, int pageNumber = 0, int recordsPerPage = 20, string orderBy = "")
    {
        Expression<Func<UserRequestEntity, bool>>? searchQuery = null;
        var skip = pageNumber * recordsPerPage;
        int count = 0;

        if (userName != null)
        {
            searchQuery = (c) => c.UserName.ToLower().StartsWith(userName.ToLower());
        }

        List<UserRequestEntity> result = (await GetAllAsync(searchQuery, null, DefaultOrderBy, skip, recordsPerPage)).ToList();
        count = searchQuery == null ? Count() : Count(searchQuery);
        return (count, result);
    }

    private Func<IQueryable<UserRequestEntity>, IOrderedQueryable<UserRequestEntity>> DefaultOrderBy = c => c.OrderByDescending(c => c.RequestDateTime);
}