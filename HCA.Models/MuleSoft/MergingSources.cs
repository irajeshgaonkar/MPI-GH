namespace HCA.Models.MuleSoft;

public class MergingSources
{
    public MergingSources(Source toSurviveSource, Source toRetireSource)
    {
        ToSurviveSource = toSurviveSource;
        ToRetireSource = toRetireSource;
    }

    public Source ToSurviveSource { get; set; }

    public Source ToRetireSource { get; set; }
}

