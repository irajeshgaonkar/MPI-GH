using HCA.Infrastructure.Exceptions;
using HCA.Infrastructure.Http;
using HCA.Models.MuleSoft.Request;
using HCA.Models.MuleSoft.Response;

namespace HCA.MuleSoft;

///<inheritdoc />
public class MuleSoftRepository : IMuleSoftRepository
{
    private readonly IHttpAdapter _httpAdapter;

    /// <summary>
    /// <see cref="MuleSoftRepository"/>
    /// </summary>
    /// <param name="httpAdapter">Http adapter for doing http(s) calls</param>
    public MuleSoftRepository(IHttpAdapter httpAdapter)
    {
        _httpAdapter = httpAdapter;
    }

    ///<inheritdoc />
    public async Task<DemoGraphicSearchResponse> DemographicSearch(PostIdentityRequest request)
        => await Execute<DemoGraphicSearchResponse>(MuleSoftUrls.DemographicSearch, request);

    ///<inheritdoc />
    public async Task<LinkIdentitiesResponse> LinkIdentities(LinkIdentitiesRequest request)
        => await Execute<LinkIdentitiesResponse>(MuleSoftUrls.LinkIdntities, request);

    ///<inheritdoc />
    public async Task<UnLinkIdentitiesResponse> UnLinkIdentities(UnLinkIdentitiesRequest request)
        => await Execute<UnLinkIdentitiesResponse>(MuleSoftUrls.UnLinkIdentities, request);

    ///<inheritdoc />
    public async Task<MergeIdentitiesResponse> MergeIdentities(MergeIdentitiesRequest request)
        => await Execute<MergeIdentitiesResponse>(MuleSoftUrls.MergeIdentities, request);

    ///<inheritdoc />
    public async Task<UnMergeIdentitiesResponse> UnMergeIdentities(UnMergeIdentitiesRequest request)
        => await Execute<UnMergeIdentitiesResponse>(MuleSoftUrls.UnMergeIdentitie, request);

    ///<inheritdoc />
    public async Task<PostIdentityResponse> PostIdentity(PostIdentityRequest request)
        => await Execute<PostIdentityResponse>(MuleSoftUrls.PostIdentities, request);

    private async Task<T> Execute<T>(string requestUrl, MuleSoftRequest request)
    {

        var response = await _httpAdapter.Post<T>(requestUrl, request);

        if (null == response)
            throw new HcaMuleSoftException("Error occured while posting request to MuleSoft");

        return response;
    }
}
