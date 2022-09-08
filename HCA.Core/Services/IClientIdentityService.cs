using HCA.Models;
using HCA.Models.Enums;
using HCA.Models.MuleSoft;
using HCA.Models.SQS;

namespace HCA.Core.Services;

public interface IClientIdentityService
{
    Task<(int, IEnumerable<ClientIdentityModel>)> GetAll(string currentUser, string searchBy = "", string searchValue = "", int pageNumber = 0, int recordsPerPage = 10);

    Task<dynamic?> LinkIdentities(LinkingSources linkingSources, string currentUser, ProcessType processType, NotificationOptions? notificationOptions);

    Task<dynamic?> UnLinkIdentities(UnLinkingSources unLinkingSources, string currentUser, ProcessType processType, NotificationOptions? notificationOptions);

    Task<dynamic?> MergeIdentities(MergingSources mergingSources, string currentUser, ProcessType processType, NotificationOptions? notificationOptions);

    Task<dynamic?> UnMergeIdentities(UnMergingSources unMergingSources, string currentUser, ProcessType processType, NotificationOptions? notificationOptions);

    Task<dynamic?> DemographicSearch(Identity filter, string currentUser, ProcessType processType, NotificationOptions? notificationOptions);
}

public interface IUserModifyRecordsService
{
    Task<IEnumerable<ClientIdentityModel>> GetUserRecords(string currentUser);

    Task MoveToModify(string userName, int clientIdentityId);

    Task RemoveModify(string userName, int clientIdentityId);
}

