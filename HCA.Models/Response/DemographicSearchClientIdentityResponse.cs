using HCA.Models.MuleSoft;
using HCA.Models.MuleSoft.Response;

namespace HCA.Models.Response;

public class DemographicSearchClientIdentityResponse : BaseResponse
{
    /// <summary>
    /// collection of search results <see cref="PostIdentityResponseContent"/>
    /// </summary>
    public List<PostIdentityResponseContent> Content { get; set; }

    public IList<Identity> Result { get; set; }
}

public class DOH_DemographicSearchClientIdentityResponse : BaseResponse
{
    /// <summary>
    /// collection of search results <see cref="PostIdentityResponseContent"/>
    /// </summary>
    public dynamic Content { get; set; }

}


public class DemographicQueryClientIdentityResponse : BaseResponse
{
    /// <summary>
    /// collection of search results <see cref="PostIdentityResponseContent"/>
    /// </summary>
    public DemographicQueryResponseContent Content { get; set; }

    public IList<Identity> Result { get; set; }
}

public class DOH_DemographicQueryClientIdentityResponse : BaseResponse
{
    /// <summary>
    /// collection of search results <see cref="PostIdentityResponseContent"/>
    /// </summary>
    public dynamic Content { get; set; }

    public IList<Identity> Result { get; set; }
}