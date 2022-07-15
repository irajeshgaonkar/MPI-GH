using HCA.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace HCA.Data.Repository;

public class UserRequestRepository : IUserRequestRepository
{
    private readonly HcaDbContext _dbContext;
    private readonly IDataAdapter _dataAdapter;

    public UserRequestRepository(HcaDbContext dbContext, IDataAdapter dataAdapter)
    {
        _dbContext = dbContext;
        _dataAdapter = dataAdapter;
    }

    public async Task<UserRequestEntity?> GetRequest(Guid requestId)
    {
        var requests = _dbContext.UserRequests.SingleOrDefault(c => c.RequestId == requestId);
        return await Task.FromResult(requests);
    }

    public async Task<UserRequestEntity> Insert(UserRequestEntity entity)
    {
        _dbContext.UserRequests.Add(entity);
        await _dbContext.SaveChangesAsync();
        return entity;
    }

    public async Task<UserRequestEntity> Update(UserRequestEntity entity)
    {
        _dbContext.Entry(entity).State = EntityState.Modified;
        await _dbContext.SaveChangesAsync();
        return entity;
    }
}

