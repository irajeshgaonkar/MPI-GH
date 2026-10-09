namespace HCA.Api.Dto;

public class PagenatedCollection<T>
{
    public int RecordsCount { get; set; }

    public int PageNumber { get; set; }

    public int RecordsPerPage { get; set; }

    public IEnumerable<T> Data { get; set; }
}

public class PagenatedDictionary<T>
{
    public int RecordsCount { get; set; }

    public int PageNumber { get; set; }

    public int RecordsPerPage { get; set; }

    public T Data { get; set; }
}