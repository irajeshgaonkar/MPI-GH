using HCA.Data.Entities;
using HCA.Infrastructure.Extensions;
using HCA.Models.MuleSoft;
using Microsoft.EntityFrameworkCore;

namespace HCA.Data.Repository;

public class ClientIdentityRepository : IClientIdentityRepository
{
    private readonly HcaDbContext _dbContext;
    private readonly IDataAdapter _dataAdapter;

    public ClientIdentityRepository(HcaDbContext dbContext, IDataAdapter dataAdapter)
    {
        _dbContext = dbContext;
        _dataAdapter = dataAdapter;
    }

    public async Task<IEnumerable<ClientIdentityEntity>> GetAll(int skip, int take)
    {
        var identities = _dbContext.ClientIdentities
                            .Include(c => c.Addresses)
                            .Include(c => c.Communications)
                            .Where(c => c.IsActive == true)
                            .Skip(skip)
                            .Take(take);

        return await Task.FromResult(identities);
    }

    public async Task<int> GetCount()
    {
        var count = _dbContext.ClientIdentities.Where(c => c.IsActive == true).Count();
        return await Task.FromResult(count);
    }

    public Task<IEnumerable<ClientIdentityEntity?>> GetBySourceAndId(string sourceSystemName, string sourceSystemId, string? mpiLinkId = null)
    {
        var clientIdentity
            = (null == mpiLinkId)
                            ? GetBySource(sourceSystemName, sourceSystemId)
                            : GetBySource(sourceSystemName, sourceSystemId, mpiLinkId);

        return Task.FromResult(clientIdentity);
    }

    public async Task UpdateMpiLinkId(string sourceSystemName, string sourceSystemId, string newMpiLinkId)
    {
        var clientIdentities = GetBySource(sourceSystemName, sourceSystemId).ToList();

        foreach (var clientIdentity in clientIdentities)
        {
            try
            {
                if (null == clientIdentity) continue;

                if (newMpiLinkId == clientIdentity.MpiLinkId) continue;

                var clonedClientIdentity = CloneIdentity(clientIdentity);

                if (null == clonedClientIdentity) continue;

                _dbContext.ClientIdentities.Remove(clientIdentity);
                _dbContext.Entry(clientIdentity).State = EntityState.Deleted;
                await _dbContext.SaveChangesAsync();

                clonedClientIdentity.MpiLinkId = newMpiLinkId;
                _dbContext.ClientIdentities.Add(clonedClientIdentity);
                await _dbContext.SaveChangesAsync();
            }
            catch(Exception e)
            {

            }
        }
    }

    

    public async Task Upsert(ClientIdentityEntity clientIdentity)
    {
        var dbIdentities = GetBySource(clientIdentity.MpiLinkId, clientIdentity.SourceSystemName, clientIdentity.SourceSystemId).ToList();

        if (null == dbIdentities || 0 == dbIdentities.Count)
        {
            _dbContext.ClientIdentities.Add(clientIdentity);
            await _dbContext.SaveChangesAsync();
        }
        else
        {
            //_dbContext.Entry(clientIdentity).State = EntityState.Modified;
            //await _dbContext.SaveChangesAsync();
        }
    }

    public async Task Update(ClientIdentityEntity clientIdentity)
    {
        _dbContext.Entry(clientIdentity).State = EntityState.Modified;
        await _dbContext.SaveChangesAsync();
    }



    public async Task<IEnumerable<ClientIdentityEntity>> GetBySources(List<Source> sources)
    {
        var result = new List<ClientIdentityEntity>();
        foreach(var source in sources)
        {
            var clientIdentities = GetBySource(source.Name, source.Id).ToList();
            if (clientIdentities != null)
            {
                foreach(var clientIdentity in clientIdentities)
                {
                    if (clientIdentity != null)
                    {
                        result.Add(clientIdentity);
                    }
                }
            }
        }

        return await Task.FromResult(result);
    }

    private IEnumerable<ClientIdentityEntity?> GetBySource(string sourceSystemName, string sourceSystemId)
    {
        var clientIdentity = _dbContext.ClientIdentities
            .Include(c => c.Addresses)
            .Include(c => c.Communications)
            .Where(t => t.SourceSystemName == sourceSystemName
                                && t.SourceSystemId == sourceSystemId);
        return clientIdentity;
    }

    private IEnumerable<ClientIdentityEntity?> GetBySource(string mpiLinkId, string sourceSystemName, string sourceSystemId)
    {
        var clientIdentity = _dbContext.ClientIdentities
            .Include(c => c.Addresses)
            .Include(c => c.Communications)
            .Where(t => t.MpiLinkId == mpiLinkId && t.SourceSystemName == sourceSystemName
                                && t.SourceSystemId == sourceSystemId);
        return clientIdentity;
    }

    private ClientIdentityEntity CloneIdentity(ClientIdentityEntity clientIdentityEntity)
    {
        var clonedIdentity = new ClientIdentityEntity();

        clonedIdentity.SourceSystemName = clientIdentityEntity.SourceSystemName;
        clonedIdentity.SourceSystemId = clientIdentityEntity.SourceSystemId;
        clonedIdentity.SourceSystemAgency = clientIdentityEntity.SourceSystemAgency;
        clonedIdentity.FirstName = clientIdentityEntity.FirstName;
        clonedIdentity.MiddleName = clientIdentityEntity.MiddleName;
        clonedIdentity.LastName = clientIdentityEntity.LastName;
        clonedIdentity.NameSuffix = clientIdentityEntity.NameSuffix;
        clonedIdentity.Ssn = clientIdentityEntity.Ssn;
        clonedIdentity.Dob = clientIdentityEntity.Dob;
        clonedIdentity.Gender = clientIdentityEntity.Gender;
        clonedIdentity.ProtectedPopulationFlag = clientIdentityEntity.ProtectedPopulationFlag;
        clonedIdentity.ProtectedPopulationType = clientIdentityEntity.ProtectedPopulationType;
        clonedIdentity.MpiUpdated = clientIdentityEntity.MpiUpdated;
        clonedIdentity.SourceSystemUpdated = clientIdentityEntity.SourceSystemUpdated;
        clonedIdentity.ExpiryDate = clientIdentityEntity.ExpiryDate;
        clonedIdentity.IsActive = clientIdentityEntity.IsActive;
        clonedIdentity.IsDelete = clientIdentityEntity.IsDelete;
        clonedIdentity.CreatedBy = clientIdentityEntity.CreatedBy;
        clonedIdentity.CreatedDate = clientIdentityEntity.CreatedDate;
        clonedIdentity.UpdatedBy = clientIdentityEntity.UpdatedBy;
        clonedIdentity.UpdatedDate = clientIdentityEntity.UpdatedDate;

        clonedIdentity.Addresses = new List<ClientIdentityAddressEntity>();

        foreach (var address in clientIdentityEntity.Addresses)
        {
            var clonedAddress = CloneIdentityAddress(address);
            clonedIdentity.Addresses.Add(address);
        }

        clonedIdentity.Communications = new List<ClientIdentityCommunicationEntity>();

        foreach (var communication in clientIdentityEntity.Communications)
        {
            var clonedCommunication = CloneIdentityCommunication(communication);
            clonedIdentity.Communications.Add(clonedCommunication);
        }

        return clonedIdentity;
    }

    private ClientIdentityAddressEntity CloneIdentityAddress(ClientIdentityAddressEntity clientIdentityAddressEntity)
    {
        var clonedAddressIdentity = new ClientIdentityAddressEntity();

        clonedAddressIdentity.SourceSystemName = clientIdentityAddressEntity.SourceSystemName;
        clonedAddressIdentity.SourceSystemId = clientIdentityAddressEntity.SourceSystemId;
        clonedAddressIdentity.AddressType = clientIdentityAddressEntity.AddressType;
        clonedAddressIdentity.AddressLine1 = clientIdentityAddressEntity.AddressLine1;
        clonedAddressIdentity.AddressLine2 = clientIdentityAddressEntity.AddressLine2;
        clonedAddressIdentity.AddressLine3 = clientIdentityAddressEntity.AddressLine3;
        clonedAddressIdentity.City = clientIdentityAddressEntity.City;
        clonedAddressIdentity.State = clientIdentityAddressEntity.State;
        clonedAddressIdentity.ZipCode = clientIdentityAddressEntity.ZipCode;
        clonedAddressIdentity.ZipFour = clientIdentityAddressEntity.ZipFour;
        clonedAddressIdentity.SourceSystemUpdated = clientIdentityAddressEntity.SourceSystemUpdated;
        clonedAddressIdentity.ExpiryDate = clientIdentityAddressEntity.ExpiryDate;
        clonedAddressIdentity.IsActive = clientIdentityAddressEntity.IsActive;
        clonedAddressIdentity.IsDelete = clientIdentityAddressEntity.IsDelete;
        clonedAddressIdentity.CreatedBy = clientIdentityAddressEntity.CreatedBy;
        clonedAddressIdentity.CreatedDate = clientIdentityAddressEntity.CreatedDate;
        clonedAddressIdentity.UpdatedBy = clientIdentityAddressEntity.UpdatedBy;
        clonedAddressIdentity.UpdatedDate = clientIdentityAddressEntity.UpdatedDate;

        return clonedAddressIdentity;

    }

    private ClientIdentityCommunicationEntity CloneIdentityCommunication(ClientIdentityCommunicationEntity clientIdentityCommunicationIdentity)
    {
        var clonedCommuncationIdentity = new ClientIdentityCommunicationEntity();

        clonedCommuncationIdentity.SourceSystemName = clientIdentityCommunicationIdentity.SourceSystemName;
        clonedCommuncationIdentity.SourceSystemId = clientIdentityCommunicationIdentity.SourceSystemId;

        clonedCommuncationIdentity.PhoneType = clientIdentityCommunicationIdentity.PhoneType;
        clonedCommuncationIdentity.EmailType = clientIdentityCommunicationIdentity.EmailType;
        clonedCommuncationIdentity.EmailAddress = clientIdentityCommunicationIdentity.EmailAddress;
        clonedCommuncationIdentity.PhoneNumber = clientIdentityCommunicationIdentity.PhoneNumber;
        clonedCommuncationIdentity.SourceSystemUpdated = clientIdentityCommunicationIdentity.SourceSystemUpdated;
        clonedCommuncationIdentity.ExpiryDate = clientIdentityCommunicationIdentity.ExpiryDate;
        clonedCommuncationIdentity.IsActive = clientIdentityCommunicationIdentity.IsActive;
        clonedCommuncationIdentity.IsDelete = clientIdentityCommunicationIdentity.IsDelete;
        clonedCommuncationIdentity.CreatedBy = clientIdentityCommunicationIdentity.CreatedBy;
        clonedCommuncationIdentity.CreatedDate = clientIdentityCommunicationIdentity.CreatedDate;
        clonedCommuncationIdentity.UpdatedBy = clientIdentityCommunicationIdentity.UpdatedBy;
        clonedCommuncationIdentity.UpdatedDate = clientIdentityCommunicationIdentity.UpdatedDate;

        return clonedCommuncationIdentity;

    }

    public async Task<IEnumerable<ClientIdentityEntity>> Search(string? fName, string? mName, string? lName, string? email, string? ssn)
    {
        if (fName.IsNotEmpty())
            return _dbContext.ClientIdentities
                            .Include(c => c.Addresses)
                            .Include(c => c.Communications)
                            .Where(c => c.FirstName == fName).AsEnumerable();

        if (mName.IsNotEmpty())
            return _dbContext.ClientIdentities
                            .Include(c => c.Addresses)
                            .Include(c => c.Communications)
                            .Where(c => c.MiddleName == mName).AsEnumerable();

        if (lName.IsNotEmpty())
            return _dbContext.ClientIdentities
                            .Include(c => c.Addresses)
                            .Include(c => c.Communications)
                            .Where(c => c.LastName == lName).AsEnumerable();

        if (ssn.IsNotEmpty())
            return _dbContext.ClientIdentities
                            .Include(c => c.Addresses)
                            .Include(c => c.Communications)
                            .Where(c => c.Ssn == ssn).AsEnumerable();

        //if (email.IsNotEmpty())
        //    return _dbContext.ClientIdentities
        //                    .Include(c => c.Addresses)
        //                    .Include(c => c.Communications)
        //                    .Where(c => c.Communications == ssn).AsEnumerable();

        return Enumerable.Empty<ClientIdentityEntity>();
    }
}

