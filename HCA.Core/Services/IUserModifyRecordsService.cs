using HCA.Models;

namespace HCA.Core.Services;

public interface IUserModifyRecordsService
{
    Task<IEnumerable<ClientIdentityModel>> GetUserRecords(string currentUser);

    Task MoveToModify(string userName, int clientIdentityId);

    Task RemoveModify(string userName, int clientIdentityId);
}

