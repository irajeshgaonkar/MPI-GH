using HCA.Models;
using HCA.Models.Enums;
using HCA.Models.MuleSoft;
using HCA.Models.Request;
using HCA.Models.Request.DOH;
using HCA.Models.SQS;

namespace HCA.Core.Services;

public interface IClientIdentityService
{
    Task<dynamic?> PostIdentities(IEnumerable<ClientIdentityRequest> identities, string currentUser, ProcessType processType, NotificationOptions? notificationOptions);
    Task<dynamic?> DOH_PostIdentities(DOH_PostClientIdentityRequest identities, string currentUser, ProcessType processType, NotificationOptions? notificationOptions);

    Task<(int, IEnumerable<ClientIdentityModel>)> GetAll(string currentUser, Dictionary<string, string> searchFilter, int pageNumber = 0, int recordsPerPage = 10, string orderBy = "");

    Task<dynamic?> LinkIdentities(LinkingSources linkingSources, string currentUser, ProcessType processType, NotificationOptions? notificationOptions);

    Task<dynamic?> UnLinkIdentities(UnLinkingSources unLinkingSources, string currentUser, ProcessType processType, NotificationOptions? notificationOptions);

    Task<dynamic?> MergeIdentities(MergingSources mergingSources, string currentUser, ProcessType processType, NotificationOptions? notificationOptions);

    Task<dynamic?> UnMergeIdentities(UnMergingSources unMergingSources, string currentUser, ProcessType processType, NotificationOptions? notificationOptions);

    Task<dynamic?> DemographicSearch(Identity filter, string currentUser, ProcessType processType, NotificationOptions? notificationOptions);

    Task<dynamic?> DemographicQuery(Identity filter, string currentUser, ProcessType processType, NotificationOptions? notificationOptions);
    Task<dynamic?> DOH_DemographicSearch(DOH_DemographicQueryRequest filter, string currentUser, ProcessType processType, NotificationOptions? notificationOptions);

    Task<dynamic?> DOH_DemographicQuery(DOH_DemographicQueryRequest filter, string currentUser, ProcessType processType, NotificationOptions? notificationOptions);
}