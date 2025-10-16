using HCA.Models.Verato.Request;

namespace HCA.Models.Verato.Request;

/// <summary>
/// Link Identities Request
/// </summary>
public class LinkIdentitiesRequest : VeratoRequest
{
    /// <summary>
    /// <see cref="LinkIdentitiesRequest"/>
    /// </summary>
    /// <param name="trackingId">Tracking id for the request</param>
    public LinkIdentitiesRequest(string trackingId, LinkingSources content) : base(trackingId)
    {
        Content = content;
    }

    /// <summary>
    /// Linking Sources
    /// </summary>
    public LinkingSources Content { get; set; }
}

public class DeleteIdentyRequest : VeratoRequest
{
    /// <summary>
    /// <see cref="LinkIdentitiesRequest"/>
    /// </summary>
    /// <param name="trackingId">Tracking id for the request</param>
    public DeleteIdentyRequest(string trackingId, DeleteIdentyRequestContent content) : base(trackingId)
    {
        Content = content;
    }

    /// <summary>
    /// Linking Sources
    /// </summary>
    public DeleteIdentyRequestContent Content { get; set; }
}

public class DeleteIdentyRequestContent
{

    public DeleteIdentyRequestContent(Source source)
    {
        Source = source;
    }

    public Source Source { get; set; }
}

