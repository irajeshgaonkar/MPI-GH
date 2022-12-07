using HCA.Core.Mapper;
using HCA.Data.Entities;
using HCA.Data.Repository;
using HCA.Models;
using HCA.Models.Request;

namespace HCA.Core.Services;

public class ReportService : IReportsService
{
    private readonly IUserRequestRepository _userRequestRepository;

    private readonly IClientIdentityRepository _clientIdentityRepository;

    private readonly IUserRequestMapper _userRequestMapper;


    public ReportService(IUserRequestRepository userRequestRepository, IUserRequestMapper userRequestMapper, IClientIdentityRepository clientIdentityRepository)
    {
        _userRequestRepository = userRequestRepository;
        _clientIdentityRepository = clientIdentityRepository;
        _userRequestMapper = userRequestMapper;
    }

    public async Task<(int, Dictionary<string, IEnumerable<ClientIdentityModel>>)> GetClientIdentityGroupedByLinkId(string linkId, int pageNumber = 0, int recordsPerPage = 10, string orderBy = "")
    {
        var result = new Dictionary<string, IEnumerable<ClientIdentityModel>>();
        var (count, clientIdentities) = await _clientIdentityRepository.GetClientIdentityGroupedByLinkId(linkId, pageNumber, recordsPerPage, orderBy);

        foreach(var key in clientIdentities.Keys)
        {
            result.Add(key, ClientIdentityMapper.MapToClientIdentityModel(clientIdentities[key]));
        }

        return (count, result);
    }

    public async Task<(int, IEnumerable<UserRequest>)> GetUserRequests(string userFilter, int pageNumber = 0, int recordsPerPage = 10, string orderBy = "")
    {
        var (count, userRequests) = await _userRequestRepository.GetAll(userFilter, pageNumber, recordsPerPage, orderBy);
        var result = _userRequestMapper.MapToModelCollection(userRequests);
        return (count, result);
    }

    

}

