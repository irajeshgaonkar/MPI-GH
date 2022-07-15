using System;
namespace HCA.Models.MuleSoft;

public class UnLinkingSources
{
    public UnLinkingSources(Source unlInkFromSource, Source source)
    {
        UnlinkFromSource = unlInkFromSource;
        Source = source;
    }

    public Source UnlinkFromSource { get; set; }

    public Source Source { get; set; }
}

