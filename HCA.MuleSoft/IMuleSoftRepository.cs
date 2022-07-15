using HCA.Infrastructure.Exceptions;
using HCA.Infrastructure.Http;
using HCA.MuleSoft.Models.Request.Link;
using HCA.MuleSoft.Models.Request.Merge;
using HCA.MuleSoft.Models.Request.Post;
using HCA.MuleSoft.Models.Response.Link;
using HCA.MuleSoft.Models.Response.Merge;
using HCA.MuleSoft.Models.Response.Post;

namespace HCA.MuleSoft;

public interface IMuleSoftRepository
{
    Task<PostIdentityResponse> PostIdentity(PostIdentityRequest clientIdentity);

    Task<LinkIdentitiesResponse> LinkIdentities(LinkIdentitiesRequest linkIdentitiesRequest);

    Task<UnLinkIdentitiesResponse> UnLinkIdentities(UnLinkIdentitiesRequest unLinkIdentitiesRequest);

    Task<MergeIdentitiesResponse> MergeIdentities(MergeIdentitiesRequest mergeIdentitiesRequest);

    Task<UnMergeIdentitiesResponse> UnMergeIdentities(UnMergeIdentitiesRequest unMergeIdentitiesRequest);

    Task<DemoGraphicSearchResponse> DemographicSearch(PostIdentityRequest searchRequest);
}

public class MuleSoftRepository : IMuleSoftRepository
{
    private readonly IHttpAdapter _httpAdapter;

    public MuleSoftRepository(IHttpAdapter httpAdapter)
    {
        _httpAdapter = httpAdapter;
    }

    public async Task<LinkIdentitiesResponse> LinkIdentities(LinkIdentitiesRequest linkIdentitiesRequest)
    {
        linkIdentitiesRequest.TrackingId = Guid.NewGuid().ToString();
        var response = await _httpAdapter.Post<LinkIdentitiesResponse, LinkIdentitiesRequest>("linkIdentities", linkIdentitiesRequest);

        if (null == response)
            throw new HcaMuleSoftException("Error occured while posting request to MuleSoft");

        return response;
    }

    public async Task<MergeIdentitiesResponse> MergeIdentities(MergeIdentitiesRequest mergeIdentitiesRequest)
    {
        var response = await _httpAdapter.Post<MergeIdentitiesResponse, MergeIdentitiesRequest>("mergeIdentities", mergeIdentitiesRequest);

        if (null == response)
            throw new HcaMuleSoftException("Error occured while posting request to MuleSoft");

        return response;
    }

    public async Task<PostIdentityResponse> PostIdentity(PostIdentityRequest clientIdentity)
    {
        var response = await _httpAdapter.Post<PostIdentityResponse, PostIdentityRequest>("postIdentity", clientIdentity);

        if (null == response)
            throw new HcaMuleSoftException("Error occured while posting request to MuleSoft");

        return response;
    }

    public async Task<UnLinkIdentitiesResponse> UnLinkIdentities(UnLinkIdentitiesRequest unLinkIdentitiesRequest)
    {
        var response = await _httpAdapter.Post<UnLinkIdentitiesResponse, UnLinkIdentitiesRequest>("unlinkIdentities", unLinkIdentitiesRequest);

        if (null == response)
            throw new HcaMuleSoftException("Error occured while posting request to MuleSoft");

        return response;
    }

    public async Task<UnMergeIdentitiesResponse> UnMergeIdentities(UnMergeIdentitiesRequest unMergeIdentitiesRequest)
    {
        var response = await _httpAdapter.Post<UnMergeIdentitiesResponse, UnMergeIdentitiesRequest>("unMergeIdentities", unMergeIdentitiesRequest);

        if (null == response)
            throw new HcaMuleSoftException("Error occured while posting request to MuleSoft");

        return response;
    }

    public async Task<DemoGraphicSearchResponse> DemographicSearch(PostIdentityRequest searchRequest)
    {
        var response = await _httpAdapter.Post<DemoGraphicSearchResponse, PostIdentityRequest>("demographicsSearch", searchRequest);

        if (null == response)
            throw new HcaMuleSoftException("Error occured while posting request to MuleSoft");

        return response;
    }
}