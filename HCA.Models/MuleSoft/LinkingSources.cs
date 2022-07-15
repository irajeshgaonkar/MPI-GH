using System;
namespace HCA.Models.MuleSoft;

public class LinkingSources
{
    public LinkingSources(Source linkToSource, Source source)
    {
        LinkToSource = linkToSource;
        Source = source;
    }

    public Source LinkToSource { get; set; }

    public Source Source { get; set; }
}

