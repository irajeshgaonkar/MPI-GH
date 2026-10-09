using HCA.Core.Mapper;
using HCA.Data.Entities;
using HCA.Data.Repository;
using HCA.Models;

namespace HCA.Core.Services;

public class UserModifyRecordsService : IUserModifyRecordsService
{
    private readonly IUserModifyRecordsRepository _userModifyRecordsRepository;

    private readonly IClientIdentityRepository _clientIdentityRepository;

    public UserModifyRecordsService(IUserModifyRecordsRepository userModifyRecordsRepository,
        IClientIdentityRepository clientIdentityRepository)
    {
        _userModifyRecordsRepository = userModifyRecordsRepository;
        _clientIdentityRepository = clientIdentityRepository;
    }

    public async Task<IEnumerable<ClientIdentityModel>> GetUserRecords(string currentUser)
    {
        var userModifyRecords = (await _userModifyRecordsRepository.GetAllAsync(r => r.UserName == currentUser)).Select(t => t.ClientIdentityId).ToList();
        var entities = await _clientIdentityRepository.GetAllByQuery(c => c.IsActive == true && userModifyRecords.Contains(c.Id));
        var models = ClientIdentityMapper.MapToClientIdentityModel(entities);
        return models;
    }

    public async Task MoveToModify(string userName, int clientIdentityId)
    {
        var entity = new UserModifyRecordsEntity()
        {
            UserName = userName,
            ClientIdentityId = clientIdentityId
        };

        _userModifyRecordsRepository.AddAsync(entity);
        await Task.CompletedTask;
    }

    public async Task RemoveModify(string userName, int clientIdentityId)
    {
        var entity = await _userModifyRecordsRepository.GetSingleAsync(m => m.UserName == userName && m.ClientIdentityId == clientIdentityId);

        if (entity != null)
        {
            _userModifyRecordsRepository.Delete(entity);
        }
    }
}

