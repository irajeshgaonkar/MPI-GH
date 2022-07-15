using System.ComponentModel.DataAnnotations;
using HCA.Models.MuleSoft;
using HCA.Models.Request;
using HCA.Models.Response;

namespace HCA.Models;


public class ClientIdentity
{
    public int Id { get; set; }

    public string MPILinkId { get; set; }

    public string SourceName { get; set; }

    public string SourceSystemId { get; set; }

    public DateTime SourceSystemLastUpdate { get; set; }

    public string FirstName { get; set; }

    public string MiddleName { get; set; }

    public string LastName { get; set; }

    public string Suffix { get; set; }

    public string BirthDate { get; set; }

    public string Gender { get; set; }

    public string SSN { get; set; }

    public string AddressType { get; set; }

    public string AddressLine1 { get; set; }

    public string AddressLine2 { get; set; }

    public string AddressLine3 { get; set; }

    public string City { get; set; }

    public string State { get; set; }

    public string ZipCode { get; set; }

    public string ZipPlusFour { get; set; }

    public string PhoneType { get; set; }

    public string PhoneNumber { get; set; }

    public string EmailType { get; set; }

    public string EmailAddress { get; set; }

    public bool ProtectecPopulationFlag { get; set; }

    public string ProtectedPopulationType { get; set; }
}

//public class LinkIdentityRequest
//{

//}

public class LinkSourcesResponse : BaseResponse
{
    public string LinkId { get; set; }

    public Source LinkToSource { get; set; }
}

public class UnLinkSourcesResponse : BaseResponse
{
    public string UnlinkedId { get; set; }

    public Source UnlinkedSource { get; set; }

    public string UnlinkedFromId { get; set; }

    public Source UnlinkedFromSource { get; set; }
}

public class MergeSourcesResponse : BaseResponse
{
    public string LinkId { get; set; }

    public Source Source { get; set; }
}

public class UnMergeSourcesResponse : BaseResponse
{
    public string UnmergedId { get; set; }

    public string UnmergedFromId { get; set; }

    public Source UnmergedFromSource { get; set; }

    public Source UnmergedSource { get; set; }
}

public class DemographicSearchResponse : BaseResponse
{
    public List<ClientIdentity> ClientIdentities { get; set; }
}

public class LinkSourcesRequest : BaseRequest
{
    public LinkSourcesRequest(Guid requestId, string trackingId, Source linkToSource, Source source)
        : base(requestId, trackingId)
    {
        LinkToSource = linkToSource;
        Source = source;
    }

    public string TackingId { get; set; }

    [Required]
    public Source LinkToSource { get; set; }

    [Required]
    public Source Source { get; set; }
}

public class UnLinkSourcesRequest : BaseRequest
{
    public UnLinkSourcesRequest(Guid requestId, string trackingId, Source unlInkFromSource, Source source)
        : base(requestId, trackingId)
    {
        UnlinkFromSource = unlInkFromSource;
        Source = source;
    }

    [Required]
    public Source UnlinkFromSource { get; set; }

    [Required]
    public Source Source { get; set; }
}

public class MergingSourcesRequest : BaseRequest
{
    public MergingSourcesRequest(Guid requestId, string trackingId, Source toSurviveSource, Source toRetireSource)
        : base(requestId, trackingId)
    {
        ToSurviveSource = toSurviveSource;
        ToRetireSource = toRetireSource;
    }

    [Required]
    public Source ToSurviveSource { get; set; }

    [Required]
    public Source ToRetireSource { get; set; }
}

public class UnMergingSourcesRequest : BaseRequest
{
    public UnMergingSourcesRequest(Guid requestId, string trackingId, Source unmergeFromSource, Source unmergeSource)
        : base(requestId, trackingId)
    {
        UnmergeFromSource = unmergeFromSource;
        UnmergeSource = unmergeSource;
    }

    [Required]
    public Source UnmergeFromSource { get; set; }

    [Required]
    public Source UnmergeSource { get; set; }
}

public class DemographicSearchRequest : BaseRequest
{
    public DemographicSearchRequest(Guid requestId, string trackingId, IdentityFilter filter)
        : base(requestId, trackingId)
    {
        Filter = filter;
    }

    [Required]
    public IdentityFilter Filter { get; set; }
}