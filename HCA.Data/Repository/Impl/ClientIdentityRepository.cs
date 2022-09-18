using System.Linq.Expressions;
using HCA.Data.Entities;
using HCA.Data.Repository.Core;
using HCA.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;

namespace HCA.Data.Repository;

public class ClientIdentityRepository : RepositoryBase<ClientIdentityEntity>, IClientIdentityRepository
{
    public ClientIdentityRepository(HcaDbContext dbContext) : base(dbContext)
    {
    }

    public async Task<(int, IEnumerable<ClientIdentityEntity>)> GetAll(string searchBy = "", string searchValue = "", List<int>? userModifyRecords = null, int pageNumber = 0, int recordsPerPage = 10, string orderBy = "")
    {
        if (userModifyRecords == null) userModifyRecords = new List<int>();
        var skip = pageNumber * recordsPerPage;
        Expression<Func<ClientIdentityEntity, bool>> searchQuery = (c) => c.IsActive == true && !userModifyRecords.Contains(c.Id);

        if (searchValue.IsNotEmpty())
        {
            searchValue = searchValue.ToLower();
            if (searchBy == "FName") searchQuery = (c) => c.IsActive == true && c.FirstName.ToLower().StartsWith(searchValue) && !userModifyRecords.Contains(c.Id);
            if (searchBy == "LName") searchQuery = (c) => c.IsActive == true && c.LastName.ToLower().StartsWith(searchValue) && !userModifyRecords.Contains(c.Id);
            if (searchBy == "Ssn") searchQuery = (c) => c.IsActive == true && c.Ssn != null && c.Ssn.ToLower().StartsWith(searchValue) && !userModifyRecords.Contains(c.Id);
            if (searchBy == "Email") searchQuery = (c) => c.IsActive == true && c.Communications.FirstOrDefault(cc => cc.EmailAddress.ToLower().StartsWith(searchValue)) != null && !userModifyRecords.Contains(c.Id);
        }

        var orderByQuery = OrderBy(orderBy) ?? DefaultOrderBy;
        var clientIdentityEntities = await GetAllAsync(searchQuery, ClientIdentitiesInclude, orderByQuery, skip, recordsPerPage);
        var count = Count(searchQuery);
        return (count, clientIdentityEntities.ToList());
    }

    public async Task<IEnumerable<ClientIdentityEntity>> GetAllByQuery(Expression<Func<ClientIdentityEntity, bool>> query)
    {
        var clientIdentityEntities = await GetAllAsync(query, ClientIdentitiesInclude, DefaultOrderBy);
        return clientIdentityEntities.ToList();
    }

    public async Task UpdateMpiLinkId(string sourceSystemName, string sourceSystemId, string newMpiLinkId)
    {
        var clientIdenty = await GetBySource(sourceSystemName, sourceSystemId);
        if (null == clientIdenty) return;
        UpdateMpiLinkId(clientIdenty, newMpiLinkId);
    }

    public async Task<ClientIdentityEntity?> GetBySource(string sourceSystemName, string sourceSystemId)
    {
        var identity = await GetSingleAsync(SourceSystemFilter(sourceSystemName, sourceSystemId), ClientIdentitiesInclude);
        return identity;
    }

    private Func<IQueryable<ClientIdentityEntity>, IOrderedQueryable<ClientIdentityEntity>>? OrderBy(string orderByStr)
    {
        Func<IQueryable<ClientIdentityEntity>, IOrderedQueryable<ClientIdentityEntity>> orderBy = null;

        var orderByParts = orderByStr.Split(" ");
        string columnName = orderByParts[0].Trim();
        string orderByType = orderByParts.Length > 1 ? orderByParts[1].Trim() : string.Empty;

        if (orderByType == string.Empty || orderByType.ToLower() == "asc")
        {
            if (columnName == "MpiLinkId") orderBy = c => c.OrderBy(i => i.MpiLinkId);
            if (columnName == "SourceSystemId") orderBy = c => c.OrderBy(i => i.SourceSystemId);
            if (columnName == "SourceSystemName") orderBy = c => c.OrderBy(i => i.SourceSystemName);
            if (columnName == "SourceSystemUpdated") orderBy = c => c.OrderBy(i => i.SourceSystemUpdated);
            if (columnName == "FirstName") orderBy = c => c.OrderBy(i => i.FirstName);
            if (columnName == "MiddleName") orderBy = c => c.OrderBy(i => i.MiddleName);
            if (columnName == "LastName") orderBy = c => c.OrderBy(i => i.LastName);
            if (columnName == "NameSuffix") orderBy = c => c.OrderBy(i => i.NameSuffix);
            if (columnName == "Dob") orderBy = c => c.OrderBy(i => i.Dob);
            if (columnName == "Gender") orderBy = c => c.OrderBy(i => i.Gender);
            if (columnName == "Ssn") orderBy = c => c.OrderBy(i => i.Ssn);
        }
        else
        {
            if (columnName == "MpiLinkId") orderBy = c => c.OrderByDescending(i => i.MpiLinkId);
            if (columnName == "SourceSystemId") orderBy = c => c.OrderByDescending(i => i.SourceSystemId);
            if (columnName == "SourceSystemName") orderBy = c => c.OrderByDescending(i => i.SourceSystemName);
            if (columnName == "SourceSystemUpdated") orderBy = c => c.OrderByDescending(i => i.SourceSystemUpdated);
            if (columnName == "FirstName") orderBy = c => c.OrderByDescending(i => i.FirstName);
            if (columnName == "MiddleName") orderBy = c => c.OrderByDescending(i => i.MiddleName);
            if (columnName == "LastName") orderBy = c => c.OrderByDescending(i => i.LastName);
            if (columnName == "NameSuffix") orderBy = c => c.OrderByDescending(i => i.NameSuffix);
            if (columnName == "Dob") orderBy = c => c.OrderByDescending(i => i.Dob);
            if (columnName == "Gender") orderBy = c => c.OrderByDescending(i => i.Gender);
            if (columnName == "Ssn") orderBy = c => c.OrderByDescending(i => i.Ssn);
        }

        return orderBy;
    }



    private Func<IQueryable<ClientIdentityEntity>, IOrderedQueryable<ClientIdentityEntity>> DefaultOrderBy = c => c.OrderByDescending(c => c.UpdatedDate);

    private IIncludableQueryable<ClientIdentityEntity, object> ClientIdentitiesInclude(IQueryable<ClientIdentityEntity> clientIdentities) =>
        clientIdentities
            .Include(c => c.Addresses)
            .ThenInclude(com => com.AddressCommunications)
            .ThenInclude(ac => ac.Communication);

    private Expression<Func<ClientIdentityEntity, bool>> SourceSystemFilter(string sourceSystemName, string sourceSystemId)
        => c => c.SourceSystemName == sourceSystemName && c.SourceSystemId == sourceSystemId && c.IsActive == true;

    public async Task<ClientIdentityEntity?> Upsert(ClientIdentityEntity entity)
    {
        var identity = await GetBySource(entity.SourceSystemName, entity.SourceSystemId);

        if (identity == null)
        {
            AddAsync(entity);
            return entity;
        }

        identity.MpiLinkId = entity.MpiLinkId;
        identity.FirstName = entity.FirstName;
        identity.LastName = entity.LastName;
        identity.MiddleName = entity.MiddleName;
        identity.NameSuffix = entity.NameSuffix;
        identity.Ssn = entity.Ssn;
        identity.Dob = entity.Dob;
        identity.Gender = entity.Gender;
        identity.ProtectedPopulationFlag = entity.ProtectedPopulationFlag;
        identity.ProtectedPopulationType = entity.ProtectedPopulationType;
        identity.SourceSystemUpdated = entity.SourceSystemUpdated;
        identity.UpdatedBy = entity.UpdatedBy;
        identity.UpdatedDate = entity.UpdatedDate;

        Update(identity);
        return identity;
    }

    public void UpdateMpiLinkId(ClientIdentityEntity clientIdentityEntity, string newMpiLinkId)
    {
        clientIdentityEntity.MpiLinkId = newMpiLinkId;
        foreach (var address in clientIdentityEntity.Addresses) address.MpiLinkId = newMpiLinkId;
        foreach (var communicaiton in clientIdentityEntity.Communications) communicaiton.MpiLinkId = newMpiLinkId;
        Update(clientIdentityEntity);
    }

    public async Task<int?> GetIdBySource(string sourceSystemName, string sourceSystemId)
    {
        var identity = await GetSingleAsync(SourceSystemFilter(sourceSystemName, sourceSystemId));
        return identity?.Id;
    }

    private Expression<Func<ClientIdentityEntity, bool>> ActiveFilter
        => c => c.IsActive == true;
}

