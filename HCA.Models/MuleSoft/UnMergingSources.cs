namespace HCA.Models.MuleSoft;

public class UnMergingSources
{
    public UnMergingSources(Source unmergeFromSource, Source unmergeSource)
    {
        UnmergeFromSource = unmergeFromSource;
        UnmergeSource = unmergeSource;
    }

    public Source UnmergeFromSource { get; set; }

    public Source UnmergeSource { get; set; }
}

