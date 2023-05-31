using HCA.Models;
using HCA.Models.Request;

namespace HCA.Core.Services;

public interface IReportsService
{
    Task<(int, IEnumerable<UserRequest>)> GetUserRequests(string userFilter, int pageNumber = 0, int recordsPerPage = 10, string orderBy = "");

    Task<(int, Dictionary<string, IEnumerable<ClientIdentityModel>>)> GetClientIdentityGroupedByLinkId(string linkId, int pageNumber = 0, int recordsPerPage = 10, string orderBy = "");
}