using System.Linq.Expressions;
using HCA.Data.Entities;
using HCA.Data.Repository.Core;
using HCA.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Query;

namespace HCA.Data.Repository;


public class ClientIdentityRepository : RepositoryBase<ClientIdentityEntity>, IClientIdentityRepository
{
    private readonly HcaDbContext _hcaDbContext;
    public ClientIdentityRepository(HcaDbContext dbContext) : base(dbContext)
    {
        _hcaDbContext = dbContext;
    }

    public async Task<(int, IEnumerable<ClientIdentityEntity>)> GetAll(Dictionary<string, string> searchFilter, List<int>? userModifyRecords = null, int pageNumber = 0, int recordsPerPage = 10, string orderBy = "")
    {
        if (userModifyRecords == null) userModifyRecords = new List<int>();
        var skip = pageNumber * recordsPerPage;
        Expression<Func<ClientIdentityEntity, bool>> searchQuery = (c) => c.IsActive == true;

        string filterFirstColumn = string.Empty;
        string filterSecondColumn = string.Empty;
        string filterFirstValue = string.Empty;
        string filterSecondValue = string.Empty;

        int i = 0;

        foreach (var key in searchFilter.Keys)
        {
            if (i == 0)
            {
                filterFirstColumn = key;
                filterFirstValue = searchFilter[key].ToLower();
            }

            if (i == 1)
            {
                filterSecondColumn = key;
                filterSecondValue = searchFilter[key].ToLower();
            }

            i++;
        }

        if (filterFirstColumn != string.Empty && filterSecondColumn == string.Empty)
        {
            if (filterFirstColumn == "FName") searchQuery = (c) => c.IsActive == true && c.FirstName.ToLower().StartsWith(filterFirstValue);
            if (filterFirstColumn == "LName") searchQuery = (c) => c.IsActive == true && c.LastName.ToLower().StartsWith(filterFirstValue);
            if (filterFirstColumn == "Ssn") searchQuery = (c) => c.IsActive == true && c.Ssn != null && c.Ssn.ToLower().StartsWith(filterFirstValue);
            if (filterFirstColumn == "SourceId") searchQuery = (c) => c.IsActive == true && c.SourceSystemId.ToLower().StartsWith(filterFirstValue);
            if (filterFirstColumn == "MpiLinkId") searchQuery = (c) => c.IsActive == true && c.MpiLinkId.ToLower().StartsWith(filterFirstValue);
            if (filterFirstColumn == "Email") searchQuery = (c) => c.IsActive == true && c.Communications.FirstOrDefault(cc => cc.EmailAddress.ToLower().StartsWith(filterFirstValue)) != null;
        }

        if (filterFirstColumn != string.Empty && filterSecondColumn != string.Empty)
        {
            if (filterFirstColumn == "FName")
            {
                if (filterSecondColumn == "FName") searchQuery = (c) => c.IsActive == true && (c.FirstName.ToLower().StartsWith(filterFirstValue) || c.FirstName.ToLower().StartsWith(filterSecondValue));
                if (filterSecondColumn == "Ssn") searchQuery = (c) => c.IsActive == true && c.FirstName.ToLower().StartsWith(filterFirstValue) && c.Ssn != null && c.Ssn.ToLower().StartsWith(filterSecondValue);
                if (filterSecondColumn == "SourceId") searchQuery = (c) => c.IsActive == true && c.FirstName.ToLower().StartsWith(filterFirstValue) && c.SourceSystemId.ToLower().StartsWith(filterSecondValue);
                if (filterSecondColumn == "MpiLinkId") searchQuery = (c) => c.IsActive == true && c.FirstName.ToLower().StartsWith(filterFirstValue) && c.MpiLinkId.ToLower().StartsWith(filterSecondValue);
                if (filterSecondColumn == "Email") searchQuery = (c) => c.IsActive == true && c.FirstName.ToLower().StartsWith(filterFirstValue) && c.Communications.FirstOrDefault(cc => cc.EmailAddress.ToLower().StartsWith(filterSecondValue)) != null;
            }

            if (filterFirstColumn == "LName")
            {
                if (filterSecondColumn == "FName") searchQuery = (c) => c.IsActive == true && c.LastName.ToLower().StartsWith(filterFirstValue) && c.FirstName.ToLower().StartsWith(filterSecondValue);
                if (filterSecondColumn == "Ssn") searchQuery = (c) => c.IsActive == true && c.LastName.ToLower().StartsWith(filterFirstValue) && c.Ssn != null && c.Ssn.ToLower().StartsWith(filterSecondValue);
                if (filterSecondColumn == "SourceId") searchQuery = (c) => c.IsActive == true && c.LastName.ToLower().StartsWith(filterFirstValue) && c.SourceSystemId.ToLower().StartsWith(filterSecondValue);
                if (filterSecondColumn == "MpiLinkId") searchQuery = (c) => c.IsActive == true && c.LastName.ToLower().StartsWith(filterFirstValue) && c.MpiLinkId.ToLower().StartsWith(filterSecondValue);
                if (filterSecondColumn == "Email") searchQuery = (c) => c.IsActive == true && c.LastName.ToLower().StartsWith(filterFirstValue) && c.Communications.FirstOrDefault(cc => cc.EmailAddress.ToLower().StartsWith(filterSecondValue)) != null;
            }

            if (filterFirstColumn == "Ssn")
            {
                if (filterSecondColumn == "FName") searchQuery = (c) => c.IsActive == true && c.Ssn != null && c.Ssn.ToLower().StartsWith(filterFirstValue) && c.FirstName.ToLower().StartsWith(filterSecondValue);
                if (filterSecondColumn == "LName") searchQuery = (c) => c.IsActive == true && c.Ssn != null && c.Ssn.ToLower().StartsWith(filterFirstValue) && c.LastName.ToLower().StartsWith(filterSecondValue);
                if (filterSecondColumn == "SourceId") searchQuery = (c) => c.IsActive == true && c.Ssn != null && c.Ssn.ToLower().StartsWith(filterFirstValue) && c.SourceSystemId.ToLower().StartsWith(filterSecondValue);
                if (filterSecondColumn == "MpiLinkId") searchQuery = (c) => c.IsActive == true && c.Ssn != null && c.Ssn.ToLower().StartsWith(filterFirstValue) && c.MpiLinkId.ToLower().StartsWith(filterSecondValue);
                if (filterSecondColumn == "Email") searchQuery = (c) => c.IsActive == true && c.Ssn != null && c.Ssn.ToLower().StartsWith(filterFirstValue) && c.Communications.FirstOrDefault(cc => cc.EmailAddress.ToLower().StartsWith(filterSecondValue)) != null;
            }

            if (filterFirstColumn == "SourceId")
            {
                if (filterSecondColumn == "FName") searchQuery = (c) => c.IsActive == true && c.SourceSystemId.ToLower().StartsWith(filterFirstValue) && c.FirstName.ToLower().StartsWith(filterSecondValue);
                if (filterSecondColumn == "LName") searchQuery = (c) => c.IsActive == true && c.SourceSystemId.ToLower().StartsWith(filterFirstValue) && c.LastName.ToLower().StartsWith(filterSecondValue);
                if (filterSecondColumn == "Ssn") searchQuery = (c) => c.IsActive == true && c.SourceSystemId.ToLower().StartsWith(filterFirstValue) && c.Ssn != null && c.Ssn.ToLower().StartsWith(filterSecondValue);
                if (filterSecondColumn == "MpiLinkId") searchQuery = (c) => c.IsActive == true && c.SourceSystemId.ToLower().StartsWith(filterFirstValue) && c.MpiLinkId.ToLower().StartsWith(filterSecondValue);
                if (filterSecondColumn == "Email") searchQuery = (c) => c.IsActive == true && c.SourceSystemId.ToLower().StartsWith(filterFirstValue) && c.Communications.FirstOrDefault(cc => cc.EmailAddress.ToLower().StartsWith(filterSecondValue)) != null;
            }

            if (filterFirstColumn == "MpiLinkId")
            {
                if (filterSecondColumn == "FName") searchQuery = (c) => c.IsActive == true && c.MpiLinkId.ToLower().StartsWith(filterFirstValue) && c.FirstName.ToLower().StartsWith(filterSecondValue);
                if (filterSecondColumn == "LName") searchQuery = (c) => c.IsActive == true && c.MpiLinkId.ToLower().StartsWith(filterFirstValue) && c.LastName.ToLower().StartsWith(filterSecondValue);
                if (filterSecondColumn == "Ssn") searchQuery = (c) => c.IsActive == true && c.MpiLinkId.ToLower().StartsWith(filterFirstValue) && c.Ssn != null && c.Ssn.ToLower().StartsWith(filterSecondValue);
                if (filterSecondColumn == "SourceId") searchQuery = (c) => c.IsActive == true && c.MpiLinkId.ToLower().StartsWith(filterFirstValue) && c.SourceSystemId.ToLower().StartsWith(filterSecondValue);
                if (filterSecondColumn == "Email") searchQuery = (c) => c.IsActive == true && c.MpiLinkId.ToLower().StartsWith(filterFirstValue) && c.Communications.FirstOrDefault(cc => cc.EmailAddress.ToLower().StartsWith(filterSecondValue)) != null;
            }

            if (filterFirstColumn == "Email")
            {
                if (filterSecondColumn == "FName") searchQuery = (c) => c.IsActive == true && c.Communications.FirstOrDefault(cc => cc.EmailAddress.ToLower().StartsWith(filterFirstValue)) != null && c.FirstName.ToLower().StartsWith(filterSecondValue);
                if (filterSecondColumn == "LName") searchQuery = (c) => c.IsActive == true && c.Communications.FirstOrDefault(cc => cc.EmailAddress.ToLower().StartsWith(filterFirstValue)) != null && c.LastName.ToLower().StartsWith(filterSecondValue);
                if (filterSecondColumn == "Ssn") searchQuery = (c) => c.IsActive == true && c.Communications.FirstOrDefault(cc => cc.EmailAddress.ToLower().StartsWith(filterFirstValue)) != null && c.Ssn != null && c.Ssn.ToLower().StartsWith(filterSecondValue);
                if (filterSecondColumn == "SourceId") searchQuery = (c) => c.IsActive == true && c.Communications.FirstOrDefault(cc => cc.EmailAddress.ToLower().StartsWith(filterFirstValue)) != null && c.SourceSystemId.ToLower().StartsWith(filterSecondValue);
                if (filterSecondColumn == "MpiLinkId") searchQuery = (c) => c.IsActive == true && c.Communications.FirstOrDefault(cc => cc.EmailAddress.ToLower().StartsWith(filterFirstValue)) != null && c.MpiLinkId.ToLower().StartsWith(filterSecondValue);
            }
        }

        var orderByQuery = OrderBy(orderBy) ?? DefaultOrderBy;
        var clientIdentityEntities = await GetAllAsync(searchQuery, ClientIdentitiesInclude, orderByQuery, skip, recordsPerPage);
        var result = clientIdentityEntities.ToList();
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
        if (orderByStr == null) return null;
        Func<IQueryable<ClientIdentityEntity>, IOrderedQueryable<ClientIdentityEntity>> orderBy = null;

        var orderByParts = orderByStr.Split(" ");
        string columnName = orderByParts[0].Trim();
        string orderByType = orderByParts.Length > 1 ? orderByParts[1].Trim() : string.Empty;

        if (orderByType == string.Empty || orderByType.ToLower() == "asc")
        {
            if (columnName == "mpiLinkId") orderBy = c => c.OrderBy(i => i.MpiLinkId);
            if (columnName == "sourceSystemId") orderBy = c => c.OrderBy(i => i.SourceSystemId);
            if (columnName == "sourceName") orderBy = c => c.OrderBy(i => i.SourceSystemName);
            if (columnName == "sourceSystemLastUpdate") orderBy = c => c.OrderBy(i => i.SourceSystemUpdated);
            if (columnName == "firstName") orderBy = c => c.OrderBy(i => i.FirstName);
            if (columnName == "middleName") orderBy = c => c.OrderBy(i => i.MiddleName);
            if (columnName == "lastName") orderBy = c => c.OrderBy(i => i.LastName);
            if (columnName == "suffix") orderBy = c => c.OrderBy(i => i.NameSuffix);
            if (columnName == "birthDate") orderBy = c => c.OrderBy(i => i.Dob);
            if (columnName == "gender") orderBy = c => c.OrderBy(i => i.Gender);
            if (columnName == "ssn") orderBy = c => c.OrderBy(i => i.Ssn);
        }
        else
        {
            if (columnName == "mpiLinkId") orderBy = c => c.OrderByDescending(i => i.MpiLinkId);
            if (columnName == "sourceSystemId") orderBy = c => c.OrderByDescending(i => i.SourceSystemId);
            if (columnName == "sourceName") orderBy = c => c.OrderByDescending(i => i.SourceSystemName);
            if (columnName == "sourceSystemLastUpdate") orderBy = c => c.OrderByDescending(i => i.SourceSystemUpdated);
            if (columnName == "firstName") orderBy = c => c.OrderByDescending(i => i.FirstName);
            if (columnName == "middleName") orderBy = c => c.OrderByDescending(i => i.MiddleName);
            if (columnName == "lastName") orderBy = c => c.OrderByDescending(i => i.LastName);
            if (columnName == "suffix") orderBy = c => c.OrderByDescending(i => i.NameSuffix);
            if (columnName == "birthDate") orderBy = c => c.OrderByDescending(i => i.Dob);
            if (columnName == "gender") orderBy = c => c.OrderByDescending(i => i.Gender);
            if (columnName == "ssn") orderBy = c => c.OrderByDescending(i => i.Ssn);
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

        foreach (var address in entity.Addresses)
        {
            var matchingAddress = identity.Addresses.FirstOrDefault(i => i.AddressType == address.AddressType && i.AddressLine1 == address.AddressLine1 && i.AddressLine2 == address.AddressLine2
                && i.AddressLine3 == address.AddressLine3 && i.City == address.City && i.State == address.State && i.ZipCode == address.ZipCode && i.ZipFour == address.ZipFour);

            if (matchingAddress == null)
            {
                identity.Addresses.Add(address);
            }
            else
            {
                foreach (var ac in address.AddressCommunications)
                {
                    var communication = matchingAddress.AddressCommunications.FirstOrDefault(mac => mac.Communication.EmailType == ac.Communication.EmailType && mac.Communication.PhoneType == ac.Communication.PhoneType
                    && mac.Communication.EmailAddress == ac.Communication.EmailAddress && mac.Communication.PhoneNumber == ac.Communication.PhoneNumber)?.Communication;

                    if (communication == null)
                    {
                        var addressCommunication = new ClientIdentityAddressCommunicationEntity()
                        {
                            Address = matchingAddress,
                            Communication = ac.Communication
                        };

                        matchingAddress.AddressCommunications.Add(addressCommunication);
                    }
                }
            }
        }

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

