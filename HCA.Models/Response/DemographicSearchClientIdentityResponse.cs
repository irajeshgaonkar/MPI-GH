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
