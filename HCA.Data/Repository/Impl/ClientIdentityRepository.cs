using System.Linq.Expressions;
using HCA.Data.Entities;
using HCA.Data.Extensions;
using HCA.Data.Repository.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Npgsql;

namespace HCA.Data.Repository;


public class ClientIdentityRepository : RepositoryBase<ClientIdentityEntity>, IClientIdentityRepository
{
    private readonly HcaDbContext _hcaDbContext;
    public ClientIdentityRepository(HcaDbContext dbContext) : base(dbContext)
    {
        _hcaDbContext = dbContext;
    }


    public Task<(int, Dictionary<string, IEnumerable<ClientIdentityEntity>>)> GetClientIdentityGroupedByLinkId(string? linkId, int pageNumber = 0, int recordsPerPage = 10, string orderBy = "")
    {

        var skip = pageNumber * recordsPerPage;
        var result = new Dictionary<string, IEnumerable<ClientIdentityEntity>>();
        Expression<Func<ClientIdentityEntity, bool>> searchQuery = (c) => c.IsActive == true;
        List<ClientIdentityEntity>? clientIdentities = null;
        int count = 0;


        if (!string.IsNullOrWhiteSpace(linkId))
        {
            linkId += "%";
            NpgsqlParameter parameterS = new NpgsqlParameter(":mpi_link_id", linkId);
            NpgsqlParameter parameterD = new NpgsqlParameter(":limit", recordsPerPage);
            NpgsqlParameter parameterP = new NpgsqlParameter(":offset", skip);
            clientIdentities = _hcaDbContext.ClientIdentities.FromSqlRaw(@"SELECT * FROM client_identity
WHERE mpi_link_id IN
(
    SELECT mpi_link_id
    FROM client_identity
    WHERE mpi_link_id like :mpi_link_id
    GROUP BY mpi_link_id
    HAVING COUNT(*) > 1
    ORDER BY mpi_link_id
    limit :limit
    offset :offset
)
ORDER BY mpi_link_id; ", parameterS, parameterD, parameterP)
                .ToList();

            count = _hcaDbContext.Set<IntReturn>().FromSqlRaw(@"SELECT COUNT(distinct mpi_link_id) As Value FROM client_identity
WHERE mpi_link_id IN
(
	SELECT mpi_link_id
    FROM client_identity
    WHERE mpi_link_id like :mpi_link_id
    GROUP BY mpi_link_id
    HAVING COUNT(*) > 1
)", parameterS)
                .AsEnumerable()
                .First().Value;

        }
        else
        {

            NpgsqlParameter parameterD1 = new NpgsqlParameter(":limit", recordsPerPage);
            NpgsqlParameter parameterP1 = new NpgsqlParameter(":offset", skip);
            clientIdentities = _hcaDbContext.ClientIdentities.FromSqlRaw(@"SELECT * FROM client_identity
WHERE mpi_link_id IN
(
    SELECT mpi_link_id
    FROM client_identity
    GROUP BY mpi_link_id
    HAVING COUNT(*) > 1
    ORDER BY mpi_link_id
    limit :limit
    offset :offset
)
ORDER BY mpi_link_id; ", parameterD1, parameterP1)
                .ToList();

            count = _hcaDbContext.Set<IntReturn>().FromSqlRaw(@"SELECT COUNT(distinct mpi_link_id) As Value  FROM client_identity
WHERE mpi_link_id IN
(
	SELECT mpi_link_id
    FROM client_identity
    GROUP BY mpi_link_id
    HAVING COUNT(*) > 1
)")
                .AsEnumerable()
            .First().Value;
        }

        var clientIdentitiesGroup = clientIdentities.GroupBy(c => c.MpiLinkId);

        foreach (var group in clientIdentitiesGroup)
        {
            result.Add(group.Key, group.ToList());
        }

        return Task.FromResult((count, result));
    }

    public async Task<(int, IEnumerable<ClientIdentityEntity>)> GetAll(
        Dictionary<string, string> searchFilter,
        List<int>? userModifyRecords = null,
        int pageNumber = 0,
        int recordsPerPage = 10,
        string orderBy = "")
    {
        userModifyRecords ??= [];
        var skip = pageNumber * recordsPerPage;
        Expression<Func<ClientIdentityEntity, bool>> searchQuery = c => c.IsActive;

        // Build the filters dynamically from the searchFilter dictionary
        foreach (var filter in searchFilter)
        {
            var filterKey = filter.Key.ToLower();
            var filterValue = filter.Value.ToLower();

            switch (filterKey)
            {
                //TODO: Convert ToLower() to use proper collation for case-insensitive comparisons
                //https://learn.microsoft.com/en-us/ef/core/miscellaneous/collations-and-case-sensitivity#explicit-collations-and-indexes

                case "fname":
                    searchQuery = searchQuery.And(() => c => c.FirstName.ToLower().StartsWith(filterValue));
                    break;
                case "lname":
                    searchQuery = searchQuery.And(() => c => c.LastName.ToLower().StartsWith(filterValue));
                    break;
                case "ssn":
                    searchQuery = searchQuery.And(() => c => c.Ssn != null && c.Ssn.ToLower().StartsWith(filterValue));
                    break;
                case "sourceid":
                    searchQuery = searchQuery.And(() => c => c.SourceSystemId.ToLower().StartsWith(filterValue));
                    break;
                case "mpilinkid":
                    searchQuery = searchQuery.And(() => c => c.MpiLinkId.ToLower().StartsWith(filterValue));
                    break;
                case "email":
                    searchQuery = searchQuery.And(() => c => c.Communications
                        .Any(cc => cc.EmailAddress.ToLower().StartsWith(filterValue)));
                    break;

                case "contact":
                    searchQuery = searchQuery.And(() => c => c.Communications
                        .Any(cc => cc.PhoneNumber.ToLower().StartsWith(filterValue)));
                    break;

                case "dateofbirth":
                    searchQuery = searchQuery.And(() => c => c.Dob != null && c.Dob == DateOnly.FromDateTime(Convert.ToDateTime(filterValue)));
                    break;

                case "addressline1":
                    searchQuery = searchQuery.And(() => c => c.Addresses
                        .Any(cc => cc.AddressLine1.ToLower().StartsWith(filterValue)));
                    break;

                case "addressline2":
                    searchQuery = searchQuery.And(() => c => c.Addresses
                        .Any(cc => cc.AddressLine2 != null && cc.AddressLine2.ToLower().StartsWith(filterValue)));
                    break;

                case "city":
                    searchQuery = searchQuery.And(() => c => c.Addresses
                        .Any(cc => cc.City.ToLower().StartsWith(filterValue)));
                    break;
                case "state":
                    searchQuery = searchQuery.And(() => c => c.Addresses
                        .Any(cc => cc.State.ToLower().StartsWith(filterValue)));
                    break;
                case "zip":
                    searchQuery = searchQuery.And(() => c => c.Addresses
                        .Any(cc => cc.ZipCode.ToLower().StartsWith(filterValue)));
                    break;

                case "sourcesystemnames":
                    var scopes = filterValue.Split(',');
                    searchQuery = searchQuery.And(() => c => scopes.Any(s => c.SourceSystemName.ToLower().StartsWith(s)));
                    break;

                // Add more filters here as needed

                default:
                    // Handle unknown filters, or skip them
                    break;
            }
        }

        // Handle ordering
        var orderByQuery = OrderBy(orderBy) ?? DefaultOrderBy;

        // Retrieve data
        var clientIdentityEntities = await GetAllAsync(searchQuery, ClientIdentitiesInclude, orderByQuery, skip, recordsPerPage);
        var result = clientIdentityEntities.ToList();
        var count = Count(searchQuery);

        return (count, result);
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

    public async Task DeleteMpiLinkId(string mpiLinkId)
    {
        var clientIdenty = await GetByMpiLinkId(mpiLinkId);
        if (null == clientIdenty) return;
        clientIdenty.IsDelete = true;
        Update(clientIdenty);
    }

    public async Task<ClientIdentityEntity?> GetBySource(string sourceSystemName, string sourceSystemId)
    {
        var identity = await GetSingleAsync(SourceSystemFilter(sourceSystemName, sourceSystemId), ClientIdentitiesInclude);
        return identity;
    }

    public async Task<ClientIdentityEntity?> GetBySourceAll(string sourceSystemName, string sourceSystemId)
    {
        var identity = await GetSingleAsync(SourceSystemFilterAll(sourceSystemName, sourceSystemId), ClientIdentitiesInclude);
        return identity;
    }

    public async Task<ClientIdentityEntity?> GetByMpiLinkId(string mpiLinkId)
    {
        var identity = await GetSingleAsync(c => c.MpiLinkId == mpiLinkId, ClientIdentitiesInclude);
        return identity;
    }

    private Func<IQueryable<ClientIdentityEntity>, IOrderedQueryable<ClientIdentityEntity>>? OrderBy(string orderByStr)
    {
        if (orderByStr == null) return null;
        Func<IQueryable<ClientIdentityEntity>, IOrderedQueryable<ClientIdentityEntity>>? orderBy = null;

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

    private Expression<Func<ClientIdentityEntity, bool>> SourceSystemFilterAll(string sourceSystemName, string sourceSystemId)
        => c => c.SourceSystemName == sourceSystemName && c.SourceSystemId == sourceSystemId;

    // TODO: what does 'Upsert' mean? Perhaps 'assert'?
    public async Task<ClientIdentityEntity?> Upsert(ClientIdentityEntity entity)
    {
        var identity = await GetBySourceAll(entity.SourceSystemName, entity.SourceSystemId);

        if (identity == null)
        {
            // TODO: this should probably be an error
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
        identity.CustomJson = entity.CustomJson;
        identity.IsActive = true;
        identity.IsDelete = false;

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

                matchingAddress.IsActive = true;
                matchingAddress.IsDelete = false;
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
                    else
                    {
                        communication.IsActive = true;
                        communication.IsDelete = false;
                    }
                }
            }
        }

        Update(identity);
        return identity;
    }

    public void UpdateMpiLinkId(ClientIdentityEntity clientIdentityEntity, string newMpiLinkId)
    {
        if (clientIdentityEntity == null || string.IsNullOrEmpty(newMpiLinkId)) return;

        clientIdentityEntity.MpiLinkId = newMpiLinkId;

        if (clientIdentityEntity.Addresses != null && clientIdentityEntity.Addresses.Count != 0)
        {
            foreach (var address in clientIdentityEntity.Addresses)
            {
                address.MpiLinkId = newMpiLinkId;
            }
        }

        if (clientIdentityEntity.Communications != null && clientIdentityEntity.Communications.Count != 0)
        {
            foreach (var communicaiton in clientIdentityEntity.Communications)
            {
                communicaiton.MpiLinkId = newMpiLinkId;
            }
        }

        Update(clientIdentityEntity);
    }

    public void DeleteClientIdentity(ClientIdentityEntity clientIdentityEntity)
    {
        if(clientIdentityEntity == null) return;

        clientIdentityEntity.IsDelete = true;
        clientIdentityEntity.IsActive = false;

        if (clientIdentityEntity.Addresses != null && clientIdentityEntity.Addresses.Count != 0)
        {
            foreach (var address in clientIdentityEntity.Addresses)
            {
                address.IsDelete = true;
                address.IsActive = false;
            }
        }

        if (clientIdentityEntity.Communications != null && clientIdentityEntity.Communications.Count != 0)
        {
            foreach (var communication in clientIdentityEntity.Communications)
            {
                communication.IsDelete = true;
                communication.IsActive = false;
            }
        }

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

